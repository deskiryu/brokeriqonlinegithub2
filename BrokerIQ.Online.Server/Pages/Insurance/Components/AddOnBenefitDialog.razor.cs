using System.Threading.Tasks;
using BrokerIQ.Online.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Insurance.Components
{
    public partial class AddOnBenefitDialog
    {
        [CascadingParameter]
        private MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public AddOnBenefit Benefit { get; set; }

        [Parameter]
        public bool IsEdit { get; set; }

        private MudForm _form;

        protected override void OnParametersSet()
        {
            Benefit ??= new AddOnBenefit();
        }

        private string SaveLabel => IsEdit ? "Update" : "Add";

        private async Task Submit()
        {
            await _form.Validate();

            if (_form.IsValid)
            {
                MudDialog.Close(DialogResult.Ok(Benefit));
            }
        }

        private void Cancel() => MudDialog.Cancel();
    }
}
