using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace BrokerIQ.Online.Pages
{
    
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;
    using MudBlazor;
    using BrokerIQ.Dto.Models;
    using BrokerIQ.Online.Server.Models;
    using BrokerIQ.Online.Services.Interface;
    using BrokerIQ.Online.Server.Shared;

    public class VideoListBase : ComponentBase
    {
        protected string Message = string.Empty;
        protected string StatusClass = string.Empty;
        protected bool Saved;

        public string VideoName { get; set; }
        public string ExtensionName { get; set; }
        public bool RenameUploadVisibility { get; set; }

        public string status;

        [Inject]
        public IVideoService VideoService { get; set; }

        [Inject]
        public IAccountService AccountService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IDialogService DialogService { get; set; }

        [Inject]
        public IBrokerService BrokerService { get; set; }

        [Inject]
        public IEmailService EmailService { get; set; }

        public List<Video> Videos { get; set; }

        public int BrokerId { get; set; }

        public IBrowserFile fileListEntry;

        public string SpinnerVisible { get; set; }

        protected override async Task OnInitializedAsync()
        {
            SpinnerVisible = "display:none";
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            try
            {
                var user = await AccountService.GetUser();
                if (user == null)
                {
                    throw new Exception();
                } 
                if(user.IsAdmin)
                {
                    BrokerId = 0;
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
            await VerifyBroker();
            var dialogParams = new DialogParameters();
            dialogParams.Add("Message", "Are you sure you want to delete this video?");
            var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Cancelled)
            {
                bool succeeded = await VideoService.DeleteVideo(name);

                if (succeeded)
                {
                    StatusClass = "alert-success";
                    Message = "Deleted successfully";
                    Saved = true;
                }
                else
                {
                    StatusClass = "alert-danger";
                    Message = "Something went wrong deleting the video. Please try again.";
                    Saved = false;
                }
            }
        }

        protected async Task SetBirthdayVideo(string name)
        {
            await VerifyBroker();
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
                    StatusClass = "alert-success";
                    Message = "Birthday video set successfully";
                    Saved = true;
                }
                else
                {
                    StatusClass = "alert-danger";
                    Message = "Something went wrong setting the birthday video. Please try again.";
                    Saved = false;
                }
            }
        }

        public async Task UploadButtonPushed()
        {
            RenameUploadVisibility = false;

            await VerifyBroker();

            bool available = await VideoService.NameAvailable(VideoName);
            if (!available)
            {
                StatusClass = "alert-danger";
                Message = "Video with that name already exists. Please try again with a different name.";
                Saved = true;
            }
            else
            {
                SpinnerVisible = "display:block";

                if (fileListEntry != null)
                {
                    var memoryStream = new MemoryStream();
                    await fileListEntry.OpenReadStream(int.MaxValue).CopyToAsync(memoryStream);
                    memoryStream.Position = 0;  
                    bool succeeded = await VideoService.UploadVideo(VideoName + ExtensionName, memoryStream, BrokerId);

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
                        StatusClass = "alert-success";
                        Message = "Uploaded successfully";
                        Saved = true;
                    }
                    else
                    {
                        StatusClass = "alert-danger";
                        Message = "Something went wrong adding the video. Please try again.";
                        Saved = true;
                    }

                }
                SpinnerVisible = "display:none";
            }
        }

        protected async Task VerifyBroker()
        {
            bool verified;
            try
            {
                var response = await BrokerService.VerifyBroker(BrokerId);
                verified = response.BoolResult;
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


        protected void NavigateToOverview()
        {
            Saved = false;
        }
    }
}
