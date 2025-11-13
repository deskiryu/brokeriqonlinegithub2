using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Dto.Enum;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Extensions;
using BrokerIQ.Online.Server.Models;
using BrokerIQ.Online.Server.Pages.Customer.Components;
using BrokerIQ.Online.Server.Shared;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Pages
{
    public class CustomerListBase : ComponentBase
    {
        [Inject]
        public ICustomerService CustomerService { get; set; }

        [Inject]
        public IBrokerService BrokerService { get; set; }

        [Inject]
        public IBrokerStaffService BrokerStaffService { get; set; }

        [Inject]
        INotificationService NotificationService { get; set; }

        [Inject]
        NavigationManager NavigationManager { get; set; }

        [Inject]
        IAlertService AlertService { get; set; }

        [Inject]
        IDialogService DialogService { get; set; }

        [Inject]
        IAccountService AccountService { get; set; }

        [Inject]
        ISnackbar Snackbar { get; set; }

        public User User { get; set; }

        public List<Customer> Customers { get; set; }

        public List<Broker> Brokers { get; set; }

        public List<BrokerStaff> Employees { get; set; }

        public HashSet<Customer> SelectedCustomers { get; set; } = new HashSet<Customer>();

        protected bool HasSelectedCustomers => SelectedCustomers?.Any() == true;

        public int BrokerId { get; set; }

        public int AssignedToId { get; set; }

        public bool SelectFilled { get; set; }

        protected string SearchTerm { get; set; } = "";

        public int FilterRecent { get; set; }
        public int FilterPeriod { get; set; }
        public int CustomerCategory { get; set; }
        public int AgeRange { get; set; }

        protected int? ProfilingOption { get; set; }

        protected Dictionary<int, string> EmployeeColour { get; set; } = new Dictionary<int, string>();

        protected CustomerNameSortOption NameSortOption { get; set; } = CustomerNameSortOption.Default;

        //filter
        protected List<Customer> FilteredCustomers => Customers.Where(i => i.ConnectedToCustomerId == null &&
            ((!string.IsNullOrWhiteSpace(i.Name) && i.Name.ToLower().Contains(SearchTerm.ToLower())) ||
            (!string.IsNullOrWhiteSpace(i.BusinessName) && i.BusinessName.ToLower().Contains(SearchTerm.ToLower())))).ToList();

        public CustomerCategoryEnum[] CustomerCategoriesByRelevance;

        public bool HasFilter { get; set; }

        public bool CanImport { get; set; }

        protected int showNonAppUsersOnlyAsInt;

        protected bool showNonAppUsersOnly;

        protected bool FiltersDialogVisible { get; set; }

        protected bool HasActiveFilterChips =>
            (User?.IsAdmin == true && BrokerId > 0) ||
            AssignedToId > 0 ||
            showNonAppUsersOnly ||
            !string.IsNullOrWhiteSpace(SearchTerm) ||
            FilterRecent > 0 ||
            CustomerCategory > 0 ||
            AgeRange > 0 ||
            FilterPeriod > 0 ||
            ProfilingOption.HasValue;

        protected async void ShowNonAppUsersOnly()
        {
            showNonAppUsersOnly = !showNonAppUsersOnly;
            showNonAppUsersOnlyAsInt = showNonAppUsersOnly ? 1 : 0;
            await RefreshListFromFilterValues();
        }

        protected void OpenFiltersDialog() => FiltersDialogVisible = true;

        protected void CloseFiltersDialog() => FiltersDialogVisible = false;

        protected string GetBrokerName(int brokerId) =>
            Brokers?.FirstOrDefault(b => b.Id == brokerId)?.Name ?? "Broker";

        protected string GetEmployeeName(int employeeId) =>
            Employees?.FirstOrDefault(e => e.Id == employeeId)?.FullName ?? "Team member";

        protected string GetCustomerCategoryLabel()
        {
            if (CustomerCategory <= 0) return string.Empty;
            if (!Enum.IsDefined(typeof(CustomerCategoryEnum), CustomerCategory)) return string.Empty;
            return ((CustomerCategoryEnum)CustomerCategory).GetDisplayName();
        }

        protected string GetAgeRangeLabel()
        {
            if (AgeRange <= 0) return string.Empty;
            if (!Enum.IsDefined(typeof(AgeRangeEnum), AgeRange)) return string.Empty;
            return ((AgeRangeEnum)AgeRange).GetDisplayName();
        }

        protected string GetUpcomingFilterLabel() => FilterRecent switch
        {
            1 => "Insurance renewal",
            2 => "Mortgage renewal",
            _ => string.Empty
        };

        protected string GetPeriodLabel()
        {
            var labels = new[] { "2 weeks", "4 weeks", "3 months", "6 months", "9 months" };
            if (FilterPeriod >= 0 && FilterPeriod < labels.Length)
            {
                return labels[FilterPeriod];
            }
            return string.Empty;
        }

        protected string GetProfilingLabel()
        {
            if (!ProfilingOption.HasValue) return string.Empty;
            if (!Enum.IsDefined(typeof(ProfilingOptionEnum), ProfilingOption.Value)) return string.Empty;
            return ((ProfilingOptionEnum)ProfilingOption.Value).GetDisplayName();
        }

        protected async Task ClearBrokerFilter()
        {
            if (BrokerId == 0) return;
            BrokerId = 0;
            await AutoCompleteClickBroker();
        }

        protected async Task ClearAssigneeFilter()
        {
            if (AssignedToId == 0) return;
            AssignedToId = 0;
            await RefreshListFromFilterValues();
        }

        protected async Task ClearUsageFilter()
        {
            if (!showNonAppUsersOnly) return;
            showNonAppUsersOnly = false;
            showNonAppUsersOnlyAsInt = 0;
            await RefreshListFromFilterValues();
        }

        protected Task ClearSearchFilter()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm)) return Task.CompletedTask;
            SearchTerm = string.Empty;
            StateHasChanged();
            return Task.CompletedTask;
        }

        protected async Task ClearUpcomingFilter()
        {
            if (FilterRecent == 0) return;
            FilterRecent = 0;
            await RefreshListFromFilterValues();
        }

        protected async Task ClearCategoryFilter()
        {
            if (CustomerCategory == 0) return;
            CustomerCategory = 0;
            await RefreshListFromFilterValues();
        }

        protected async Task ClearAgeRangeFilter()
        {
            if (AgeRange == 0) return;
            AgeRange = 0;
            await RefreshListFromFilterValues();
        }

        protected async Task ClearPeriodFilter()
        {
            if (FilterPeriod == 0) return;
            FilterPeriod = 0;
            await RefreshListFromFilterValues();
        }

        protected async Task ClearProfilingFilter()
        {
            if (!ProfilingOption.HasValue) return;
            ProfilingOption = null;
            await RefreshListFromFilterValues();
        }

        protected override async Task OnInitializedAsync()
        {
            await GetCustomersInit();
        }

        protected async Task GetCustomersInit()
        {
            try
            {
                await ShowCustomerLoadingIndicatorAsync();
                SelectFilled = false;
                await GetCustomers();
                User = await AccountService.GetUser();

                CustomerCategoriesByRelevance = ExtensionClass.GetAllCustomerCategories();

                if (User.IsAdmin)
                {
                    Brokers = (await BrokerService.GetBrokers()).ToList();
                    Employees = new List<BrokerStaff>();
                }
                else
                {
                    Brokers = new List<Broker>();
                    BrokerId = User.MasterBrokerId;

                    var broker = await BrokerService.GetBroker(User.MasterBrokerId);

                    HasFilter = broker.HasFilter;
                    CanImport = broker.CanImport;

                    if (broker.BrokerIdentifier.InsuranceOnly)
                    {
                        CustomerCategoriesByRelevance = ExtensionClass.GetFilteredCustomerCategories(new int[] { 0, 2 });
                    }

                    await RefreshEmployees();
                }
            }
            catch
            {
                NavigationManager.NavigateTo($"account/logout");
            }
        }

        private async Task RefreshEmployees()
        {
            Employees = (await BrokerStaffService.GetBrokerStaffbyBrokerId(BrokerId)).ToList();
            GenerateEmployeeColours();
        }

        private void GenerateEmployeeColours()
        {
            string[] colourValues = new string[] {
                Colors.Red.Lighten3, Colors.DeepPurple.Lighten3, Colors.LightBlue.Lighten3, Colors.Green.Lighten3, Colors.Yellow.Lighten3, Colors.DeepOrange.Lighten3, Colors.Grey.Lighten3,
                Colors.Pink.Lighten3, Colors.Indigo.Lighten3, Colors.Cyan.Lighten3, Colors.LightGreen.Lighten3, Colors.Amber.Lighten3, Colors.Brown.Lighten3, Colors.Purple.Lighten3,
                Colors.Blue.Lighten3, Colors.Teal.Lighten3, Colors.Lime.Lighten3, Colors.Orange.Lighten3, Colors.BlueGrey.Lighten3,
                Colors.Red.Lighten1, Colors.DeepPurple.Lighten1, Colors.LightBlue.Lighten1, Colors.Green.Lighten1, Colors.Yellow.Lighten1, Colors.DeepOrange.Lighten1, Colors.Grey.Lighten1,
                Colors.Pink.Lighten1, Colors.Indigo.Lighten1, Colors.Cyan.Lighten1, Colors.LightGreen.Lighten1, Colors.Amber.Lighten1, Colors.Brown.Lighten1, Colors.Purple.Lighten1,
                Colors.Blue.Lighten1, Colors.Teal.Lighten1, Colors.Lime.Lighten1, Colors.Orange.Lighten1, Colors.BlueGrey.Lighten1,
                Colors.Red.Accent3, Colors.DeepPurple.Accent3, Colors.LightBlue.Accent3, Colors.Green.Accent3, Colors.Yellow.Accent3, Colors.DeepOrange.Accent3,
                Colors.Pink.Accent3, Colors.Indigo.Accent3, Colors.Cyan.Accent3, Colors.LightGreen.Accent3, Colors.Amber.Accent3, Colors.Purple.Accent3,
                Colors.Blue.Accent3, Colors.Teal.Accent3, Colors.Lime.Accent3, Colors.Orange.Accent3
            };

            var colourIndex = 0;
            EmployeeColour.Clear();

            foreach (var member in Employees)
            {
                EmployeeColour.Add(member.Id, colourValues[colourIndex++]);

                if (colourIndex > colourValues.Length) colourIndex = 0;
            }
        }

        private Task ShowCustomerLoadingIndicatorAsync()
        {
            Customers?.Clear();
            Customers = null;
            return InvokeAsync(StateHasChanged);
        }

        protected async Task GetCustomers(bool clear = false)
        {
            try
            {
                Customers = (await CustomerService.GetAllCustomers(profilePictures: true)).ToList();
                ApplyCustomerSort();
            }
            catch
            {
                NavigationManager.NavigateTo($"account/logout");
            }

            if (clear)
            {
                await InvokeAsync(StateHasChanged);
            }
        }

        protected async Task<IEnumerable<string>> OnFilter(string value)
        {
            if (!string.IsNullOrEmpty(value) && Customers != null && Customers.Any())
            {
                // In real life use an asynchronous function for fetching data from an api.
                var filtered = Customers
                    .Where(
                            i => !string.IsNullOrEmpty(i.Name) && i.Name.ToLower().Contains(value.ToLower()) ||
                                 !string.IsNullOrEmpty(i.EmailAddress) && i.EmailAddress.ToLower().Contains(value.ToLower()) ||
                                 !string.IsNullOrEmpty(i.TelephoneNumber) && i.TelephoneNumber.ToLower().Contains(value.ToLower())
                            );
                var results = await Task.FromResult(filtered.Select(x => x.Name).Distinct().ToList());
                return results;
            }
            else
            {
                return new List<string>();
            }
        }

        protected async Task<IEnumerable<string>> OnFilterBroker(string value)
        {
            if (Brokers != null && Brokers.Any())
            {
                // In real life use an asynchronous function for fetching data from an api.
                IEnumerable<Broker> filtered = null;
                if (string.IsNullOrEmpty(value))
                {
                    filtered = Brokers;
                }
                else
                {
                    filtered = Brokers.Where(i => !string.IsNullOrEmpty(i.Name) && i.Name.ToLower().Contains(value.ToLower()) ||
                    !string.IsNullOrEmpty(i.EmailAddress) && i.EmailAddress.ToLower().Contains(value.ToLower()));
                }


                var results = await Task.FromResult(filtered.Select(x => x.Name).Distinct().ToList());
                return results;
            }
            else
            {
                return new List<string>();
            }
        }

        protected void AutoCompleteClick(string args)
        {
            var customer = Customers.FirstOrDefault(x => x.Name == args);
            if (customer != null)
            {
                var id = customer.ConnectedToCustomerId.HasValue ? customer.ConnectedToCustomerId : customer.Id;
                NavigationManager.NavigateTo($"clientdetail/{id}");
            }
        }

        protected async Task AutoCompleteClickBroker()
        {
            await ShowCustomerLoadingIndicatorAsync();
            Customers = (await CustomerService.GetAllCustomers(BrokerId, FilterRecent, FilterPeriod, CustomerCategory, AgeRange, profilePictures: true)).ToList();
            ApplyCustomerSort();

            if (SelectedCustomers != null && SelectedCustomers.Any())
            {
                SelectedCustomers.Clear();
            }

            await InvokeAsync(StateHasChanged);
        }

        protected async Task RefreshListFromFilterValues()
        {
            await ShowCustomerLoadingIndicatorAsync();

            var filterValues = new CustomerFilter()
            {
                BrokerId = BrokerId,
                AssignedToId = AssignedToId,
                Recent = FilterRecent,
                Period = FilterPeriod,
                Category = CustomerCategory,
                AgeRange = AgeRange,
                ProfilePictures = true,
                ProfilingOption = ProfilingOption.HasValue ? (ProfilingOptionEnum)ProfilingOption : null,
                NonAppUsersOnly = showNonAppUsersOnly
            };

            Customers = (await CustomerService.GetFilteredCustomers(filterValues)).ToList();
            ApplyCustomerSort();

            if (SelectedCustomers != null && SelectedCustomers.Any())
            {
                SelectedCustomers.Clear();
            }

            await InvokeAsync(StateHasChanged);
        }

        protected async Task SendChatMessageToSelected()
        {
            var dialogParams = new DialogParameters();

            var targets = new List<(string, int)>();
            if (SelectedCustomers != null && SelectedCustomers.Any())
            {
                targets = SelectedCustomers.Where(x => x.MarketingMessagesAllowed && x.EmailConfirmed == true).Select(x => (x.Name, x.Id)).ToList();
            }

            if (targets == null || !targets.Any())
            {
                AlertService.Error("No targets chosen");
                return;
            }

            if (User.IsAdmin)
            {
                if (BrokerId <= 0)
                {
                    AlertService.Error("Please filter by broker first");
                    return;
                }
            }

            dialogParams.Add("BrokerId", BrokerId);
            dialogParams.Add("Customers", targets);

            var dialogOptions = new DialogOptions()
            {
                MaxWidth = MaxWidth.Medium,
                FullWidth = true
            };

            await DialogService.Show<MultipleChatDialog>("Send Chat To Multiple", dialogParams, dialogOptions).Result;
        }

        protected async Task HandleSendChatOption() => await SendChatMessageToSelected();

        protected async Task SendNotificationToSelected()
        {
            if (SelectedCustomers == null || !SelectedCustomers.Any())
            {
                AlertService.Error("No targets chosen or marketing for those targets not allowed");
                return;
            }

            var eligibleCustomers = SelectedCustomers
                .Where(x => x.MarketingMessagesAllowed && x.EmailConfirmed)
                .ToList();

            if (!eligibleCustomers.Any())
            {
                AlertService.Error("No targets chosen or marketing for those targets not allowed");
                return;
            }

            var masterBrokerId = User?.MasterBrokerId ?? 0;
            if (masterBrokerId <= 0)
            {
                var user = await AccountService.GetUser();
                masterBrokerId = user?.MasterBrokerId ?? 0;
            }

            if (masterBrokerId <= 0)
            {
                AlertService.Error("Unable to determine broker context for sending notifications.");
                return;
            }

            var dialogParams = new DialogParameters
            {
                { "CustomerNames", eligibleCustomers.Select(x => x.Name).ToList() },
                { "CustomerIds", eligibleCustomers.Select(x => x.Id).ToList() },
                { "MasterBrokerId", masterBrokerId }
            };

            var dialogOptions = new DialogOptions()
            {
                MaxWidth = MaxWidth.Medium,
                FullWidth = true
            };

            await DialogService.Show<MultipleNotificationDialog>("Send Notification", dialogParams, dialogOptions).Result;
        }

        protected async Task HandleSendNotificationOption() => await SendNotificationToSelected();

        protected async Task OnCategoryClick(int customerId, int newCategory)
        {
            await CustomerService.SetCustomerCategory(customerId, (CustomerCategoryEnum)newCategory);

            await GetCustomers();
        }

        protected string GetCategoryDisplayName(Customer c)
        {
            return c.CustomerCategory.GetDisplayName();
        }

        protected string AssignVisibilityClass()
        {
            if (User.IsBrokerStaff || BrokerId == 0) return "invisible";

            return SelectedCustomers != null && SelectedCustomers.Count > 0 ? "visible" : "invisible";
        }

        protected async Task AssignToStaff()
        {
            var dialogParams = new DialogParameters
            {
                { "BrokerId", BrokerId},
                { "SelectedCustomerIds", SelectedCustomers.Select(c => c.Id).ToArray() }
            };

            var result = await DialogService.Show<AssignmentDialog>("Assign to Employee", dialogParams).Result;

            if (!result.Canceled)
            {
                SelectedCustomers = null;
                await GetCustomers();
                await RefreshEmployees();
                StateHasChanged();
            }
        }

        protected async Task ShowImportDialog()
        {
            var dialogParams = new DialogParameters
            {
                { "User", User },
                { "BrokerId", BrokerId},
            };

            var dialogOptions = new DialogOptions() { MaxWidth = MaxWidth.Large, FullWidth = true };

            await DialogService.Show<CustomerImportDialog>("Import customers", dialogParams, dialogOptions).Result;

            await GetCustomers();

            StateHasChanged();
        }

        protected async Task SendInvite()
        {
            var toInvite = SelectedCustomers.Where(c => string.IsNullOrWhiteSpace(c.AppVersion)).ToArray();

            if (!toInvite.Any())
            {
                var noCustomersParams = new DialogParameters
                {
                    { "Message", "All selected customers have downloaded the application already." }
                };
                await DialogService.Show<AlertDialog>("Invalid selection", noCustomersParams).Result;

                return;
            }

            var dialogParams = new DialogParameters
            {
                { "SelectedCustomers", toInvite }
            };

            var result = await DialogService.Show<BulkActionConfirmationDialog>("Import customers", dialogParams).Result;

            if (!result.Canceled)
            {
                var customerIds = toInvite.Select(c => c.Id).ToArray();

                var wasSuccessfull = await CustomerService.SendAppInvites(customerIds);

                SelectedCustomers = new HashSet<Customer>();

                Snackbar.Add(wasSuccessfull ? "Emails sent successfully" : "Some emails failed", wasSuccessfull ? Severity.Success : Severity.Warning);
            }
        }

        protected async Task SortCustomersByNameAscending()
        {
            NameSortOption = CustomerNameSortOption.Ascending;
            ApplyCustomerSort();
            await InvokeAsync(StateHasChanged);
        }

        protected async Task SortCustomersByNameDescending()
        {
            NameSortOption = CustomerNameSortOption.Descending;
            ApplyCustomerSort();
            await InvokeAsync(StateHasChanged);
        }

        protected async Task ResetCustomerSort()
        {
            NameSortOption = CustomerNameSortOption.Default;
            ApplyCustomerSort();
            await InvokeAsync(StateHasChanged);
        }

        protected string GetSortMenuItemClass(CustomerNameSortOption option)
        {
            var cssClass = "customer-sort-menu__item";

            if (NameSortOption == option)
            {
                cssClass += " customer-sort-menu__item--active";
            }

            return cssClass;
        }

        private void ApplyCustomerSort()
        {
            if (Customers == null || !Customers.Any())
            {
                return;
            }

            switch (NameSortOption)
            {
                case CustomerNameSortOption.Ascending:
                    Customers = Customers
                        .OrderBy(customer => customer.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                    break;
                case CustomerNameSortOption.Descending:
                    Customers = Customers
                        .OrderByDescending(customer => customer.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                    break;
                default:
                    Customers = Customers
                        .OrderByDescending(customer => customer.Id)
                        .ToList();
                    break;
            }
        }

        protected enum CustomerNameSortOption
        {
            Default,
            Ascending,
            Descending
        }
    }
}
