using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BrokerIQ.Dto.Enum;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Components;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Settings.Components
{
    public partial class DefinedMessageTable
    {
        [Inject]
        private IDialogService DialogService { get; set; }

        [Inject]
        private ISnackbar Snackbar { get; set; }

        [Inject]
        public IBrokerDefinedMessageService BrokerDefinedMessageService { get; set; }

        [Parameter]
        public User User { get; set; }

        [Parameter]
        public Online.Models.Broker Broker { get; set; }

        private ICollection<BrokerDefinedMessageDto> DefinedMessages { get; set; }

        protected override async Task OnInitializedAsync()
        {
            DefinedMessages = new List<BrokerDefinedMessageDto>(await BrokerDefinedMessageService.GetAllForCurrentBroker());
        }

        protected static string GetReminderTargetName(int targetId)
        {
            return ((ReminderTargetEnum)targetId).ToString();
        }

        private async Task RemoveDefinedMessage(BrokerDefinedMessageDto message)
        {
            var parameters = new DialogParameters
            {
                { "ContentText", $"Do you really want to delete \"{message.Prompt}\" message? This cannot be undone." },
                { "ButtonText", "Delete" },
                { "Color", Color.Error }
            };

            var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };

            var result = await DialogService.Show<ConfirmationDialog>("Delete", parameters, dialogOptions).Result;

            if (!result.Cancelled)
            {
                var wasSuccessfull = await BrokerDefinedMessageService.Delete(message);

                if (wasSuccessfull)
                {
                    Snackbar.Add("Defined message deleted successfully", Severity.Success);
                }
                else
                {
                    Snackbar.Add("Unable to delete defined message. Please try again.", Severity.Error);
                }
            }

            await ReloadDefinedMessages();
        }

        private async Task SaveDefinedMessage(BrokerDefinedMessageDto template)
        {
            bool wasSuccessfull;

            if (template.Id == 0)
            {
                var newTemplate = new CreateBrokerDefinedMessageDto()
                {
                    BrokerId = template.BrokerId,
                    Prompt = template.Prompt,
                    Message = template.Message,
                    FileName = template.FileName,
                    File = template.File,
                    WelcomeChat = template.WelcomeChat
                };

                wasSuccessfull = await BrokerDefinedMessageService.Create(newTemplate);
            }
            else
            {
                wasSuccessfull = await BrokerDefinedMessageService.Update(template);
            }

            if (wasSuccessfull)
            {
                Snackbar.Add("Defined message saved successfully", Severity.Success);
            }
            else
            {
                Snackbar.Add("Unable to save defined message. Please try again.", Severity.Error);
            }
        }

        private async Task ReloadDefinedMessages()
        {
            DefinedMessages = new List<BrokerDefinedMessageDto>(await BrokerDefinedMessageService.GetAllForCurrentBroker());

            StateHasChanged();
        }

        private async Task EditDefinedMessage(BrokerDefinedMessageDto template)
        {
            var operation = template.Id == 0 ? "Create" : "Edit";
            var title = $"{operation} {template.Prompt} template";
            var parameters = new DialogParameters
            {
                { "Template", template }
            };

            var options = new DialogOptions() { MaxWidth = MaxWidth.Medium, FullWidth = true };

            var result = await DialogService.Show<DefinedMessageDialog>(title, parameters, options).Result;

            if (!result.Cancelled)
            {
                await SaveDefinedMessage(template);
            }

            await ReloadDefinedMessages();
        }
    }
}