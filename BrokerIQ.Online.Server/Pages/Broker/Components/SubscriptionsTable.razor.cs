using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Dto.CreateDto;
using BrokerIQ.Dto.Dto;
using BrokerIQ.Dto.Enum;
using BrokerIQ.Dto.UpdateDto;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Broker.Components
{
    public partial class SubscriptionsTable
    {
        [Inject]
        private IDialogService DialogService { get; set; }

        [Inject]
        private ISnackbar Snackbar { get; set; }

        [Inject]
        public IBrokerSubscriptionService BrokerSubscriptionService { get; set; }

        [Parameter]
        public Online.Models.Broker Broker { get; set; }

        protected IEnumerable<SubscriptionServiceEnum> SystemSubscriptions = new SubscriptionServiceEnum[] { SubscriptionServiceEnum.BrokerIQ, SubscriptionServiceEnum.WhiteLabel, SubscriptionServiceEnum.YAH };

        public bool SubscriptionsAreAvailable
        {
            get
            {
                var availableServices = Enum.GetValues<SubscriptionServiceEnum>();

                var subscribedServices = Broker.Subscriptions.Select(s => (SubscriptionServiceEnum)s.SubscriptionServiceId);

                return availableServices.Except(SystemSubscriptions).Except(subscribedServices).Count() > 0;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await ReloadSubscriptions();
        }

        private async Task SaveSubscription(BrokerSubscriptionDto subscription)
        {
            var wasSuccessfull = false;

            if (subscription.BrokerId == 0)
            {
                wasSuccessfull = await BrokerSubscriptionService.Create(new CreateBrokerSubscriptionDto()
                {
                    BrokerId = Broker.Id,
                    SubscriptionServiceId = subscription.SubscriptionServiceId,
                    StartDate = subscription.StartDate,
                    EndDate = subscription.EndDate,
                });
            }
            else
            {
                wasSuccessfull = await BrokerSubscriptionService.Update(new UpdateBrokerSubscriptionDto()
                {
                    BrokerId = subscription.BrokerId,
                    SubscriptionServiceId = subscription.SubscriptionServiceId,
                    StartDate = subscription.StartDate,
                    EndDate = subscription.EndDate,
                });
            }

            //if (wasSuccessfull)
            //{
            //    Snackbar.Add("Susbscription was saved.", Severity.Success);
            //}
            //else
            //{
            //    Snackbar.Add("Subscription save failed. Please try again.", Severity.Error);
            //}
        }

        private async Task ReloadSubscriptions()
        {
            Broker.Subscriptions = new List<BrokerSubscriptionDto>(await BrokerSubscriptionService.GetAllForBroker(Broker.Id));

            StateHasChanged();
        }

        private async Task EditSubscription(BrokerSubscriptionDto subscription)
        {
            string operation;
            SubscriptionServiceEnum[] toExclude;
            if (subscription.BrokerId == 0)
            {
                operation = "Create";
                toExclude = SystemSubscriptions.ToArray();
            }
            else
            {
                operation = "Edit";
                if (SystemSubscriptions.Contains((SubscriptionServiceEnum)subscription.SubscriptionServiceId))
                {
                    toExclude = Enum.GetValues<SubscriptionServiceEnum>().Except(SystemSubscriptions).ToArray();
                }
                else
                {
                    toExclude = SystemSubscriptions.ToArray();
                }
            }

            var title = $"{operation} subscription";
            var parameters = new DialogParameters
            {
                { "Subscription", subscription },
                { "Exclude", toExclude}
            };

            var options = new DialogOptions() { MaxWidth = MaxWidth.Small, FullWidth = true };

            var result = await DialogService.Show<SubscriptionDialog>(title, parameters, options).Result;

            if (!result.Canceled)
            {
                await SaveSubscription(subscription);
            }

            await ReloadSubscriptions();
        }
    }
}