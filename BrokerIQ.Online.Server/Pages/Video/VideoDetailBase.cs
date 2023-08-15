
namespace BrokerIQ.Online.Server.Pages.Video
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
    using BrokerIQ.Online.Models;
    using BrokerIQ.Online.Server.Models;
    using BrokerIQ.Online.Services.Interface;
    using MudBlazor;
    using Microsoft.AspNetCore.WebUtilities;
    using System.IO;
    using BrokerIQ.Online.Server.Shared;

    public class VideoDetailBase : ComponentBase
    {
        [Inject]
        protected ICustomerService CustomerService { get; set; }

        [Inject]
        protected NavigationManager NavigationManager{ get; set; }
        [Inject]
        protected INotificationService NotificationService{ get; set; }

        [Inject]
        protected IAlertService AlertService{ get; set; }

        [Inject]
        protected IDialogService DialogService{ get; set; }

        [Inject]
        public IVideoService VideoService { get; set; }

        [Inject]
        public IAccountService AccountService { get; set; }

        [Inject]
        public IBrokerService BrokerService { get; set; }


        protected Video Video { get; set; }

        protected List<Customer> Customers { get; set; }

        protected HashSet<Customer> SelectedCustomers { get; set; }

        public List<Broker> Brokers { get; set; }

        public int BrokerId { get; set; }

        protected string Url { get; set; }
        protected string VideoName { get; set; }
        protected string VideoNameNoExtension { get; set; }
        protected string selectedNotification;

        protected string SearchTerm { get; set; } = "";

        protected bool Vetted { get; set; }

        protected bool IsAdmin { get; set; }

        public int CustomerCategory { get; set; }
        public int AgeRange { get; set; }         

        //filter
        protected List<Customer> FilteredCustomers => Customers.Where(i => !string.IsNullOrEmpty(i.Name) && i.Name.ToLower().Contains(SearchTerm.ToLower())).ToList();

        protected override async Task OnInitializedAsync()
        {
            var query = new Uri(NavigationManager.Uri).Query;
            selectedNotification = "A new video has arrived";

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


            if (QueryHelpers.ParseQuery(query).TryGetValue("Url", out var value))
            {
                Url = value;
                try
                {
                    int pos = Url.LastIndexOf("/") + 1;
                    VideoName = Url.Substring(pos, Url.Length - pos);
                    VideoNameNoExtension = Path.GetFileNameWithoutExtension(VideoName);
                }
                catch
                {
                    AlertService.Error("Get Videos failed");
                }
            }
            
            try
            {
                if (user == null)
                {
                    throw new Exception();
                }

                Vetted = await VideoService.IsVetted(VideoName, user.MasterBrokerId);
                var queryCust = await CustomerService.GetAllCustomers();
                Customers = queryCust.Where(x =>x.VideoNotificationsAllowed==true).ToList();
            }
            catch
            {
                AlertService.Error("Get Videos failed");
            }
        }

        protected string currentValue;

        protected async Task SendNotificationToAll()
        {
            await OpenNotificationDialog(true);
        }

        protected async Task SendNotificationToSelected(bool sendAll = false)
        {
            await OpenNotificationDialog(false);
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

        protected async Task AutoCompleteClickBroker()
        {
            Customers.Clear();
            Customers = null;
            Customers = (await CustomerService.GetAllCustomers(BrokerId)).ToList();
        }

        protected async Task OpenNotificationDialog(bool sendAll = false)
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
            if (sendAll)
            {
                targetsName = Customers.Where(x => x.EmailConfirmed == true).Select(x => x.Name).ToList();
            }
            else
            {
                targetsName = SelectedCustomers.Where(x => x.EmailConfirmed == true).Select(x => x.Name).ToList();
            }


            dialogParams.Add("Users", targetsName);
            dialogParams.Add("areBrokers", false);
            var result = await DialogService.Show<ScrollableDialog>("Send Video Notification", dialogParams).Result;

            if (!result.Cancelled)
            {
                var targets = new List<int>();
                if (sendAll)
                {
                    targets = Customers.Where(x => x.EmailConfirmed == true).Select(x => x.Id).ToList();
                }
                else
                {
                    targets = SelectedCustomers.Where(x => x.EmailConfirmed == true).Select(x => x.Id).ToList();
                }

                if (targets != null && targets.Any())
                {
                    var succeeded = false;
                    try
                    {
                        succeeded = await NotificationService.SendVideoNotification(selectedNotification, Url, targets, sendAll);
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
        protected async Task RecentFilterSelect()
        {
            Customers.Clear();
            Customers = null;
            Customers = (await CustomerService.GetAllCustomers(BrokerId, filterCategory:CustomerCategory, filterAgeRange:AgeRange, profilePictures: false)).ToList();
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
            dialogParams.Add("VideoUrl", Url);
            dialogParams.Add("VideoName", VideoNameNoExtension);
            

            var dialogOptions = new DialogOptions()
            {
                MaxWidth = MaxWidth.Medium,
                FullWidth = true
            };

            await DialogService.Show<VideoChatDialog>("Send Video To Multiple Chats", dialogParams, dialogOptions).Result;
        }
    }
}
