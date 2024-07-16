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
using System.Linq;

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
        private int LastSortOrder { get; set; }

        public string SpinnerVisible { get; set; }

        public bool MaxTemplatesReached
        {
            get
            {
                if (Broker is null) return true; // disabled while broker is not set

                if (Broker.BrokerIdentifier is null || !Broker.BrokerIdentifier.IdentifierFound) return false; // non white labels always unlimited

                if (DefinedMessages is null) return true; // disabled while messages not loaded

                return Broker.BrokerIdentifier.IsLimitedBroker && DefinedMessages.Count() >= Broker.BrokerIdentifier.MaxTemplates;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            SpinnerVisible = "display:none";
            await RefreshMessages();
        }

        private async Task RefreshMessages()
        {
            DefinedMessages = new List<BrokerDefinedMessageDto>(await BrokerDefinedMessageService.GetAllForCurrentBroker());
            LastSortOrder = 0;
            if (DefinedMessages != null && DefinedMessages.Any())
            {
                var maxSort = DefinedMessages.OrderByDescending(item => item.SortOrder).FirstOrDefault();
                if (maxSort != null)
                {
                    LastSortOrder = maxSort.SortOrder;
                }
                else
                {
                    LastSortOrder = DefinedMessages.Count();
                }
            }
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

            if (!result.Canceled)
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
            await RefreshMessages();

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
            if (template.Message == null)
            {
                template.Message = string.Empty;
            }

            var options = new DialogOptions() { MaxWidth = MaxWidth.Medium, FullWidth = true };

            var result = await DialogService.Show<DefinedMessageDialog>(title, parameters, options).Result;

            if (!result.Canceled)
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

        private async Task MoveUp(BrokerDefinedMessageDto context)
        {
            SpinnerVisible = "display:block";
            StateHasChanged();

            if (context.SortOrder >= 1)
            {
                context.SortOrder--;
            }
            await BrokerDefinedMessageService.Update(context);
            await RefreshMessages();
            SpinnerVisible = "display:none";
            StateHasChanged();
        }

        private async Task MoveDown(BrokerDefinedMessageDto context)
        {
            SpinnerVisible = "display:block";
            StateHasChanged();
            if (context.SortOrder <= DefinedMessages.Count())
            {
                context.SortOrder++;
            }
            await BrokerDefinedMessageService.Update(context);
            await RefreshMessages();
            SpinnerVisible = "display:none";
            StateHasChanged();
        }

    }
}