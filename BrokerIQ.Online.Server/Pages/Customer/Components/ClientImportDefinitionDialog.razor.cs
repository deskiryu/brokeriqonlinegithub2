using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BrokerIQ.Dto.Dto.Import;
using BrokerIQ.Online.Server.Extensions;
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

        private string GetSampleHeader()
        {
            return Import.GetSampleHeader(Definitions, Delimiter);
        }

        private MarkupString GetSampleContent()
        {
            var sampleData = new CustomerImportDto[] {
                new CustomerImportDto()
                {
                    Title = "Dr",
                    Forename = "Graham",
                    Surname = "Morales",
                    Nationality = 1,
                    Telephone = "070 9711 7201",
                    Email = "m-graham@aol.couk",
                    AddressLine = "343-4795 Lectus Avenue",
                    City = "Devizes",
                    PostCode = "RD8Q 6FA",
                    DateOfBirth = DateTime.Parse("1997-03-02"),
                    Employment = 4,
                    ResidentialStatus = 1
                },
                new CustomerImportDto()
                {
                    Title = "Mrs",
                    Forename = "Cassady",
                    Surname = "HinAton",
                    Nationality = 2,
                    Telephone = "07624 157575",
                    Email = "hinton-cassady@aol.net",
                    AddressLine = "762-9200 Donec St.",
                    City = "Kington",
                    PostCode = "LJ8 5UJ",
                    DateOfBirth = DateTime.Parse("1979-07-23"),
                    Employment = 2,
                    ResidentialStatus = 3
                }
            };

            var result = string.Empty;
            foreach (var customer in sampleData)
            {
                Type t = customer.GetType();
                PropertyInfo[] props = t.GetProperties();

                var line = string.Empty;
                foreach (var item in Definitions.Where(d => d.Active).OrderBy(v => v.ColumnOrder))
                {
                    if (!string.IsNullOrWhiteSpace(line)) line += ",";

                    if (props.Any(p => p.Name == item.FieldName))
                    {
                        line += customer.GetPropertyValue(props.First(p => p.Name == item.FieldName), 50);
                    }
                }

                if (!string.IsNullOrWhiteSpace(result)) result += "<br/>";
                result += line;
            }

            return new MarkupString(result);
        }
    }
}