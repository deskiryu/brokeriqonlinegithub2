using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace BrokerIQ.Online.Pages
{
    using Microsoft.AspNetCore.Components;
    using MudBlazor;
    using BrokerIQ.Online.Server.Models;
    using BrokerIQ.Online.Services.Interface;
    using BrokerIQ.Online.Server.Shared;

    public class AudioListBase : ComponentBase
    {
        protected string Message = string.Empty;
        protected string StatusClass = string.Empty;

        public string AudioName { get; set; }
        public string ExtensionName { get; set; }
        public bool RenameUploadVisibility { get; set; }

        public string status;

        [Inject]
        public IAudioService AudioService { get; set; }

        [Inject]
        public IAccountService AccountService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IDialogService DialogService { get; set; }

        [Inject]
        public IBrokerService BrokerService { get; set; }

        public List<Audio> Audios { get; set; }

        public int BrokerId { get; set; }

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
                if (user.IsAdmin)
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
                Audios = (await AudioService.GetAudios(BrokerId)).ToList();
                StateHasChanged();
            }
            catch
            {
                NavigationManager.NavigateTo($"account/logout");
            }
        }


        public async Task OnCancelPushed(){
            RenameUploadVisibility = false;
        }


        protected async Task DeleteAudio(string name)
        {
            await VerifyBroker();
            var dialogParams = new DialogParameters();
            dialogParams.Add("Message", "Are you sure you want to delete this Audio?");
            var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Cancelled)
            {
                bool succeeded = await AudioService.DeleteAudio(name);

                if (succeeded)
                {
                    RefreshAudioListWithDialogMessage(succeeded, "Deleted successfully");
                }
                else
                {
                    RefreshAudioListWithDialogMessage(succeeded, "Something went wrong deleting the Audio. Please try again.");
                }
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

        public async Task UploadButtonPushed()
        {
            // is this a TODO? Moved from AudioList.razor
        }

        protected async Task AddRecording()
        {
            await VerifyBroker();
            var dialogParams = new DialogParameters();
            dialogParams.Add("BrokerId", BrokerId);
            var dialogOptions = new DialogOptions()
            {
                MaxWidth = MaxWidth.Medium
            };

            var result = await DialogService.Show<AudioRecordDialog>("Record voice message", dialogParams, dialogOptions).Result;
            if (!result.Cancelled)
            {
                NavigationManager.NavigateTo($"/audiolist");
            }
        }

        /// <summary>
        /// Refreshes audiolist on page if desired. Displays appropriate dialog message.
        /// </summary>
        /// <param name="success">Success of prior API call</param>
        /// <param name="message">Message to be displayed in dialog</param>
        private async void RefreshAudioListWithDialogMessage(bool success, string message)
        {
            if (success)
            {
                Audios = (await AudioService.GetAudios(BrokerId)).ToList();
                StateHasChanged();
            }
            var responseParams = new DialogParameters();
            responseParams.Add("Message", message);
            await DialogService.Show<AlertDialog>("Information", responseParams).Result;
        }
    }
}
