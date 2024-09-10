using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Dto;
using BrokerIQ.Dto.Enum;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Components;
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

        private ICollection<ReminderOptionDto> ReminderOptions { get; set; }

        protected override async Task OnInitializedAsync()
        {
            ReminderOptions = new List<ReminderOptionDto>(await BrokerReminderOptionService.GetAllForCurrentBroker());
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

            var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };

            var result = await DialogService.Show<ConfirmationDialog>("Delete", parameters, dialogOptions).Result;

            if (!result.Canceled)
            {
                await BrokerReminderOptionService.Delete(option);
            }

            await ReloadReminderOptions();
        }

        private async Task SaveReminderOptions()
        {
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
        }

        private async Task ReloadReminderOptions()
        {
            ReminderOptions = new List<ReminderOptionDto>(await BrokerReminderOptionService.GetAllForCurrentBroker());

            StateHasChanged();
        }

        private async Task EditReminderOption(ReminderOptionDto option)
        {
            var operation = option.Id == 0 ? "Create" : "Edit";
            var title = $"{operation} {Enum.GetName((ReminderTargetEnum)option.ReminderTargetId)} {Enum.GetName((ReminderTypeEnum)option.ReminderTypeId)} reminder";
            var parameters = new DialogParameters
            {
                { "Option", option },
                { "BrokerIdentifier", Broker.BrokerIdentifier}
            };

            var options = new DialogOptions() { MaxWidth = MaxWidth.Medium, FullWidth = true };

            var result = await DialogService.Show<ReminderOptionDialog>(title, parameters, options).Result;

            if (!result.Canceled)
            {
                ReminderOptionDto updated = result.Data as ReminderOptionDto;

                option.ReminderTypeId = updated.ReminderTypeId;

                option.NotificationPeriod = updated.NotificationPeriod;

                option.ReminderTargetId = updated.ReminderTargetId;

                option.MessageContent = updated.MessageContent;

                if (option.Id == 0) ReminderOptions.Add(option);

                await SaveReminderOptions();
            }

            await ReloadReminderOptions();
        }

        private string FormatOptionForDisplay(string template)
        {
            if (!template.Contains("<--") || !template.Contains("-->")) return template;

            var linkStart = template.IndexOf("<--");
            var linkEnd = template.IndexOf("-->");
            var url = template.Substring(linkStart + 3, linkEnd - linkStart - 3);

            return template.Replace("<--", "<a target=\"_blank\" href=\"").Replace("-->", $"\">{url}</a>");
        }
    }
}