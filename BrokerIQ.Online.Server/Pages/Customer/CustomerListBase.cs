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

        public int BrokerId { get; set; }

        public int AssignedToId { get; set; }

        public bool SelectFilled { get; set; }

        protected string allNotification;
        protected string selectedNotification;
        protected string SearchTerm { get; set; } = "";

        public int FilterRecent { get; set; }
        public int FilterPeriod { get; set; }
        public int CustomerCategory { get; set; }
        public int AgeRange { get; set; }

        protected int? ProfilingOption { get; set; }

        protected Dictionary<int, string> EmployeeColour { get; set; } = new Dictionary<int, string>();

        //filter
        protected List<Customer> FilteredCustomers => Customers.Where(i => !string.IsNullOrEmpty(i.Name) && i.Name.ToLower().Contains(SearchTerm.ToLower())).ToList();

        public CustomerCategoryEnum[] CustomerCategoriesByRelevance;

        public bool HasFilter { get; set; }

        public bool CanImport { get; set; }

        protected int showNonAppUsersOnlyAsInt;

        protected bool showNonAppUsersOnly;

        protected async void ShowNonAppUsersOnly()
        {
            showNonAppUsersOnly = !showNonAppUsersOnly;
            showNonAppUsersOnlyAsInt = showNonAppUsersOnly ? 1 : 0;
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
                SelectFilled = false;
                await GetCustomers();
                User = await AccountService.GetUser();

                CustomerCategoriesByRelevance = Extensions.GetAllCustomerCategories();

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
                        CustomerCategoriesByRelevance = Extensions.GetFilteredCustomerCategories(new int[] { 0, 2 });
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

        protected async Task GetCustomers(bool clear = false)
        {
            try
            {
                Customers = (await CustomerService.GetAllCustomers(profilePictures: true)).OrderByDescending(x => x.Id).ToList();
            }
            catch
            {
                NavigationManager.NavigateTo($"account/logout");
            }

            if (clear)
            {
                StateHasChanged();
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
                NavigationManager.NavigateTo($"clientdetail/{customer.Id}");
            }
        }

        protected async Task AutoCompleteClickBroker()
        {
            Customers.Clear();
            Customers = null;
            Customers = (await CustomerService.GetAllCustomers(BrokerId, FilterRecent, FilterPeriod, CustomerCategory, AgeRange, profilePictures: true)).ToList();

            if (SelectedCustomers != null && SelectedCustomers.Any())
            {
                SelectedCustomers.Clear();
            }

            StateHasChanged();
        }

        protected async Task RefreshListFromFilterValues()
        {
            Customers.Clear();
            Customers = null;

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

            if (SelectedCustomers != null && SelectedCustomers.Any())
            {
                SelectedCustomers.Clear();
            }

            StateHasChanged();
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

        protected async Task SendNotificationToSelected()
        {
            var dialogParams = new DialogParameters();
            if (string.IsNullOrEmpty(selectedNotification))
            {
                dialogParams.Add("Message", $"Please enter a notification to send.");
                await DialogService.Show<AlertDialog>("Send Notification", dialogParams).Result;
                return;
            }

            if (selectedNotification.Length > 299)
            {
                dialogParams.Add("Message", $"Your notification is too long. It needs to be less than 300 letters.");
                await DialogService.Show<AlertDialog>("Send Notification", dialogParams).Result;
                return;
            }
            dialogParams.Add("Notification", selectedNotification);

            var targetsName = new List<string>();
            if (SelectedCustomers != null)
            {
                targetsName = SelectedCustomers.Where(x => x.MarketingMessagesAllowed && x.EmailConfirmed).Select(x => x.Name).ToList();
            }

            if (targetsName == null && !targetsName.Any())
            {
                AlertService.Error("No targets chosen or marketing for those targets not allowed");
            };

            //var longlist = string.Join(",", targets);

            dialogParams.Add("Users", targetsName);
            dialogParams.Add("areBrokers", false);
            var result = await DialogService.Show<ScrollableDialog>("Send Notification", dialogParams).Result;

            if (!result.Canceled)
            {
                var targets = new List<int>();
                if (SelectedCustomers != null)
                {
                    targets = SelectedCustomers.Where(x => x.MarketingMessagesAllowed && x.EmailConfirmed).Select(x => x.Id).ToList();
                }

                if (targets != null && targets.Any())
                {
                    var user = await AccountService.GetUser();

                    var succeeded = false;
                    try
                    {
                        succeeded = await NotificationService.SendMessageNotification(selectedNotification, targets, user.MasterBrokerId);
                    }
                    catch
                    {

                    }

                    if (succeeded)
                    {
                        AlertService.Alert(new AlertBIQ
                        {
                            AutoClose = true,
                            Message = "Notification Sent"
                        });
                    }
                    else
                    {
                        AlertService.Error("Notification sending failed");
                    };
                }
                else
                {
                    AlertService.Error("No targets chosen or marketing for those targets not allowed");
                };
            }
        }

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
    }
}
