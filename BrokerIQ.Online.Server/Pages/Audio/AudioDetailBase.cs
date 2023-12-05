using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

using BrokerIQ.Dto.Enum;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Models;
using BrokerIQ.Online.Server.Shared;
using BrokerIQ.Online.Services.Interface;

using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Audio
{
    public class AudioDetailBase : ComponentBase
    {
        [Inject]
        protected ICustomerService CustomerService { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }
        [Inject]
        protected INotificationService NotificationService { get; set; }

        [Inject]
        protected IAlertService AlertService { get; set; }

        [Inject]
        protected IDialogService DialogService { get; set; }

        [Inject]
        public IAudioService AudioService { get; set; }

        [Inject]
        public IAccountService AccountService { get; set; }

        [Inject]
        public IBrokerService BrokerService { get; set; }

        protected Models.Audio Audio { get; set; }

        protected List<Online.Models.Customer> Customers { get; set; }

        protected HashSet<Online.Models.Customer> SelectedCustomers { get; set; }

        protected string Url { get; set; }

        protected string AudioName { get; set; }

        protected string AudioNameNoExtension { get; set; }

        protected string selectedNotification;

        protected string SearchTerm { get; set; } = "";

        protected bool Vetted { get; set; }

        public List<Online.Models.Broker> Brokers { get; set; }

        public int BrokerId { get; set; }

        protected bool IsAdmin { get; set; }

        public int CustomerCategory { get; set; }

        public int AgeRange { get; set; }

        protected int? ProfilingOption { get; set; }

        public CustomerCategoryEnum[] CustomerCategoriesByRelevance;

        //filter
        protected List<Online.Models.Customer> FilteredCustomers => Customers.Where(i => !string.IsNullOrEmpty(i.Name) && i.Name.ToLower().Contains(SearchTerm.ToLower())).ToList();

        protected override async Task OnInitializedAsync()
        {
            var query = new Uri(NavigationManager.Uri).Query;
            selectedNotification = "A new voice recording has arrived";

            CustomerCategoriesByRelevance = Extensions.Extensions.GetAllCustomerCategories();

            if (QueryHelpers.ParseQuery(query).TryGetValue("Url", out var value))
            {
                Url = value;
                try
                {
                    int pos = Url.LastIndexOf("/") + 1;
                    AudioName = Url.Substring(pos, Url.Length - pos);
                    AudioNameNoExtension = Path.GetFileNameWithoutExtension(AudioName);
                }
                catch
                {
                    AlertService.Error("Get Audios failed");
                }

            }

            var user = await AccountService.GetUser();
            IsAdmin = user.IsAdmin;
            if (IsAdmin)
            {
                Brokers = (await BrokerService.GetBrokers()).ToList();
            }
            else
            {
                Brokers = new List<Online.Models.Broker>();
                BrokerId = user.MasterBrokerId;

                var broker = await BrokerService.GetBroker(user.MasterBrokerId);

                if (broker.BrokerIdentifier.InsuranceOnly)
                {
                    CustomerCategoriesByRelevance = Extensions.Extensions.GetFilteredCustomerCategories(new int[] { 0, 2 });
                }
            }

            try
            {
                Vetted = true;//await AudioService.IsVetted(AudioName);
                var allCustomers = await CustomerService.GetAllCustomers();
                Customers = allCustomers.Where(x => x.AudioNotificationsAllowed || x.MarketingMessagesAllowed).ToList();
            }
            catch
            {
                AlertService.Error("Get Audios failed");
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
            var result = await DialogService.Show<ScrollableDialog>("Send Audio Notification", dialogParams).Result;

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
                        succeeded = await NotificationService.SendAudioNotification(selectedNotification, Url, targets, sendAll);
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
            Customers = (await CustomerService.GetFilteredCustomers(new CustomerFilter()
            {
                BrokerId = BrokerId,
                Category = CustomerCategory,
                AgeRange = AgeRange,
                ProfilePictures = false,
                ProfilingOption = ProfilingOption.HasValue ? (ProfilingOptionEnum)ProfilingOption : null
            })).ToList();

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
            dialogParams.Add("AudioUrl", Url);
            dialogParams.Add("AudioName", AudioNameNoExtension);


            var dialogOptions = new DialogOptions()
            {
                MaxWidth = MaxWidth.Medium,
                FullWidth = true
            };

            await DialogService.Show<AudioChatDialog>("Send Audio To Multiple Chats", dialogParams, dialogOptions).Result;
        }
    }
}
