using System;
using System.Threading.Tasks;
using BrokerIQ.Dto.Dto;
using BrokerIQ.Dto.Enum;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Broker.Components
{
    public partial class SubscriptionDialog
    {
        [Microsoft.AspNetCore.Components.CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Microsoft.AspNetCore.Components.Parameter]
        public BrokerSubscriptionDto Subscription { get; set; }

        MudForm form;

        protected DateTime? StartDate;

        protected DateTime? EndDate;

        protected override async Task OnInitializedAsync()
        {
            StartDate = Subscription.StartDate;
            EndDate = Subscription.EndDate;
        }

        void Submit()
        {
            form.Validate();

            if (form.IsValid)
            {
                Subscription.StartDate = StartDate.Value;
                Subscription.EndDate = EndDate.Value;

                MudDialog.Close(DialogResult.Ok(Subscription));
            };
        }

        void Cancel() => MudDialog.Cancel();

        protected static string IsValidSubscriptionService(int i)
        {
            return Enum.IsDefined(typeof(SubscriptionServiceEnum), i) ? null : "Please select a subscription service";
        }
    }
}