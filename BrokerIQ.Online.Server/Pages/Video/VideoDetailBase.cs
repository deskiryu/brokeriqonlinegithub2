using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using BrokerIQ.Dto.Enum;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Models;
using BrokerIQ.Online.Server.Shared;
using BrokerIQ.Online.Services.Interface;

using MudBlazor;
using BrokerIQ.Online.Server.Services;
using Microsoft.AspNetCore.Components.Forms;
using System.IO;

namespace BrokerIQ.Online.Server.Pages.Video
{
    public class VideoDetailBase : ComponentBase
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
        public IVideoService VideoService { get; set; }

        [Inject]
        public IAccountService AccountService { get; set; }

        [Inject]
        public IBrokerService BrokerService { get; set; }

        protected Models.Video Video { get; set; }

        protected List<Online.Models.Customer> Customers { get; set; }

        protected HashSet<Online.Models.Customer> SelectedCustomers { get; set; }

        public List<Online.Models.Broker> Brokers { get; set; }

        public int BrokerId { get; set; }

        protected string Url { get; set; }

        [Parameter]
        public string VideoId { get; set; }

        protected VideoThumbnail VideoThumbnail { get; set; }

        protected string VideoNameNoExtension { get; set; }

        protected string selectedNotification;

        protected string SearchTerm { get; set; } = "";

        protected Models.Video ThisVideo { get; set; }

        protected bool Vetted { get; set; }

        protected bool IsAdmin { get; set; }

        protected string ThumbnailImage { get; set; }

        public int CustomerCategory { get; set; }

        public int AgeRange { get; set; }

        protected int? ProfilingOption { get; set; }

        public CustomerCategoryEnum[] CustomerCategoriesByRelevance;

        //filter
        protected List<Online.Models.Customer> FilteredCustomers => Customers.Where(i => !string.IsNullOrEmpty(i.Name) && i.Name.ToLower().Contains(SearchTerm.ToLower())).ToList();

        protected override async Task OnInitializedAsync()
        {
            selectedNotification = "A new video has arrived";

            CustomerCategoriesByRelevance = Extensions.ExtensionClass.GetAllCustomerCategories();

            var user = await AccountService.GetUser();
            IsAdmin = user.IsAdmin;
            if (IsAdmin)
            {
                Brokers = (await BrokerService.GetBrokers()).ToList();
                BrokerId = 0;
            }
            else
            {
                Brokers = new List<Online.Models.Broker>();
                BrokerId = user.MasterBrokerId;

                var broker = await BrokerService.GetBroker(user.MasterBrokerId);

                if (broker.BrokerIdentifier.InsuranceOnly)
                {
                    CustomerCategoriesByRelevance = Extensions.ExtensionClass.GetFilteredCustomerCategories(new int[] { 0, 2 });
                }
            }

            try
            {
                if (user == null)
                {
                    throw new Exception();
                }

                ThisVideo = await VideoService.GetVideo(int.Parse(VideoId), BrokerId);
                Url = ThisVideo.Url;
                Vetted = ThisVideo.Vetted;
                ThumbnailImage = ThisVideo.VideoThumbnailData;
                var allCustomers = await CustomerService.GetAllCustomers();
                Customers = allCustomers.Where(x => x.VideoNotificationsAllowed || x.MarketingMessagesAllowed).ToList();

                Url = ThisVideo.Url;

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
                IEnumerable<Online.Models.Broker> filtered = null;
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
                if (SelectedCustomers != null && SelectedCustomers.Any())
                {
                    targetsName = SelectedCustomers.Where(x => x.EmailConfirmed == true).Select(x => x.Name).ToList();
                }
            }

            if (targetsName == null || !targetsName.Any())
            {
                AlertService.Error("No targets chosen");
                return;
            }

            dialogParams.Add("Users", targetsName);
            dialogParams.Add("areBrokers", false);
            var result = await DialogService.Show<ScrollableDialog>("Send Video Notification", dialogParams).Result;

            if (!result.Canceled)
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
                }
                ;

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
            dialogParams.Add("MediaUrl", ThisVideo.Url);
            dialogParams.Add("MediaName", ThisVideo.Name);
            dialogParams.Add("VideoThumbnailData", ThisVideo.VideoThumbnailData);
            dialogParams.Add("IsAudio", false);

            var dialogOptions = new DialogOptions()
            {
                MaxWidth = MaxWidth.Medium,
                FullWidth = true
            };

            await DialogService.Show<VideoAudioChatDialog>("Send Video To Multiple Chats", dialogParams, dialogOptions).Result;
        }

        public async Task LoadFiles(InputFileChangeEventArgs e)
        {

            try
            {
                var file = e.GetMultipleFiles(1).FirstOrDefault();
                var ext = Path.GetExtension(file.Name);
                if (ext != ".jpeg" && ext != ".jpg")
                {
                    throw new Exception("Jpeg files only");
                }
                if (file != null)
                {
                    var memoryStream = new MemoryStream();
                    await file.OpenReadStream(int.MaxValue).CopyToAsync(memoryStream);
                    var data = memoryStream.ToArray();
                    memoryStream.Position = 0;
                    var result = await VideoService.UploadThumbnail(ThisVideo.Id, memoryStream, BrokerId);
                    if (result.Item1)
                    {
                        ThisVideo.VideoThumbnailData = ThumbnailImage = result.Item2;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {

                StateHasChanged();
            }
        }
    }
}
