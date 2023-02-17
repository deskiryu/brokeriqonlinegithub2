using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Pages
{
    using BrokerIQ.Online.Server.Shared;
    using Dto.Models;
    using Microsoft.AspNetCore.Components;
    using Models;
    using MudBlazor;
    using Services.Interface;
    using System.Globalization;

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

        protected async Task GetCustomersInit()
        {
            try
            {
                SelectFilled = false;
                await GetCustomers();
                var user = await AccountService.GetUser();
                IsAdmin = user.IsAdmin;
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

        protected async Task GetCustomers(bool clear=false)
        {
            try
            {
                Customers = (await CustomerService.GetAllCustomers(profilePictures:true)).OrderByDescending(x => x.Id).ToList();
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
                var filtered = Customers.Where(i => i.Name.ToLower().Contains(value.ToLower()) ||
                i.EmailAddress.ToLower().Contains(value.ToLower()) ||
                i.TelephoneNumber.ToLower().Contains(value.ToLower()));
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
                    filtered = Brokers.Where(i => i.Name.ToLower().Contains(value.ToLower()) ||
                    i.EmailAddress.ToLower().Contains(value.ToLower()));
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
            Customers = (await CustomerService.GetAllCustomers(BrokerId, FilterRecent, FilterPeriod, CustomerCategory, AgeRange, profilePictures:true)).ToList();
            SelectedCustomers.Clear();
            StateHasChanged();
        }

        protected async Task RecentFilterSelect()
        {
            Customers.Clear();
            Customers = null;
            Customers = (await CustomerService.GetAllCustomers(BrokerId, FilterRecent, FilterPeriod, CustomerCategory, AgeRange, profilePictures: true)).ToList();
            SelectedCustomers.Clear();
            StateHasChanged();
        }

        

        protected async Task SendChatMessageToSelected()
        {
            var dialogParams = new DialogParameters();

            var targets = new List<(string,int)>();
            if(SelectedCustomers != null && SelectedCustomers.Any())
            {
                targets = SelectedCustomers.Where(x => x.EmailConfirmed == true).Select(x => (x.Name,x.Id)).ToList();
            }

            if (targets==null || !targets.Any())
            {
                AlertService.Error("No targets chosen");
                return;
            }

            if (IsAdmin)
            {
                if (BrokerId <= 0) {
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

    }
}
