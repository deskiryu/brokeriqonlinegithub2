using System.Collections.Generic;
using System.Linq;
using BrokerIQ.Dto.Dto.Import;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Customer.Components
{
    public partial class ClientImportDefinitionDialog : ComponentBase
    {
        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public IEnumerable<ImportRecordDefinitionDto> Definitions { get; set; }

        private int LastSortOrder => Definitions.Max(d => d.ColumnOrder);

        private void MoveUp(ImportRecordDefinitionDto definition)
        {
            if (definition.ColumnOrder >= 1)
            {
                var previousDefinition = Definitions.First(d => d.ColumnOrder == definition.ColumnOrder - 1);

                previousDefinition.ColumnOrder++;
                definition.ColumnOrder--;
            }

            Definitions = Definitions.OrderBy(d => d.ColumnOrder);

            StateHasChanged();
        }

        private void MoveDown(ImportRecordDefinitionDto definition)
        {
            if (definition.ColumnOrder <= LastSortOrder)
            {
                var nextDefinition = Definitions.First(d => d.ColumnOrder == definition.ColumnOrder + 1);

                nextDefinition.ColumnOrder--;
                definition.ColumnOrder++;
            }

            Definitions = Definitions.OrderBy(d => d.ColumnOrder);

            StateHasChanged();
        }

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private void SetDefaults()
        {
            MudDialog.Close(DialogResult.Ok(Definitions));
        }
    }
}