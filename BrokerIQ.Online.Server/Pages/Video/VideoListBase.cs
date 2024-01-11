using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Globalization;

namespace BrokerIQ.Online.Pages
{

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;
    using MudBlazor;
    using BrokerIQ.Dto.Models;
    using BrokerIQ.Online.Server.Models;
    using BrokerIQ.Online.Services.Interface;
    using BrokerIQ.Online.Server.Shared;
    using BrokerIQ.Online.Models;
    using System.ComponentModel.DataAnnotations;
    using BrokerIQ.Online.Server.Helper;
    using Microsoft.JSInterop;
    using static System.Runtime.InteropServices.JavaScript.JSType;
    using Dto.Enum;

    public class VideoListBase : ComponentBase
    {
        protected string Message = string.Empty;
        protected string StatusClass = string.Empty;
        protected bool Saved;
        public string VideoName { get; set; }
        public DateTime? VideoSendDate { get; set; }

        public string ExtensionName { get; set; }
        public bool RenameUploadVisibility { get; set; }

        public string status;

        [Inject]
        protected IJSRuntime js { get; set; }

        [Inject]
        public IVideoService VideoService { get; set; }

        [Inject]
        public IMetaDefenderCoreService MetaDefenderCoreService { get; set; }

        [Inject]
        public IAccountService AccountService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IDialogService DialogService { get; set; }

        [Inject]
        public IBrokerService BrokerService { get; set; }

        [Inject]
        public IAdminService AdminService { get; set; }

        [Inject]
        public IEmailService EmailService { get; set; }

        public List<Video> Videos { get; set; }

        public List<Broker> Brokers { get; set; }

        public Dictionary<int, bool> DisplayEmbeddedVideo { get; set; }

        public int BrokerId { get; set; }

        public int FilterBrokerId { get; set; }

        [Required]
        public int BrokerListId = 0;

        public IBrowserFile fileListEntry;

        public string SpinnerVisible { get; set; }

        public bool VideoUploading { get; set; }

        public bool IsAdmin { get; set; }

        public bool IsMinorAdmin { get; set; }

        public bool IsBroker { get; set; }

        public bool IsBrokerStaff { get; set; }

        protected override async Task OnInitializedAsync()
        {
            SpinnerVisible = "display:none";
            CultureInfo.CurrentCulture = new CultureInfo("en-GB", false);
            StateHasChanged();
            IsAdmin = false;
            IsMinorAdmin = false;

            try
            {
                var user = await AccountService.GetUser();
                if (user == null)
                {
                    throw new Exception();
                }
                IsMinorAdmin = false;
                if (user.IsAdmin || user.IsMinorAdmin || user.MasterBrokerId == 0)
                {
                    await VerifyAdmin();
                    IsAdmin = user.IsAdmin;
                    IsMinorAdmin = user.IsMinorAdmin;
                    BrokerId = 0;
                    Brokers = (await BrokerService.GetBrokers()).ToList();
                }
                else if (user.IsBroker || user.IsBrokerStaff)
                {
                    IsBroker = user.IsBroker;
                    BrokerId = user.MasterBrokerId;
                    IsBrokerStaff = user.IsBrokerStaff;
                }
                else
                {
                    throw new Exception();
                }

                Videos = (await VideoService.GetVideos(BrokerId)).ToList();
                DisplayEmbeddedVideo = new Dictionary<int, bool>();
                foreach (Video video in Videos)
                {
                    DisplayEmbeddedVideo[video.Id] = false;
                    if (IsAdmin)
                    {
                        video.BrokerName = Brokers.FirstOrDefault(x => x.Id == video.BrokerId)?.Name;
                    }
                }
                StateHasChanged();
            }
            catch
            {
                NavigationManager.NavigateTo($"account/logout");
            }
        }

        protected async Task LoadFiles(InputFileChangeEventArgs e)
        {
            fileListEntry = e.GetMultipleFiles(1).FirstOrDefault();
            if (fileListEntry != null)
            {
                //Rename dialog
                RenameUploadVisibility = true;
                VideoName = Path.GetFileNameWithoutExtension(fileListEntry.Name);
                ExtensionName = Path.GetExtension(fileListEntry.Name);
            }
        }

        public async Task OnCancelPushed()
        {
            RenameUploadVisibility = false;
        }


