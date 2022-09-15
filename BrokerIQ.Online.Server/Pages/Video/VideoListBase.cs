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

        public Dictionary<string, VideoThumbnail> VideoThumbnails { get; set; }

        public Dictionary<string, bool> DisplayEmbeddedVideo { get; set; }

        public int BrokerId { get; set; }

        [Required]
        public int BrokerListId = 0;

        public IBrowserFile fileListEntry;

        public string SpinnerVisible { get; set; }

        public bool VideoUploading { get; set; }

        public bool VideoScanUploading { get; set; }

        public bool VideoScanning { get; set; }

        public int VideoScanningProgress { get; set; }

        public bool IsAdmin { get; set; }

        protected override async Task OnInitializedAsync()
        {
            SpinnerVisible = "display:none";
            CultureInfo.CurrentCulture = new CultureInfo("en-GB", false);
            StateHasChanged();
            IsAdmin = false;

            try
            {
                var user = await AccountService.GetUser();
                if (user == null)
                {
                    throw new Exception();
                }
                if (user.IsAdmin || user.MasterBrokerId == 0)
                {
                    await VerifyAdmin();
                    IsAdmin = true;
                    BrokerId = 0;
                    Brokers = (await BrokerService.GetBrokers()).ToList();
                }
                else if (user.IsBroker || user.IsBrokerStaff)
                {
                    BrokerId = user.MasterBrokerId;
                }
                else
                {
                    throw new Exception();
                }

                Videos = (await VideoService.GetVideos(BrokerId)).ToList();
                VideoThumbnails = (await VideoService.GetVideoThumbnails(BrokerId));
                DisplayEmbeddedVideo = new Dictionary<string, bool>();
                foreach (Video video in Videos)
                {
                    DisplayEmbeddedVideo[video.Name] = false;
                    if (user.IsAdmin)
                    {
                        video.Broker = Brokers.FirstOrDefault(x => x.Id == video.BrokerId)?.Name;
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

        public async Task OnCancelPushed(){
            RenameUploadVisibility = false;
        }


        protected async Task DeleteVideo(string name)
        {
            await VerifyAccess();
            var dialogParams = new DialogParameters();
            dialogParams.Add("Message", "Are you sure you want to delete this video?");
            var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Cancelled)
            {
                bool succeeded = await VideoService.DeleteVideo(name, BrokerId);

                if (succeeded)
                {
                    // cleanup video from in video list to be displayed
                    foreach (Video video in Videos)
                    {
                        if (video.Name == name)
                        {
                            Videos.Remove(video);
                            break;
                        }
                    }
                    
                    string thumbnailName = $"{name}.jpeg";
                    bool thumbnail_deleted = await VideoService.DeleteVideoThumbnail(thumbnailName, BrokerId);
                    if (!thumbnail_deleted)
                    {
                        // just log to console for now, user doesn't need to know about thumbnail deletion
                        Console.WriteLine("Something went wrong deleting the thumbnail");
                    }

                    // clean up dictionaries for displaying and storing thumbnails in memory
                    VideoThumbnails.Remove(thumbnailName);
                    DisplayEmbeddedVideo.Remove(name);
                    await RefreshVideosWithDialogMessage(succeeded, "Deleted successfully");
                }
                else
                {
                    await RefreshVideosWithDialogMessage(succeeded, "Something went wrong deleting the video. Please try again.");
                }
            }
        }

        protected async Task SetBirthdayVideo(string name)
        {
            await VerifyAccess();

            if (await CheckIsAdmin())
            {
                await RefreshVideosWithDialogMessage(true, "Admin cannot set birthday video");
                return;
            }

            var dialogParams = new DialogParameters();
            var videoAlreadyChecked = Videos.FirstOrDefault(x => x.Name == name);
            bool alreadyChecked = false;
            if (videoAlreadyChecked != null)
            {
                alreadyChecked = videoAlreadyChecked.BirthdayVideo;
            }
            if (alreadyChecked)
            {
                dialogParams.Add("Message", "There will be no birthday video. Continue?");
            }
            else
            {
                dialogParams.Add("Message", "This video will be sent to clients on their birthday. Continue?");
            }


            var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Cancelled)
            {
                var returned = await VideoService.SetBirthdayVideo(name, BrokerId, !alreadyChecked);
                if (returned.Item1)
                {
                    returned.Item1 = await BrokerService.UpdateBrokerBirthdayVideoUrl(BrokerId, returned.Item2);
                }
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

        protected async Task<bool> SetVideoSendDate(string name, DateTime? date)
        {
            await VerifyAccess();
            if (await CheckIsAdmin())
            {
                await RefreshVideosWithDialogMessage(true, "Admin cannot set video date");
                return false;
            }

            var returned = await VideoService.SetVideoSendDate(name, BrokerId, date);

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

        protected async Task SetSendDateTick(string name)
        {
            await VerifyAccess();

            if (await CheckIsAdmin())
            {
                await RefreshVideosWithDialogMessage(true, "Admin cannot set send date tick");
                return;
            }

            var dialogParams = new DialogParameters();
            var videoAlreadyChecked = Videos.FirstOrDefault(x => x.Name == name);
            bool alreadyChecked = false;
            if (videoAlreadyChecked != null)
            {
                alreadyChecked = videoAlreadyChecked.SendDateTick;
            }
            if (alreadyChecked)
            {
                dialogParams.Add("Message", "This video will not send on this date. Continue?");
            }
            else
            {
                dialogParams.Add("Message", "This video will be sent to all cilents on this date. Continue?");
            }


            var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Cancelled)
            {
                var returned = await VideoService.SetVideoSendDateTick(name, BrokerId, !alreadyChecked);

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

        protected async Task SetVetted(string name)
        {
            await VerifyAccess();

            if (!(await CheckIsAdmin()))
            {
                await RefreshVideosWithDialogMessage(true, "Only admin can vet");
                return;
            }

            var dialogParams = new DialogParameters();
            var videoAlreadyChecked = Videos.FirstOrDefault(x => x.Name == name);
            bool alreadyChecked = false;
            if (videoAlreadyChecked != null)
            {
                alreadyChecked = videoAlreadyChecked.Vetted;
            }

            var returned = await VideoService.SetVetted(name, !alreadyChecked);

            if (returned.Item1)
            {
                await RefreshVideos();
            }
            else
            {
                await RefreshVideosWithDialogMessage(false, "Something went wrong setting vetted. Please try again.");
            }

        }
        /// <summary>
        /// Uploads a file to the OPSWAT service which scans the file for viruses.
        /// Once uploaded, probes the OPSWAT service for the result of the scan.
        /// Updates the UI to reflect the status of the scan.
        /// </summary>
        /// <param name="fileName">Video to be uploaded</param>
        /// <param name="ms">Memory stream representation of video</param>
        /// <returns>boolean result of scan</returns>
        private async Task<bool> ScanVideo(String fileName, MemoryStream ms)
        {
            bool scanPass = false;
            string dataId = MetaDefenderCoreService.AnalyseFile(fileName, ms).Result;

            VideoScanUploading = false;
            StateHasChanged();

            if (dataId != null)
            {
                int attempts = 60;
                VideoScanning = true;
                StateHasChanged();

                while (attempts > 0)
                {
                    dynamic resultJson = MetaDefenderCoreService.FetchAnalysisResult(dataId).Result;
                    if (resultJson != null)
                    {
                        var progressPercentage = resultJson["scan_results"]["progress_percentage"];
                        VideoScanningProgress = progressPercentage;
                        StateHasChanged();

                        if (progressPercentage == 100)
                        {
                            string result = resultJson["process_info"]["result"];
                            string resultFiletype = resultJson["file_info"]["file_type_category"];
                            if (result.Equals("Allowed")&& resultFiletype.Equals("M"))
                            {
                                scanPass = true;
                            }
                            else
                            {
                                Console.WriteLine($"Scan failed for {fileName} dataId {dataId} result {result} fileType {resultFiletype}");
                                await DisplayErrorDialog($"The antivirus scan failed for {fileName}. Result - {result} - {resultFiletype}");
                            }
                            return scanPass;
                        }
                    }
                    attempts--;
                }
                await DisplayErrorDialog($"Something went wrong retrieving the results of the anti-virus scan. Try uploading the file again.");
            }
            else
            {
                await DisplayErrorDialog($"Something went wrong uploading {fileName} to anti-virus scanning service. Try again.");
            }

            return scanPass;
        }

        public async Task UploadButtonPushed()
        {
            VideoScanUploading = false;
            VideoScanning = false;
            VideoScanningProgress = 0;
            VideoUploading = false;
            RenameUploadVisibility = false;

            await VerifyAccess();

            bool available = await VideoService.NameAvailable(VideoName, BrokerId);
            if (!available)
            {
                StatusClass = "alert-danger";
                Message = "Video with that name already exists. Please try again with a different name.";
                Saved = true;
            }
            else
            {
                if (fileListEntry != null)
                {
                    SpinnerVisible = "display:block";
                    VideoScanUploading = true;
                    StateHasChanged();

                    var memoryStream = new MemoryStream();
                    await fileListEntry.OpenReadStream(int.MaxValue).CopyToAsync(memoryStream);

                    string fileName = $"{VideoName}{ExtensionName}";
                    bool scanPass = await ScanVideo(fileName, memoryStream);

                    VideoScanning = false;
                    StateHasChanged();

                    if (scanPass)
                    {
                        VideoUploading = true;
                        StateHasChanged();

                        memoryStream.Position = 0;
                        var localBrokerId = BrokerId;
                        if (IsAdmin)
                        {
                            localBrokerId = BrokerListId;
                        }
                        bool succeeded = await VideoService.UploadVideo(VideoName + ExtensionName, memoryStream, localBrokerId);

                        status = $"Finished loading {fileListEntry.Size} bytes from {fileListEntry.Name}";

                        if (succeeded)
                        {
                            var email = new CreateEmailDto
                            {
                                Subject = "New Video",
                                Content = $"A video {VideoName + ExtensionName} has been added and needs to be vetted"
                            };

                            try
                            {
                                await EmailService.SendEmail(email);
                            }
                            catch
                            {

                            }
                        }

                        if (succeeded)
                        {
                            // fetch thumbnail for uploaded video (might not be instantly available as generated by FunctionVideoThumbnail Azure function)
                            // if issue fetching the image after 10 seconds, just proceed without thumbnail
                            int attempts = 20;

                            while (attempts > 0)
                            {
                                string thumbnailName = $"{VideoName}{ExtensionName}.jpeg";
                                var thumbnail = await VideoService.GetVideoThumbnail(thumbnailName, BrokerId);
                                if (thumbnail != null && thumbnail.Data != null)
                                {
                                    VideoThumbnails[thumbnailName] = thumbnail; 
                                    break;
                                }
                                await Task.Delay(1000);
                                attempts--;
                            }

                            DisplayEmbeddedVideo[$"{VideoName}{ExtensionName}"] = false;
                            await RefreshVideosWithDialogMessage(succeeded, $"Uploaded successfully");
                        }
                        else
                        {
                            await RefreshVideosWithDialogMessage(succeeded, "Something went wrong adding the video. Please try again.");
                        }
                    }
                }

                VideoUploading = false;
                SpinnerVisible = "display:none";
                StateHasChanged();
            }
        }

        protected async Task VerifyAccess()
        {
            var user = await AccountService.GetUser();
            if (user.IsAdmin || user.MasterBrokerId == 0)
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

        protected void ShowVideoPlayer(string videoName)
        {
            DisplayEmbeddedVideo[videoName] = true;
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
            if (IsAdmin)
            {
                foreach (Video video in Videos)
                {
                    video.Broker = Brokers.FirstOrDefault(x => x.Id == video.BrokerId)?.Name;
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

        protected async Task UpdateVideoSendDate(DateTime? sendDate, String name)
        {
            // Set the new value first, to avoid double firing from the DateChanged event.
            var existingVideo = this.Videos.First(item => item.Name == name);
            DateTime? oldDate = existingVideo.SendDate;
            existingVideo.SendDate = sendDate;

            await SetVideoSendDate(name, sendDate);
        }
    }
}
