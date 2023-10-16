using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Dto.Enum;
using BrokerIQ.Online.Server.Extensions;
using BrokerIQ.Online.Server.Models;
using BrokerIQ.Online.Server.Shared;
using Microsoft.AspNetCore.Components;
using BrokerIQ.Online.Models;
using MudBlazor;
using BrokerIQ.Online.Services.Interface;
using BrokerIQ.Online.Server.Pages.Customer.Components;

namespace BrokerIQ.Online.Pages
{
    public class CustomerListBase : ComponentBase
    {
        [Inject]
        public ICustomerService CustomerService { get; set; }

        [Inject]
        public IBrokerService BrokerService { get; set; }

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

        public List<Customer> Customers { get; set; }

        public List<Broker> Brokers { get; set; }

        public HashSet<Customer> SelectedCustomers { get; set; }

        public int BrokerId { get; set; }

        public bool SelectFilled { get; set; }

        protected string allNotification;
        protected string selectedNotification;
        protected string SearchTerm { get; set; } = "";

        protected bool IsAdmin { get; set; }
        public int FilterRecent { get; set; }
        public int FilterPeriod { get; set; }
        public int CustomerCategory { get; set; }
        public int AgeRange { get; set; }

        private bool hasNeeds;
        public bool HasNeeds
        {
            get { return hasNeeds; }
            set
            {
                hasNeeds = value;
                RefreshListFromFilterValues();
            }
        }

        private bool withoutIncomeProtection;
        public bool WithoutIncomeProtection
        {
            get { return withoutIncomeProtection; }
            set
            {
                withoutIncomeProtection = value;
                RefreshListFromFilterValues();
            }
        }

        private bool withoutLifeInsurance;
        public bool WithoutLifeInsurance
        {
            get { return withoutLifeInsurance; }
            set
            {
                withoutLifeInsurance = value;
                RefreshListFromFilterValues();
            }
        }

        private bool withoutLifeAndCritical;
        public bool WithoutLifeCritical
        {
            get { return withoutLifeAndCritical; }
            set
            {
                withoutLifeAndCritical = value;
                RefreshListFromFilterValues();
            }
        }

        //filter
        protected List<Customer> FilteredCustomers => Customers.Where(i => !string.IsNullOrEmpty(i.Name) && i.Name.ToLower().Contains(SearchTerm.ToLower())).ToList();

        public CustomerCategoryEnum[] CustomerCategoriesByRelevance;

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
                var user = await AccountService.GetUser();
                IsAdmin = user.IsAdmin;

                CustomerCategoriesByRelevance = Extensions.BuildCustomerCategoriesByRelevance();

                if (IsAdmin)
                {
                    Brokers = (await BrokerService.GetBrokers()).ToList();
                }
                else
                {
                    Brokers = new List<Broker>();
                    BrokerId = user.MasterBrokerId;
                }
            }
            catch
            {
                NavigationManager.NavigateTo($"account/logout");
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

            Customers = (await CustomerService.GetFilteredCustomers(new CustomerFilter()
            {
                BrokerId = BrokerId,
                Recent = FilterRecent,
                Period = FilterPeriod,
                Category = CustomerCategory,
                AgeRange = AgeRange,
                ProfilePictures = true,
                HasNeeds = HasNeeds,
                WithoutIncomeProtection = WithoutIncomeProtection,
                WithoutLifeInsurance = WithoutLifeInsurance,
                WithoutLifeCritical = WithoutLifeCritical
            })).ToList();

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
                targets = SelectedCustomers.Where(x => x.EmailConfirmed == true).Select(x => (x.Name, x.Id)).ToList();
            }

            if (targets == null || !targets.Any())
            {
                AlertService.Error("No targets chosen");
                return;
            }

            if (IsAdmin)
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
            if (SelectedCustomers != null && SelectedCustomers.Any())
            {
                targetsName = SelectedCustomers.Where(x => x.EmailConfirmed == true).Select(x => x.Name).ToList();
            }

            if (targetsName == null && !targetsName.Any())
            {
                AlertService.Error("No targets chosen");
            };

            //var longlist = string.Join(",", targets);

            dialogParams.Add("Users", targetsName);
            dialogParams.Add("areBrokers", false);
            var result = await DialogService.Show<ScrollableDialog>("Send Notification", dialogParams).Result;

            if (!result.Cancelled)
            {
                var targets = new List<int>();
                if (SelectedCustomers != null && SelectedCustomers.Any())
                {
                    targets = SelectedCustomers.Where(x => x.EmailConfirmed == true).Select(x => x.Id).ToList();
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
                    AlertService.Error("No targets chosen");
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

            if (!result.Cancelled)
            {
                SelectedCustomers = null;

                StateHasChanged();
            }
        }
    }
}
