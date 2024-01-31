using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using BrokerIQ.Dto.Enum;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Components;
using BrokerIQ.Online.Services.Interface;

using MudBlazor;
using System;
using AutoMapper.Configuration.Conventions;

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

        private IEnumerable<BrokerDefinedMessageDto> DefinedMessages { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await RefreshMessages();
        }

        private async Task RefreshMessages()
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

        private string FormatTemplateForDisplay(string template)
        {
            if (!template.Contains("<--") || !template.Contains("-->")) return template;

            var linkStart = template.IndexOf("<--");
            var linkEnd = template.IndexOf("-->");
            var url = template.Substring(linkStart + 3, linkEnd - linkStart - 3);

            return template.Replace("<--", "<a target=\"_blank\" href=\"").Replace("-->", $"\">{url}</a>");
        }

        #region Row drag and drop
        private BrokerDefinedMessageDto? draggedItem;

        private int? enterIndex;
        private bool? enterAfterDropZone;

        private void DragStart(BrokerDefinedMessageDto model)
        {
            draggedItem = model;
        }

        private void DragEnter(int index, bool isAfterDropZone)
        {
            if (draggedItem?.SortOrder == index)
            {
                enterIndex = null;
                enterAfterDropZone = null;
            }
            else
            {
                enterIndex = index;
                enterAfterDropZone = isAfterDropZone;
            }
        }

        private async Task DropAsync(int index)
        {
            await DragEnd();
        }

        private async Task DragEnd()
        {
            if (enterIndex.HasValue && draggedItem.SortOrder != enterIndex.Value)
            {
                draggedItem.SortOrder = enterIndex.Value;

                await BrokerDefinedMessageService.Update(draggedItem);

                await RefreshMessages();

                StateHasChanged();
            }

            draggedItem = null;
            enterIndex = null;
            enterAfterDropZone = null;
        }

        private bool IsEntering(int index)
        {
            return enterIndex == index;
        }

        private string IsDraggable()
        {
            return "true";
        }
        #endregion

    }
}