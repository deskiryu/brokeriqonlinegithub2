using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;

using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Server.Shared;
using BrokerIQ.Online.Server.AppSettings;

using MudBlazor;
using BrokerIQ.Online.Server.Extensions;

namespace BrokerIQ.Online.Server.Pages.Settings.Components
{
    public partial class DocumentVaultTypeDialog
    {
        [Inject]
        public IDialogService DialogService { get; set; }

        [Microsoft.AspNetCore.Components.CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Microsoft.AspNetCore.Components.Parameter]
        public DocumentVaultTypeDto VaultType { get; set; }

        MudForm form;

        async Task Submit()
        {
            await form.Validate();

            if (form.IsValid)
            {
                MudDialog.Close(DialogResult.Ok(VaultType));
            }
        }

        void Cancel() => MudDialog.Cancel();
    }
}