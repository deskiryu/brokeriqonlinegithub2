using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;

using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Server.AppSettings;

using MudBlazor;
using BrokerIQ.Online.Server.Extensions;
using BrokerIQ.Online.Services.Interface;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Components;

namespace BrokerIQ.Online.Pages
{
    public class ClientAppointmentBase: ComponentBase
    {
        [Inject]
        private IDialogService DialogService { get; set; }

        [Inject]
        private ISnackbar Snackbar { get; set; }

        [Inject]
        public ICustomerAppointmentService CustomerAppointmentService { get; set; }

        [Parameter]
        public User User { get; set; }

        [Parameter]
        public BrokerIQ.Online.Models.Broker Broker { get; set; }

        protected IEnumerable<CustomerAppointmentDto> DefinedAppointments { get; set; }
        private int LastSortOrder { get; set; }

        public string SpinnerVisible { get; set; }

        private int BrokerID { get; set; }

        protected override async Task OnInitializedAsync()
        {
            SpinnerVisible = "display:none";
            await GetAppointments();
        }

        private async Task GetAppointments()
        {
            DefinedAppointments = new List<CustomerAppointmentDto>(await CustomerAppointmentService.GetByBrokerID(BrokerID));
        }

        public async Task RemoveCustomerAppointment(CustomerAppointmentDto message)
        {
            var parameters = new DialogParameters
            {
                { "ContentText", $"Do you really want to delete this appointment? No reminders will be sent to client. The corresponding appointment in Calendly should be deleted if not already." },
                { "ButtonText", "Delete" },
                { "Color", Color.Error }
            };

            var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };

            var result = await DialogService.Show<ConfirmationDialog>("Delete", parameters, dialogOptions).Result;

            if (!result.Cancelled)
            {
                var wasSuccessfull = await CustomerAppointmentService.Delete(message.Id);

                if (wasSuccessfull)
                {
                    Snackbar.Add("Appointment deleted successfully", Severity.Success);
                }
                else
                {
                    Snackbar.Add("Unable to delete defined message. Please try again.", Severity.Error);
                }
            }

            await ReloadDefinedAppointments();
        }

        private async Task ReloadDefinedAppointments()
        {
            await GetAppointments();

            StateHasChanged();
        }

    }
}