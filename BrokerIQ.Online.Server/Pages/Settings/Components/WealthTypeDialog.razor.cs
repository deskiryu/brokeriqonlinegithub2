using System.Threading.Tasks;
using BrokerIQ.Dto.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Settings.Components
{
    public partial class WealthTypeDialog
    {
        [Inject]
        public IDialogService DialogService { get; set; }

        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public WealthTypeDto WealthType { get; set; }

        MudForm form;

        async Task Submit()
        {
            await form.Validate();

            if (form.IsValid)
            {
                MudDialog.Close(DialogResult.Ok(WealthType));
            }
        }

        void Cancel() => MudDialog.Cancel();
    }
}