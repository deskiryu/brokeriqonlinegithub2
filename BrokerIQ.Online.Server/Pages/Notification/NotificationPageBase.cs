using Microsoft.AspNetCore.Components;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Enumuration;
using BrokerIQ.Online.Server.Models;
using BrokerIQ.Online.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrokerIQ.Online.Server.Extensions;
using Microsoft.JSInterop;
using BrokerIQ.Online.Server.Services;
using BrokerIQ.Online.Server.Shared;
using MudBlazor;

namespace BrokerIQ.Online.Pages
{
    public class NotificationPageBase : ComponentBase
    {
        [Inject]
        public INotificationService NotificationService { get; set; }

        [Inject]
        public ICustomerService CustomerService { get; set; }

        [Inject]
        public IBrokerStaffService BrokerStaffService { get; set; }

        [Inject]
        public IBrokerService BrokerService { get; set; }

        [Inject]
        public IAlertService AlertService { get; set; }

        [Inject]
        public IAccountService AccountService { get; set; }

        [Inject]
        public IVideoService VideoService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        protected IJSRuntime js { get; set; }

        public List<Notification> NotificationsSent { get; set; }

        public List<Notification> NotificationsSentBase { get; set; }

        public List<Notification> NotificationsSentByType { get; set; }

        public List<Customer> Customers { get; set; }

        public Customer Customer { get; set; }

        public List<BrokerStaff> BrokerStaff { get; set; }

        public string SearchTerm { get; set; } = "";

        public bool IsAdmin { get; set; }

        public int BrokerId { get; set; }

        public List<Broker> Brokers { get; set; }

        public bool ShowEmployee { get; set; }

        public bool ShowBroker { get; set; }

        public int FilterType { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                IsAdmin = false;
                Customers = (await CustomerService.GetAllCustomers(profilePictures:false)).ToList();
                var user = await this.AccountService.GetUser();
                ShowEmployee = false;
                ShowBroker = false;
                if (user.IsBroker || user.IsBrokerStaff)
                {
                    if (user.IsBroker)
                    {
                        BrokerStaff = (await BrokerStaffService.GetBrokerStaffbyBrokerId(user.MasterBrokerId)).ToList();
                        ShowEmployee = true;
                        BrokerId = user.MasterBrokerId;
                    }

                    NotificationsSentBase = (await NotificationService.GetNotificationByBrokerId(user.MasterBrokerId)).ToList();
                    FillBrokerStaff();
                    NotificationsSentByType = NotificationsSent = NotificationsSentBase;
                    Brokers = new List<Broker>();
                }
                else if ( user.IsAdmin)
                {
                    IsAdmin = true;
                    ShowBroker = true;
                    Brokers = (await BrokerService.GetBrokers()).ToList();
                    BrokerStaff = (await BrokerStaffService.GetBrokerStaff()).ToList();
                    NotificationsSentBase = (await NotificationService.GetNotifications()).ToList();
                    FillBrokerStaff();
                    NotificationsSentByType = NotificationsSent = NotificationsSentBase;
                }
                else
                {
                    ShowEmployee = false;
                }
            }
            catch
            {
                NavigationManager.NavigateTo($"account/logout");
            }
        }

        public async Task<List<Customer>> GetCustomers(List<int> customerIds)
        {
            return (await CustomerService.GetCustomersByList(customerIds)).ToList();
        }

        protected async Task AutoCompleteClickBroker()
        {
            if (BrokerId == 0)
            {
                NotificationsSentByType = NotificationsSent = NotificationsSentBase;
            }
            else if (BrokerId > 0)
            {
                NotificationsSentByType = NotificationsSent = NotificationsSentBase.Where(x => x.BrokerId == BrokerId).ToList();
            }        
        }

        public async Task<IEnumerable<string>> OnFilter(string value)
        {
            if (!string.IsNullOrEmpty(value) && Customers != null && Customers.Any())
            {
                // In real life use an asynchronous function for fetching data from an api.
                var filtered = Customers.Where(i => !string.IsNullOrEmpty(i.Name) && i.Name.ToLower().Contains(value.ToLower()) ||
                !string.IsNullOrEmpty(i.EmailAddress) && i.EmailAddress.ToLower().Contains(value.ToLower()) ||
                !string.IsNullOrEmpty(i.TelephoneNumber) && i.TelephoneNumber.ToLower().Contains(value.ToLower()));
                return await Task.FromResult(filtered.Select(x => x.Name).Distinct().ToList());
            }
            else
            {
                return new List<string>();
            }

        }

        public async Task AutoCompleteClick(string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                NotificationsSent = NotificationsSentBase;

            }
            else
            {
                Customer = Customers.FirstOrDefault(x => x.Name == args);

                if (Customer != null)
                {

                    try
                    {
                        NotificationsSent = NotificationsSentBase.Where(x => x.Targets.Contains(Customer.Id)).ToList();
                        RecentTypeSelect();
                    }
                    catch
                    {
                        AlertService.Error("Get Notifications failed");
                    }
                }
                else
                {
                    NotificationsSent = NotificationsSentBase;
                }
            }
            RecentTypeSelect();
        }

        private void FillBrokerStaff()
        {
            foreach (var notif in NotificationsSentBase)
            {
                notif.BrokerStaffName = "-";
                notif.BrokerName = "-";
                if (notif.BrokerStaffId != null && BrokerStaff != null && BrokerStaff.Count > 0)
                {
                    var foundStaff = BrokerStaff.FirstOrDefault(x => x.Id == notif.BrokerStaffId);
                    if (foundStaff != null)
                    {
                        notif.BrokerStaffName = foundStaff.FirstName + " " + foundStaff.LastName;
                    }
                }
                if (IsAdmin)
                {
                    var foundBroker = Brokers.FirstOrDefault(x => x.Id == notif.BrokerId);
                    if (foundBroker != null)
                    {
                        notif.BrokerName = foundBroker.Name;
                    }
                }

            }
        }

        protected void RecentTypeSelect()
        {
            switch ((NotificationTypeEnum)FilterType)
            {
                case NotificationTypeEnum.Text:
                    NotificationsSentByType = NotificationsSent.Where(x => x.IsNotification).ToList(); break;
                case NotificationTypeEnum.Video:
                    NotificationsSentByType = NotificationsSent.Where(x => x.IsVideo).ToList(); break;
                case NotificationTypeEnum.Audio:
                    NotificationsSentByType = NotificationsSent.Where(x => x.IsAudio).ToList(); break;
                case NotificationTypeEnum.All:
                default:
                    NotificationsSentByType = NotificationsSent;
                    break;
            }
            
        }

        protected async Task ViewLink(string url)
        {
            var Videos = (await VideoService.GetVideos(BrokerId)).ToList();
            var video = Videos.FirstOrDefault(x => x.Url == url);
            if (video != null)
            {
                NavigationManager.NavigateTo($"videodetail/{video.Id}");
            }     
        }
    }
}
