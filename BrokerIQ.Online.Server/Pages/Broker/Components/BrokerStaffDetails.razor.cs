using System.Threading.Tasks;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Broker.Components
{
    public partial class BrokerStaffDetails : ComponentBase
    {
        [Parameter]
        public Online.Models.BrokerStaff CurrentStaff { get; set; }

        [Inject]
        public ISnackbar Snackbar { get; set; }

        [Inject]
        public IAccountService AccountService { get; set; }

        [Inject]
        public IBrokerStaffService BrokerStaffService { get; set; }

        protected string StatusClass = string.Empty;

        protected string Message = string.Empty;

        protected async Task SetTwoFactorEnabled()
        {
            bool success = await AccountService.ToggleTwoFactor(CurrentStaff.EmailAddress, !CurrentStaff.TwoFactorEnabled);

            if (success)
            {
                Snackbar.Add("Details updated successfully", Severity.Success);
            }
            else
            {
                Snackbar.Add("Unable to update details.", Severity.Error);
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
                await BrokerStaffService.UpdateBrokerStaff(CurrentStaff);

                Snackbar.Add("Details updated successfully", Severity.Success);
            }
            catch
            {
                Snackbar.Add("Unable to update details.", Severity.Error);
            }
        }
    }
}