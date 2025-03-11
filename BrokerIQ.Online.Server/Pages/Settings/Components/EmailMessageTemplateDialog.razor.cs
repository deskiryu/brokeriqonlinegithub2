using System.Threading.Tasks;
using BrokerIQ.Dto.Dto;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Settings.Components
{
    public partial class EmailMessageTemplateDialog
    {
        [Inject]
        public IDialogService DialogService { get; set; }

        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public EmailMessageTemplateDto EmailMessageTemplate { get; set; }

        MudForm form;

        async Task Submit()
        {
            await form.Validate();

            if (form.IsValid)
            {
                MudDialog.Close(DialogResult.Ok(EmailMessageTemplate));
            }
        }

        void Cancel() => MudDialog.Cancel();
    }
}