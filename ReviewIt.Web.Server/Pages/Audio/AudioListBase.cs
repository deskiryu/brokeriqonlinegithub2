using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace ReviewIt.Web.Pages
{
    using Microsoft.AspNetCore.Components;
    using MudBlazor;
    using ReviewIt.Web.Server.Models;
    using ReviewIt.Web.Services.Interface;

    public class AudioListBase : ComponentBase
    {
        protected string Message = string.Empty;
        protected string StatusClass = string.Empty;
        protected bool Saved;

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
            var result = await DialogService.Show<ReviewIt.Web.Server.Shared.ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Cancelled)
            {
                bool succeeded = await AudioService.DeleteAudio(name);

                if (succeeded)
                {
                    StatusClass = "alert-success";
                    Message = "Deleted successfully";
                    Saved = true;
                }
                else
                {
                    StatusClass = "alert-danger";
                    Message = "Something went wrong deleting the Audio. Please try again.";
                    Saved = false;
                }
            }
        }

        protected void NavigateToOverview()
        {
            Saved = false;
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
    }
}
