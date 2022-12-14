namespace BrokerIQ.Online.Pages
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using BrokerIQ.Online.Server.Shared;
    using Microsoft.AspNetCore.Components;
    using Models;
    using MudBlazor;
    using Services.Interface;

    public class BrokerStaffEditBase : ComponentBase
    {
        private int id;

        [Inject]
        public IBrokerStaffService BrokerStaffService { get; set; }

        [Inject]
        public IAccountService AccountService { get; set; }


        [Inject] 
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IDialogService DialogService { get; set; }

        public BrokerStaff _brokerStaff { get; set; }

        public int BrokerId { get; set; }

        protected string Message = string.Empty;
        protected string StatusClass = string.Empty;
        protected bool Saved;

        [Parameter]
        public string BrokerStaffId { get; set; }

        public BrokerStaffEditBase()
        {
            _brokerStaff = new BrokerStaff();
        }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var user = await AccountService.GetUser();
                BrokerId = 0;                    
                id = Int32.Parse(BrokerStaffId);

                if (user.IsBroker || user.IsAdmin)
                {
                    if (id > 0)
                    {
                        _brokerStaff = (await BrokerStaffService.GetBrokerStaff(id));
                    }
                }
            }
            catch
            {
                NavigationManager.NavigateTo($"account/logout");
            }
        }

        protected void HandleInvalidSubmit()
        {
            StatusClass = "alert-danger";
            Message = "There are some validation errors. Please try again.";
        }

        protected async Task HandleValidSubmit()
        {
            try
            {
                await BrokerStaffService.UpdateBrokerStaff(_brokerStaff);
            }
            catch
            {
                StatusClass = "alert-danger";
                Message = "Something went wrong updating the Broker Staff. Please try again.";
                Saved = true;
                return;
            }

            StatusClass = "alert-success";
            Message = "Broker staff updated successfully.";
            Saved = true;
        }

        protected void NavigateToOverview()
        {
            NavigationManager.NavigateTo($"/brokerstafflist");
        }

        public async Task DeleteCustomer()
        {
            var dialogParams = new DialogParameters();
            dialogParams.Add("Message", "Are you sure you want to delete this employee?");
            var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Cancelled)
            {
                await BrokerStaffService.DeleteBrokerStaff(_brokerStaff.Id);
                NavigationManager.NavigateTo($"/brokerstafflist");
            }

        }

        protected async Task ResendEmailBrokerStaff()
        {
            var dialogParams = new DialogParameters();
            dialogParams.Add("Message", $"A verify email will be sent to {_brokerStaff.EmailAddress}. Continue? ");
            var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Cancelled)
            {
                var resent = await AccountService.ResendEmailBroker(_brokerStaff.EmailAddress);
                if (resent)
                {
                    var responseParams = new DialogParameters();
                    responseParams.Add("Message", "Resent successfully");
                    await DialogService.Show<AlertDialog>("Information", responseParams).Result;
                }
                else
                {
                    var responseParams = new DialogParameters();
                    responseParams.Add("Message", "The resend email failed.");
                    await DialogService.Show<AlertDialog>("Information", responseParams).Result;
                }
                NavigationManager.NavigateTo($"/clientlist");
            }
        }
    }
}
