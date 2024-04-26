using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Dto;
using BrokerIQ.Dto.Enum;
using BrokerIQ.Dto.Models;
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

        private List<BrokerSubscriptionDto> Subscriptions { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Subscriptions = new List<BrokerSubscriptionDto>(await BrokerSubscriptionService.GetAllForBroker(Broker.Id));

            ServicesAvailable = Enum.GetNames(typeof(SubscriptionServiceEnum)).Length > Subscriptions.Count;
        }

        public bool ServicesAvailable { get; set; }

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

            // if (wasSuccessfull)
            // {
            //     Snackbar.Add("Susbscription was saved.", Severity.Success);
            // }
            // else
            // {
            //     Snackbar.Add("Subscription save failed. Please try again.", Severity.Error);
            // }
        }

        private async Task ReloadSubscriptions()
        {
            Subscriptions = new List<BrokerSubscriptionDto>(await BrokerSubscriptionService.GetAllForBroker(Broker.Id));

            ServicesAvailable = Enum.GetNames(typeof(SubscriptionServiceEnum)).Length > Subscriptions.Count;

            StateHasChanged();
        }

        private async Task EditSubscription(BrokerSubscriptionDto subscription)
        {
            var operation = subscription.BrokerId == 0 ? "Create" : "Edit";
            var title = $"{operation} subscription";
            var parameters = new DialogParameters
            {
                { "Subscription", subscription }
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