using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using BrokerIQ.Online.Models;
using MudBlazor;
using BrokerIQ.Online.Server.Shared;
using BrokerIQ.Online.Services.Interface;
using BrokerIQ.Online.Server.Services.Interface;
using System.Linq;

namespace BrokerIQ.Online.Pages
{
    public class BrokerStaffEditBase : ComponentBase
    {
        private int id;

        [Inject]
        public ISnackbar Snackbar { get; set; }

        [Inject]
        public IBrokerStaffService BrokerStaffService { get; set; }

        [Inject]
        public IAccountService AccountService { get; set; }

        [Inject]
        public IAssignmentService AssignmentService { get; set; }


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

        public bool IsAdmin { get; set; }
        public bool IsMinorAdmin { get; set; }

        public HashSet<Customer> SelectedCustomers { get; set; }

        protected IEnumerable<Customer> AssignedCustomers;

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
                IsAdmin = user.IsAdmin;
                IsMinorAdmin = user.IsMinorAdmin;

                if (user.IsBroker || user.IsAdmin || user.IsMinorAdmin)
                {
                    if (id > 0)
                    {
                        _brokerStaff = await BrokerStaffService.GetBrokerStaff(id);

                        AssignedCustomers = await AssignmentService.GetForEmployee(id);
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
                NavigationManager.NavigateTo($"/brokerstafflist");
            }
        }

        protected async Task SetTwoFactorEnabled()
        {
            bool success = await AccountService.ToggleTwoFactor(_brokerStaff.EmailAddress, !_brokerStaff.TwoFactorEnabled);
            if (success)
            {
                var responseParams = new DialogParameters();
                responseParams.Add("Message", "Two factor set/cleared successfully");
                await DialogService.Show<AlertDialog>("Information", responseParams).Result;
            }
            else
            {
                var responseParams = new DialogParameters();
                responseParams.Add("Message", "The two factor was not changed.");
                await DialogService.Show<AlertDialog>("Information", responseParams).Result;
            }
            NavigationManager.NavigateTo($"/brokerstafflist");
        }

        protected string AssignVisibilityClass()
        {
            return SelectedCustomers != null && SelectedCustomers.Count > 0 ? "visible" : "invisible";
        }

        protected async Task UnassignFromStaff()
        {
            bool? result = await DialogService.ShowMessageBox(
                "Unassign selected customers",
                "Are you sure you want to unassign from this staff member?",
                yesText: "Unassign", cancelText: "Cancel");

            if (result != null)
            {
                try
                {
                    await AssignmentService.Unassign(_brokerStaff, SelectedCustomers.Select(c => c.Id).ToArray());

                    Snackbar.Add("Unassignment was successfull", Severity.Success);

                    SelectedCustomers = null;

                    AssignedCustomers = await AssignmentService.GetForEmployee(id);

                    StateHasChanged();
                }
                catch
                {
                    Snackbar.Add("Unable to remove assignment. Please try again", Severity.Error);
                }
            }
        }
    }
}
