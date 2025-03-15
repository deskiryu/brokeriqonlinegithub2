using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Dto;
using BrokerIQ.Dto.Enum;
using BrokerIQ.Dto.UpdateDto;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Extensions;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Settings.Components
{
    public partial class EmailMessageTemplateTable : ComponentBase
    {
        [Inject]
        private IDialogService DialogService { get; set; }

        [Inject]
        public IEmailMessageTemplateService EmailMessageTemplateService { get; set; }

        [Parameter]
        public User User { get; set; }

        [Parameter]
        public Online.Models.Broker Broker { get; set; }

        private ICollection<EmailMessageTemplateDto> EmailMessageTemplates { get; set; }

        protected override async Task OnInitializedAsync()
        {
            EmailMessageTemplates = new List<EmailMessageTemplateDto>(await EmailMessageTemplateService.GetAllForBroker(Broker.Id));
        }

        private async Task ReloadEmailMessageTemplates()
        {
            EmailMessageTemplates = new List<EmailMessageTemplateDto>(await EmailMessageTemplateService.GetAllForBroker(Broker.Id));

            StateHasChanged();
        }

        private async Task EditEmailMessageTemplate(EmailMessageTemplateDto EmailMessageTemplate)
        {
            var title = $"Edit {((EmailTemplate)EmailMessageTemplate.EmailTemplateId).GetDisplayName()} Template";
            var parameters = new DialogParameters
            {
                { "EmailMessageTemplate", EmailMessageTemplate }
            };

            var options = new DialogOptions() { MaxWidth = MaxWidth.Small, FullWidth = true };

            var result = await DialogService.Show<EmailMessageTemplateDialog>(title, parameters, options).Result;

            if (!result.Canceled)
            {
                EmailMessageTemplateDto updated = result.Data as EmailMessageTemplateDto;

                var wasSuccessfull = await EmailMessageTemplateService.Update(new UpdateEmailMessageTemplateDto()
                {
                    Id = updated.Id,
                    Message = updated.Message
                });
            }

            await ReloadEmailMessageTemplates();
        }
    }
}