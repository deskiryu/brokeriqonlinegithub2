using System;
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
        public bool HasHeaderRecord { get; set; }

        [Parameter]
        public string Delimiter { get; set; }

        [Parameter]
        public IEnumerable<ImportRecordDefinitionDto> Definitions { get; set; }

        public bool IsEditing { get; set; }

        private int LastSortOrder => Definitions.Max(d => d.ColumnOrder);

        public DateTime? DefaultDateOfBirth { get; set; }

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

        private void Close()
        {
            Definitions.First(d => d.FieldName.Equals("DateOfBirth")).DefaultValue = DefaultDateOfBirth.HasValue ? DefaultDateOfBirth.Value.ToString("yyyy-MM-dd") : string.Empty;

            MudDialog.Cancel();
        }

        private void PreviewEditClick()
        {
            IsEditing = true;
        }

        private void CommitEditClick()
        {
            IsEditing = false;
        }
    }
}