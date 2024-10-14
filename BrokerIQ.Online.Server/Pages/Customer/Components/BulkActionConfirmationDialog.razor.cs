using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Customer.Components
{
    public partial class BulkActionConfirmationDialog : ComponentBase
    {
        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public IEnumerable<Online.Models.Customer> SelectedCustomers { get; set; }

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private void Confirm()
        {
            MudDialog.Close();
        }
    }
}