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
    using MudBlazor;
    using BrokerIQ.Online.Server.Extensions;

    public class VideoListBase2 : ComponentBase
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

        public Video WelcomeVideo { get; set; }

        public Video BirthdayVideo { get; set; }

        public Video MortgageVideo { get; set; }

        public Video InsuranceVideo { get; set; }

        public Video SendDateVideo1 { get; set; }

        public Video SendDateVideo2 { get; set; }

        public Video SendDateVideo3 { get; set; }

        public Video SendDateVideo4 { get; set; }

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

                if (user.IsBroker || user.IsBrokerStaff)
                {
                    await RefreshVideos();
                    StateHasChanged();
                }

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

                var uploadedVideoId = 0;
                var localBrokerId = BrokerId;
                if (IsAdmin || IsMinorAdmin)
                {
                    localBrokerId = BrokerListId;
                }

                uploadedVideoId = await VideoService.UploadAnalyseAndConvertVideo(VideoName + ExtensionName, memoryStream, localBrokerId);

                status = $"Finished loading {fileListEntry.Size} bytes from {fileListEntry.Name}";

                StateHasChanged();

                if (uploadedVideoId>0)
                {
                    StateHasChanged();

                    // fetch thumbnail for uploaded video (might not be instantly available as generated by FunctionVideoThumbnail Azure function)
                    // if issue fetching the image after 10 seconds, just proceed without thumbnail
                    int attempts = 20;

                    while (attempts > 0)
                    {
                        
                        var thumbnail = await VideoService.GetVideoThumbnail(uploadedVideoId, BrokerId);
                        if (thumbnail != null && thumbnail.Data != null)
                        {
                            break;
                        }
                        await Task.Delay(1000);
                        attempts--;
                    }

                    DisplayEmbeddedVideo[uploadedVideoId] = false;
                    await RefreshVideosWithDialogMessage(true, $"Uploaded successfully");

                }
                else
                {
                    await RefreshVideosWithDialogMessage(false, "Something went wrong adding the video. Please try again.");
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
            NavigationManager.NavigateTo("/videolist2/");
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

        public async Task RefreshVideoDrag(string newIdentifier)
        {
            var message = string.Empty;

            if (newIdentifier.Contains("SendDateVideo"))
            {
                message = await AddDraggedToVideoSendList(newIdentifier);
            }
            else
            {
                message = await CompareDraggedToVideoList();
            }

            if (!string.IsNullOrEmpty(message))
            {
                var responseParams = new DialogParameters();
                responseParams.Add("Message", message);

                var result = await DialogService.Show<ConfirmCancelDialog>("Information", responseParams).Result;
                if (!result.Cancelled)
                {
                    await ApplyDraggedChanges();
                }
            }
            await RefreshVideos();
        }

        public async Task DropNotAllowed()
        {
            var message = "Please remove existing send date video first";

            var responseParams = new DialogParameters();
            responseParams.Add("Message", message);

            await DialogService.Show<AlertDialog>("Information", responseParams).Result;
   
        }

        protected async Task<string> CompareDraggedToVideoList()
        {
            await VerifyAccess();
            var message = string.Empty;

            foreach(var video in Videos)
            {
                if(video != null)
                {
                    if(video.Identifier!="Welcome" && video.VideoSendTypeId == VideoSendEnum.WelcomeVideo)
                    {
                        message += $"Video {video.Name} will no longer be a welcome video. ";                 
                    }
                    else if (video.Identifier == "Welcome" && video.VideoSendTypeId != VideoSendEnum.WelcomeVideo)
                    {
                        if (video.Vetted)
                        {
                            message += $"Video {video.Name} will be a welcome video. ";
                        }
                        else
                        {
                            message += $"Video {video.Name} is not vetted. By continuing you verify that this video is of appropriate content and will be a welcome video. ";
                        }

                    }
                    if (video.Identifier != "Mortgage" && video.VideoSendTypeId == VideoSendEnum.MortgageVideo)
                    {
                        message += $"Video {video.Name} will no longer be a mortgage video. ";
                    }
                    else if (video.Identifier == "Mortgage" && video.VideoSendTypeId != VideoSendEnum.MortgageVideo)
                    {
                        if (video.Vetted)
                        {
                            message += $"Video {video.Name} will be a mortgage video. ";
                        }
                        else
                        {
                            message += $"Video {video.Name} is not vetted. By continuing you verify that this video is of appropriate content and will be a mortgage video. ";
                        }
                    }
                    if (video.Identifier != "Birthday" && video.VideoSendTypeId == VideoSendEnum.BirthdayVideo)
                    {
                        message += $"Video {video.Name} will no longer be a birthday video. ";
                    }
                    else if (video.Identifier == "Birthday" && video.VideoSendTypeId != VideoSendEnum.BirthdayVideo)
                    {
                        if (video.Vetted)
                        {
                            message += $"Video {video.Name} will be a birthday video. ";
                        }
                        else
                        {
                            message += $"Video {video.Name} is not vetted. By continuing you verify that this video is of appropriate content and will be a birthday video. ";
                        }
                    }
                    if (video.Identifier != "Insurance" && video.VideoSendTypeId == VideoSendEnum.InsuranceVideo)
                    {
                        message += $"Video {video.Name} will no longer be a insurance video. ";
                    }
                    else if (video.Identifier == "Insurance" && video.VideoSendTypeId != VideoSendEnum.InsuranceVideo)
                    {
                        if (video.Vetted)
                        {
                            message += $"Video {video.Name} will be a insurance video. ";
                        }
                        else
                        {
                            message += $"Video {video.Name} is not vetted. By continuing you verify that this video is of appropriate content and will be a insurance video. ";
                        }
                    }
                }
            }
            return message;
        }

        protected async Task<string> AddDraggedToVideoSendList(string newIdentifier)
        {
            await VerifyAccess();
            var message = string.Empty;

            foreach (var video in Videos)
            {
                if (video != null)
                {
                    if (newIdentifier == "SendDateVideo1")
                    {
                        if (video.Identifier == "SendDateVideo1" && video.VideoSendTypeId != VideoSendEnum.SendOnDate)
                        {
                            if (video.Vetted)
                            {
                                message += $"Video {video.Name} will be a send on date video. ";
                            }
                            else
                            {
                                message += $"Video {video.Name} is not vetted. By continuing you verify that this video is of appropriate content and will be a send on date video. ";
                            }

                        }
                    }
                    if (newIdentifier == "SendDateVideo2")
                    {
                        if (video.Identifier == "SendDateVideo2" && video.VideoSendTypeId != VideoSendEnum.SendOnDate)
                        {
                            if (video.Vetted)
                            {
                                message += $"Video {video.Name} will be a send on date video. ";
                            }
                            else
                            {
                                message += $"Video {video.Name} is not vetted. By continuing you verify that this video is of appropriate content and will be a send on date video. ";
                            }
                        }
                    }
                    if (newIdentifier == "SendDateVideo3")
                    {
                        if (video.Identifier == "SendDateVideo3" && video.VideoSendTypeId != VideoSendEnum.SendOnDate)
                        {
                            if (video.Vetted)
                            {
                                message += $"Video {video.Name} will be a send on date video. ";
                            }
                            else
                            {
                                message += $"Video {video.Name} is not vetted. By continuing you verify that this video is of appropriate content and will be a send on date video. ";
                            }
                        }
                    }
                    if (newIdentifier == "SendDateVideo4")
                    {
                        if (video.Identifier == "SendDateVideo4" && video.VideoSendTypeId != VideoSendEnum.SendOnDate)
                        {
                            if (video.Vetted)
                            {
                                message += $"Video {video.Name} will be a send on date video. ";
                            }
                            else
                            {
                                message += $"Video {video.Name} is not vetted. By continuing you verify that this video is of appropriate content and will be a send on date video. ";
                            }
                        }
                    }

                }
            }
            return message;
        }

        protected async Task<string> RemoveDraggedToVideoSendList(string oldIdentifier)
        {
            await VerifyAccess();
            var message = string.Empty;

            foreach (var video in Videos)
            {
                if (video != null)
                {
                    var dateStr = video.SendDate.HasValue ? video.SendDate.Value.ToString() : "n/a";
                    if (oldIdentifier == "SendDateVideo1")
                    {
                        if (video.Identifier == "SendDateVideo1" && video.VideoSendTypeId == VideoSendEnum.SendOnDate)
                        {  
                            message += $"Video {video.Name} will no longer be sont on date {dateStr}.";
                        }
                    }
                    if (oldIdentifier == "SendDateVideo2")
                    {
                        if (video.Identifier == "SendDateVideo2" && video.VideoSendTypeId == VideoSendEnum.SendOnDate)
                        {
                            message += $"Video {video.Name} will no longer be sont on date {dateStr}.";
                        }
                    }
                    if (oldIdentifier == "SendDateVideo3")
                    {
                        if (video.Identifier == "SendDateVideo3" && video.VideoSendTypeId == VideoSendEnum.SendOnDate)
                        {
                            message += $"Video {video.Name} will no longer be sont on date {dateStr}.";
                        }
                    }
                    if (oldIdentifier == "SendDateVideo4")
                    {
                        if (video.Identifier == "SendDateVideo4" && video.VideoSendTypeId == VideoSendEnum.SendOnDate)
                        {
                            message += $"Video {video.Name} will no longer be sont on date {dateStr}.";
                        }
                    }
                }
            }
            return message;
        }

        protected async Task<string> ApplyDraggedChanges()
        {
            await VerifyAccess();
            var message = string.Empty;

            foreach (var video in Videos)
            {
                if (video != null)
                {
                    await this.VideoService.SetVetted(video.Id, true);
                    if (video.Identifier == "Welcome" && video.VideoSendTypeId != VideoSendEnum.WelcomeVideo)
                    {
                        await this.VideoService.SetWelcomeVideo(video.Id, BrokerId, true);
                        break;
                    }
                    if (video.Identifier == "Mortgage" && video.VideoSendTypeId != VideoSendEnum.MortgageVideo)
                    {
                        await this.VideoService.SetMortgageVideo(video.Id, BrokerId, true);
                        break;
                    }
                    if (video.Identifier == "Birthday" && video.VideoSendTypeId != VideoSendEnum.BirthdayVideo)
                    {
                        await this.VideoService.SetBirthdayVideo(video.Id, BrokerId, true);
                        break;
                    }

                    if (video.Identifier == "Insurance" && video.VideoSendTypeId != VideoSendEnum.InsuranceVideo)
                    {
                        await this.VideoService.SetInsuranceVideo(video.Id, BrokerId, true);
                        break;
                    }

                    if (video.Identifier.Contains("SendDateVideo") && video.VideoSendTypeId != VideoSendEnum.SendOnDate)
                    {
                        await this.VideoService.SetVideoSendDateTick(video.Id, BrokerId, true);
                        break;
                    }

                }
            }
            return message;
        }

        protected async Task RemoveDrag(int videoId, VideoSendEnum videoenum)
        {

            var video = Videos.FirstOrDefault(x => x.Id == videoId);
            var previousIdentifier = video.Identifier;

            var message = string.Empty;

            if (previousIdentifier.Contains("SendDateVideo"))
            {
                message = await RemoveDraggedToVideoSendList(previousIdentifier);
                video.Identifier = "Files";
            }
            else
            {            
                video.Identifier = "Files";
                message = await CompareDraggedToVideoList();
            }



            if (!string.IsNullOrEmpty(message))
            {
                var responseParams = new DialogParameters();
                responseParams.Add("Message", message);

                var result = await DialogService.Show<ConfirmCancelDialog>("Information", responseParams).Result;
                if (!result.Cancelled)
                {
                    switch (videoenum)
                    {
                        case VideoSendEnum.WelcomeVideo: WelcomeVideo = null;break;
                        case VideoSendEnum.BirthdayVideo: BirthdayVideo = null; break;
                        case VideoSendEnum.MortgageVideo: MortgageVideo = null; break;
                        case VideoSendEnum.InsuranceVideo: InsuranceVideo = null; break;
                        case VideoSendEnum.SendOnDate:
                            {
                                switch (previousIdentifier)
                                {
                                    case "SendDateVideo1": SendDateVideo1 = null; break;
                                    case "SendDateVideo2": SendDateVideo2 = null; break;
                                    case "SendDateVideo3": SendDateVideo3 = null; break;
                                    case "SendDateVideo4": SendDateVideo4 = null; break;
                                }
                                break;
                            }
                    }
                    await this.VideoService.SetNoVideo(video.Id, BrokerId);
                }
            }
            await RefreshVideos();
        }

        protected async Task SendVideo(int videoId)
        {
            var video = Videos.FirstOrDefault(x => x.Id == videoId);
            if (video != null)
            {
                var message = string.Empty;
                if (!video.Vetted)
                {
                    message = $"Video {video.Name} is not vetted. By continuing you verify that this video is of appropriate content.";
                    var responseParams = new DialogParameters();
                    responseParams.Add("Message", message);

                    var result = await DialogService.Show<ConfirmCancelDialog>("Information", responseParams).Result;
                    if (!result.Cancelled)
                    {
                        await this.VideoService.SetVetted(video.Id, true);
                    }
                    else 
                    { 
                        return; 
                    }
                }
            }
            NavigationManager.NavigateTo($"videodetail/{videoId}");
        }

        public void OnBrokerChanged(int brokerId)
        {
            FilterBrokerId = brokerId;
            RefreshVideos();
        }

        private async Task RefreshVideos()
        {
            var brokerId = (IsAdmin || IsMinorAdmin) ? FilterBrokerId : BrokerId;

            if (Videos != null)
            {
                Videos.Clear();
                SendDateVideo1 = null;
                SendDateVideo2 = null;
                SendDateVideo3 = null;
                SendDateVideo4 = null;
            }

            DisplayEmbeddedVideo = new Dictionary<int, bool>();

            Videos = (await VideoService.GetVideos(brokerId)).ToList();
            foreach (Video video in Videos)
            {
                video.Identifier = "Files";
                DisplayEmbeddedVideo[video.Id] = false;
                if (IsAdmin)
                {
                    video.BrokerName = Brokers.FirstOrDefault(x => x.Id == video.BrokerId)?.Name;
                }
                if (video.WelcomeVideo)
                {
                    WelcomeVideo = video;
                    DisplayEmbeddedVideo[video.Id] = false;
                    video.Identifier = "Welcome";
                }
                else if (video.BirthdayVideo)
                {
                    BirthdayVideo = video;
                    DisplayEmbeddedVideo[video.Id] = false;
                    video.Identifier = "Birthday";
                }
                else if (video.MortgageVideo)
                {
                    MortgageVideo = video;
                    DisplayEmbeddedVideo[video.Id] = false;
                    video.Identifier = "Mortgage";
                }
                else if (video.InsuranceVideo)
                {
                    InsuranceVideo = video;
                    DisplayEmbeddedVideo[video.Id] = false;
                    video.Identifier = "Insurance";
                }
                else if(video.SendDateVideo)
                {
                    if (SendDateVideo1 == null)
                    {
                        SendDateVideo1 = video;
                        DisplayEmbeddedVideo[video.Id] = false;
                        video.Identifier = "SendDateVideo1";
                    }
                    else if (SendDateVideo2 == null)
                    {
                        SendDateVideo2 = video;
                        DisplayEmbeddedVideo[video.Id] = false;
                        video.Identifier = "SendDateVideo2";
                    }
                    else if (SendDateVideo3 == null)
                    {
                        SendDateVideo3 = video;
                        DisplayEmbeddedVideo[video.Id] = false;
                        video.Identifier = "SendDateVideo3";
                    }
                    else if (SendDateVideo4 == null)
                    {
                        SendDateVideo4 = video;
                        DisplayEmbeddedVideo[video.Id] = false;
                        video.Identifier = "SendDateVideo4";
                    }
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