        protected async Task DeleteVideo(int id)
        {
            await VerifyAccess();
            var dialogParams = new DialogParameters();
            dialogParams.Add("Message", "Are you sure you want to delete this video?");
            var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Cancelled)
            {
                bool succeeded = await VideoService.DeleteVideo(id, BrokerId);

                if (succeeded)
                {
                    // cleanup video from in video list to be displayed
                    foreach (Video video in Videos)
                    {
                        if (video.Id == id)
                        {
                            Videos.Remove(video);
                            break;
                        }
                    }

                    await RefreshVideosWithDialogMessage(succeeded, "Deleted successfully");
                }
                else
                {
                    await RefreshVideosWithDialogMessage(succeeded, "Something went wrong deleting the video. Please try again.");
                }
            }
        }

        protected async Task SetWelcomeVideo(int id)
        {
            await VerifyAccess();

            if ((IsAdmin || IsMinorAdmin) && FilterBrokerId == 0)
            {
                await RefreshVideosWithDialogMessage(true, "Filter videos by broker first");
                return;
            }

            var dialogParams = new DialogParameters();
            var videoAlreadyChecked = Videos.FirstOrDefault(x => x.VideoSendTypeId == VideoSendEnum.WelcomeVideo && x.Id==id);
            bool alreadyChecked = false;
            if (videoAlreadyChecked != null)
            {
                alreadyChecked = true;
            }
            if (alreadyChecked)
            {
                dialogParams.Add("Message", "There will be no welcome video. Continue?");
            }
            else
            {
                dialogParams.Add("Message", "This video will be sent to clients when they sign up. Continue?");
            }

            var brokerId = (IsAdmin || IsMinorAdmin) ? FilterBrokerId : BrokerId;
            var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Cancelled)
            {
                var returned = await VideoService.SetWelcomeVideo(id, brokerId, !alreadyChecked);
                if (returned.Item1)
                {
                    await RefreshVideosWithDialogMessage(true, "Welcome video set successfully");
                }
                else
                {
                    await RefreshVideosWithDialogMessage(false, "Something went wrong setting the welcome video. Please try again.");
                }
            }
        }

        protected async Task SetBirthdayVideo(int id)
        {
            await VerifyAccess();

            if ((IsAdmin || IsMinorAdmin) && FilterBrokerId == 0)
            {
                await RefreshVideosWithDialogMessage(true, "Filter videos by broker first");
                return;
            }

            var dialogParams = new DialogParameters();
            var videoAlreadyChecked = Videos.FirstOrDefault(x => x.VideoSendTypeId == VideoSendEnum.BirthdayVideo && x.Id == id && x.Id == id);
            bool alreadyChecked = false;
            if (videoAlreadyChecked != null)
            {
                alreadyChecked = true;
            }
            if (alreadyChecked)
            {
                dialogParams.Add("Message", "There will be no birthday video. Continue?");
            }
            else
            {
                dialogParams.Add("Message", "This video will be sent to clients on their birthday. Continue?");
            }

            var brokerId = (IsAdmin || IsMinorAdmin) ? FilterBrokerId : BrokerId;
            var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Cancelled)
            {
                var returned = await VideoService.SetBirthdayVideo(id, brokerId, !alreadyChecked);

                if (returned.Item1)
                {
                    await RefreshVideosWithDialogMessage(true, "Birthday video set successfully");
                }
                else
                {
                    await RefreshVideosWithDialogMessage(false, "Something went wrong setting the birthday video. Please try again.");
                }
            }
        }

        protected async Task SetMortgageVideo(int id)
        {
            await VerifyAccess();

            if ((IsAdmin || IsMinorAdmin) && FilterBrokerId == 0)
            {
                await RefreshVideosWithDialogMessage(true, "Filter videos by broker first");
                return;
            }

            var selectedVideo = Videos.FirstOrDefault(x => x.VideoSendTypeId == VideoSendEnum.MortgageVideo && x.Id == id);
            var isMortgageVideo = selectedVideo != null;

            var dialogParams = new DialogParameters();
            dialogParams.Add("Message", isMortgageVideo ?
                "There will be no mortgage video. Continue?" :
                "This video will be sent to clients when a new mortgage is created. Continue?");

            var brokerId = (IsAdmin || IsMinorAdmin) ? FilterBrokerId : BrokerId;
            var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Cancelled)
            {
                var returned = await VideoService.SetMortgageVideo(id, brokerId, !isMortgageVideo);

                if (returned.Item1)
                {
                    await RefreshVideosWithDialogMessage(true, "Mortgage video set successfully");
                }
                else
                {
                    await RefreshVideosWithDialogMessage(false, "Something went wrong setting the mortgage video. Please try again.");
                }
            }
        }

        protected async Task<bool> SetVideoSendDate(int id, DateTime? date)
        {
            await VerifyAccess();

            if ((IsAdmin || IsMinorAdmin) && FilterBrokerId == 0)
            {
                await RefreshVideosWithDialogMessage(true, "Filter videos by broker first");
                return false;
            }
            var brokerId = (IsAdmin || IsMinorAdmin) ? FilterBrokerId : BrokerId;
            var returned = await VideoService.SetVideoSendDate(id, brokerId, date);

            if (returned.Item1)
            {
                await RefreshVideosWithDialogMessage(true, "Video send date set successfully");
            }
            else
            {
                await RefreshVideosWithDialogMessage(false, "Something went wrong setting the video send date. Please try again.");
            }

            return returned.Item1;
        }

        protected async Task SetSendDateTick(int id)
        {
            await VerifyAccess();

            if ((IsAdmin || IsMinorAdmin) && FilterBrokerId == 0)
            {
                await RefreshVideosWithDialogMessage(true, "Filter videos by broker first");
                return;
            }

            var dialogParams = new DialogParameters();
            var videoAlreadyChecked = Videos.FirstOrDefault(x => x.Id == id && x.VideoSendTypeId == VideoSendEnum.SendOnDate);
            bool alreadyChecked = false;
            if (videoAlreadyChecked != null)
            {
                alreadyChecked = true;
            }
            if (alreadyChecked)
            {
                dialogParams.Add("Message", "This video will not send on this date. Continue?");
            }
            else
            {
                dialogParams.Add("Message", "This video will be sent to all cilents on this date. Continue?");
            }

            var brokerId = (IsAdmin || IsMinorAdmin) ? FilterBrokerId : BrokerId;
            var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Cancelled)
            {
                var returned = await VideoService.SetVideoSendDateTick(id, brokerId, !alreadyChecked);

                if (returned.Item1)
                {
                    await RefreshVideosWithDialogMessage(true, "Send date tick set successfully");
                }
                else
                {
                    await RefreshVideosWithDialogMessage(false, "Something went wrong setting the send date tick. Please try again.");
                }
            }
        }

        protected async Task SetVetted(int id)
        {
            await VerifyAccess();

            if (!(await CheckIsAdmin()))
            {
                if (IsBrokerStaff)
                {
                    await RefreshVideosWithDialogMessage(true, "Only the main broker can vet the video");
                }
                else
                {
                    await RefreshVideosWithDialogMessage(true, "Only admin can vet");
                }

                return;
            }

            if (IsMinorAdmin)
            {
                await RefreshVideosWithDialogMessage(true, "Only brokeriq admin can vet the video");
                return;
            }


            var dialogParams = new DialogParameters();
            var videoAlreadyChecked = Videos.FirstOrDefault(x => x.Id == id);
            bool alreadyChecked = false;
            if (videoAlreadyChecked != null)
            {
                alreadyChecked = videoAlreadyChecked.Vetted;
            }

            var returned = await VideoService.SetVetted(id, !alreadyChecked);

            if (returned.Item1)
            {
                await RefreshVideos();
            }
            else
            {
                await RefreshVideosWithDialogMessage(false, "Something went wrong setting vetted. Please try again.");
            }

        }
        public async Task UploadButtonPushed()
        {
            VideoUploading = false;
            RenameUploadVisibility = false;

            await VerifyAccess();

            if (fileListEntry != null)
            {
                SpinnerVisible = "display:block";
                VideoUploading = true;
                StateHasChanged();

                var memoryStream = new MemoryStream();
                await fileListEntry.OpenReadStream(int.MaxValue).CopyToAsync(memoryStream);

                var uploadedVideoId = (0,"");
                var localBrokerId = BrokerId;
                if (IsAdmin || IsMinorAdmin)
                {
                    localBrokerId = BrokerListId;
                }

                uploadedVideoId = await VideoService.UploadAnalyseAndConvertVideo(VideoName + ExtensionName, memoryStream, localBrokerId);

                status = $"Finished loading {fileListEntry.Size} bytes from {fileListEntry.Name}";

                StateHasChanged();

                if (uploadedVideoId.Item1>0)
                {
                    StateHasChanged();

                    // fetch thumbnail for uploaded video (might not be instantly available as generated by FunctionVideoThumbnail Azure function)
                    // if issue fetching the image after 10 seconds, just proceed without thumbnail
                    int attempts = 20;

                    while (attempts > 0)
                    {
                        
                        var thumbnail = await VideoService.GetVideoThumbnail(uploadedVideoId.Item1, BrokerId);
                        if (thumbnail != null && thumbnail.Data != null)
                        {
                            break;
                        }
                        await Task.Delay(1000);
                        attempts--;
                    }

                    DisplayEmbeddedVideo[uploadedVideoId.Item1] = false;
                    await RefreshVideosWithDialogMessage(true, $"Uploaded successfully");

                }
                else
                {
                    await RefreshVideosWithDialogMessage(false, "Something went wrong adding the video. " + uploadedVideoId.Item2);
                }
            }

            VideoUploading = false;
            SpinnerVisible = "display:none";
            StateHasChanged();
        }

        protected async Task VerifyAccess()
        {
            var user = await AccountService.GetUser();
            if ((IsAdmin || IsMinorAdmin) || user.MasterBrokerId == 0)
            {
                await VerifyAdmin();
            }
            else if (user.IsBroker || user.IsBrokerStaff)
            {
                await VerifyBroker();
            }
            else
            {
                NavigationManager.NavigateTo($"account/logout");
            }
        }

        protected async Task VerifyBroker()
        {
            bool verified;
            try
            {
                var response = await BrokerService.VerifyBroker(BrokerId);
                verified = response.BoolResult;
                IsAdmin = false;
            }
            catch
            {
                verified = false;
            }

            if (!verified)
            {
                NavigationManager.NavigateTo($"account/logout");
            }
        }

        protected async Task VerifyAdmin()
        {
            if (!await CheckIsAdmin())
            {
                NavigationManager.NavigateTo($"account/logout");
            }
        }

        protected async Task<bool> CheckIsAdmin()
        {
            bool verified;
            try
            {
                var response = await AdminService.VerifyAdmin();
                verified = response.BoolResult;
                IsAdmin = verified;

                response = await AdminService.VerifyMinorAdmin();
                verified = response.BoolResult;
                IsMinorAdmin = verified;

                var user = await AccountService.GetUser();
                IsBroker = user.IsBroker;

                verified = IsBroker || IsMinorAdmin || IsAdmin;
            }
            catch
            {
                verified = false;
            }

            return verified;
        }

        protected void NavigateToOverview()
        {
            Saved = false;
            NavigationManager.NavigateTo("/videolist/");
        }

        protected void ShowVideoPlayer(int id)
        {
            DisplayEmbeddedVideo[id] = true;
            StateHasChanged();
        }

        /// <summary>
        /// Refreshes videolist on page if desired. Displays appropriate dialog message.
        /// </summary>
        /// <param name="success">Success of prior API call</param>
        /// <param name="message">Message to be displayed in dialog</param>
        private async Task RefreshVideosWithDialogMessage(bool refresh, string message)
        {
            if (refresh)
            {
                RefreshVideos();
            }

            var responseParams = new DialogParameters();
            responseParams.Add("Message", message);
            await DialogService.Show<AlertDialog>("Information", responseParams).Result;
        }

        private async Task RefreshVideos()
        {
            Videos.Clear();
            Videos = (await VideoService.GetVideos(BrokerId)).ToList();
            if (IsAdmin || IsMinorAdmin)
            {
                foreach (Video video in Videos)
                {
                    video.BrokerName = Brokers.FirstOrDefault(x => x.Id == video.BrokerId)?.Name;
                }
            }
            StateHasChanged();
        }

        /// <summary>
        /// Displays error message to user in a dialog box.
        /// </summary>
        /// <param name="message">Message to be displayed</param>
        private async Task DisplayErrorDialog(string message)
        {
            var responseParams = new DialogParameters();
            responseParams.Add("Message", message);
            await DialogService.Show<AlertDialog>("Error", responseParams).Result;
        }

        protected async Task UpdateVideoSendDate(DateTime? sendDate, int id)
        {
            // Set the new value first, to avoid double firing from the DateChanged event.
            var existingVideo = this.Videos.First(item => item.Id == id);
            DateTime? oldDate = existingVideo.SendDate;
            existingVideo.SendDate = sendDate;

            await SetVideoSendDate(id, sendDate);
        }
    }
}
