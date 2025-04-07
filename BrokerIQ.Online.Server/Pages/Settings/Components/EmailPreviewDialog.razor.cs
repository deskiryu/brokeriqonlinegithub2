using System.Threading.Tasks;
using BrokerIQ.Dto.Dto;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Settings.Components
{
    public partial class EmailPreviewDialog
	{
        [Inject]
        public IDialogService DialogService { get; set; }

        [Inject]
        public IEmailMessageTemplateService EmailMessageTemplateService { get; set; }

        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public User User { get; set; }

        [Parameter]
        public Online.Models.Broker Broker { get; set; }

        [Parameter]
        public EmailMessageTemplateDto EmailMessageTemplate { get; set; }

        string EmailPreview = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            EmailPreview = await EmailMessageTemplateService.GetContentPreviewFor(EmailMessageTemplate);

            await base.OnInitializedAsync();

            return;
        }

        void Close() => MudDialog.Cancel();
    }
}

