using System.Threading.Tasks;
using BrokerIQ.Online.Server.Extensions;
using BrokerIQ.Online.Services;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Broker.Components
{
    public partial class BrokerDetails : ComponentBase
    {
        [Parameter]
        public Online.Models.Broker CurrentBroker { get; set; }

        [Inject]
        public ISnackbar Snackbar { get; set; }

        [Inject]
        public IBrokerService BrokerService { get; set; }

        [Inject]
        public IAccountService AccountService { get; set; }

        protected string StatusClass = string.Empty;

        protected string Message = string.Empty;

        protected async Task SetUseBrokerPhoneNumber()
        {
            if (!CurrentBroker.TwoFactorUseBrokerPhoneNumber)
            {
                CurrentBroker.TwoFactorPhoneNumber = CurrentBroker.TelephoneNumber.GetFormattedPhoneNumber();
            }
            else
            {
                CurrentBroker.TwoFactorPhoneNumber = "";
            }
        }

        protected void HandleInvalidSubmit()
        {
            StatusClass = "alert-danger";
            Message = "There are some validation errors. Please try again.";
        }

        protected async Task HandleValidSubmit()
        {
            
            StatusClass = "alert-success";
            Message = "Broker updated successfully.";
            try
            {
                await BrokerService.UpdateBroker(CurrentBroker);

                Snackbar.Add("Details updated successfully", Severity.Success);
            }
            catch
            {
                Snackbar.Add("Unable to update details.", Severity.Error);
            }
        }

        protected async Task SetTwoFactorEnabled()
        {
            bool success = await AccountService.ToggleTwoFactor(CurrentBroker.EmailAddress, !CurrentBroker.TwoFactorEnabled);

            if (success)
            {
                Snackbar.Add("Details updated successfully", Severity.Success);
            }
            else
            {
                Snackbar.Add("Unable to update details.", Severity.Error);
            }
        }
    }
}