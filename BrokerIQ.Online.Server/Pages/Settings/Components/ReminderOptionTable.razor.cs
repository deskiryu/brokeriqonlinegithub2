using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Dto;
using BrokerIQ.Dto.Enum;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Components;
using BrokerIQ.Online.Server.Models;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Settings.Components
{
    public partial class ReminderOptionTable
    {
        [Inject]
        private IDialogService DialogService { get; set; }

        [Inject]
        private ISnackbar Snackbar { get; set; }

        [Inject]
        public IBrokerReminderOptionService BrokerReminderOptionService { get; set; }

        [Parameter]
        public User User { get; set; }

        [Parameter]
        public Online.Models.Broker Broker { get; set; }

        private IEnumerable<ReminderOptionDto> ReminderOptions { get; set; }

        protected override async Task OnInitializedAsync()
        {
            ReminderOptions = await BrokerReminderOptionService.GetAllForCurrentBroker();
        }

        protected static string GetReminderTargetName(int targetId)
        {
            return ((ReminderTargetEnum)targetId).ToString();
        }

        private async Task RemoveReminderOption(ReminderOptionDto option)
        {
            var parameters = new DialogParameters
            {
                { "ContentText", "Do you really want to delete these reminder options? This process cannot be undone." },
                { "ButtonText", "Delete" },
                { "Color", Color.Error }
            };

            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };

            var result = await DialogService.Show<ConfirmationDialog>("Delete", parameters, options).Result;

            if (!result.Cancelled)
            {
                option.FirstNotificationPeriod = TimeSpan.Zero;
                option.SecondNotificationPeriod = TimeSpan.Zero;
                option.ThirdNotificationPeriod = TimeSpan.Zero;
                option.FourthNotificationPeriod = TimeSpan.Zero;
                option.FifthNotificationPeriod = TimeSpan.Zero;

                var wasSuccessfull = await BrokerReminderOptionService.UpdateOrCreate(ReminderOptions);


                // TODO : Re introduce these when a fix for the parsing error has been found
                // if (wasSuccessfull)
                // {
                //     Snackbar.Add("Reminder option was removed.", Severity.Success);
                // }
                // else
                // {
                //     Snackbar.Add("Reminder options update failed. Please try again.", Severity.Error);
                // }

                ReminderOptions = await BrokerReminderOptionService.GetAllForCurrentBroker();

                StateHasChanged();
            }
        }
    }
}