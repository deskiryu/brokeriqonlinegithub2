using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BrokerIQ.Dto.Dto.Import;
using BrokerIQ.Dto.Request;
using BrokerIQ.Dto.Response;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Customer.Components
{
    public partial class CustomerImportDialog : ComponentBase
    {
        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Inject]
        public IDialogService DialogService { get; set; }

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        public ISnackbar Snackbar { get; set; }

        [Inject]
        public ICustomerService CustomerService { get; set; }

        [Parameter]
        public int BrokerId { get; set; }

        private const string HIDE_CLASS = "d-none";

        private string TitleFileName => string.IsNullOrWhiteSpace(CurrentFileName) ? string.Empty : $" from {CurrentFileName}";

        public IEnumerable<ImportRecordDefinitionDto> RecordDefinitions { get; set; } = new List<ImportRecordDefinitionDto>(){
           new ImportRecordDefinitionDto() { ColumnOrder = 0, FieldName = "Title", Active=true, Required=false, DefaultValue=string.Empty },
           new ImportRecordDefinitionDto() { ColumnOrder = 1, FieldName = "Forename", Active=true, Required=false, DefaultValue=string.Empty  },
           new ImportRecordDefinitionDto() { ColumnOrder = 2, FieldName = "Surname", Active=true, Required=false, DefaultValue=string.Empty  },
           new ImportRecordDefinitionDto() { ColumnOrder = 3, FieldName = "Nationality", Active=true, Required=false, DefaultValue=string.Empty  },
           new ImportRecordDefinitionDto() { ColumnOrder = 4, FieldName = "Telephone", Active=true, Required=false, DefaultValue=string.Empty },
           new ImportRecordDefinitionDto() { ColumnOrder = 5, FieldName = "Email", Active=true, Required=true, DefaultValue=string.Empty },
           new ImportRecordDefinitionDto() { ColumnOrder = 6, FieldName = "AddressLine", Active=true, Required=false, DefaultValue=string.Empty  },
           new ImportRecordDefinitionDto() { ColumnOrder = 7, FieldName = "City", Active=true, Required=false, DefaultValue=string.Empty },
           new ImportRecordDefinitionDto() { ColumnOrder = 8, FieldName = "PostCode", Active=true, Required=false, DefaultValue=string.Empty },
           new ImportRecordDefinitionDto() { ColumnOrder = 9, FieldName = "DateOfBirth", Active=true, Required=false, DefaultValue=string.Empty },
           new ImportRecordDefinitionDto() { ColumnOrder = 10, FieldName = "Employment", Active=true, Required=false, DefaultValue=string.Empty  },
           new ImportRecordDefinitionDto() { ColumnOrder = 11, FieldName = "ResidentialStatus", Active=true, Required=false, DefaultValue=string.Empty}
        };

        private IBrowserFile csvFile;

        private ImportResponse ImportResult { get; set; }

        public bool HasHeaderRecord { get; set; } = true;

        public string Delimiter { get; set; } = ";";

        public bool HasValidRecords => ImportResult is not null && ImportResult.RecordsImportedCount > 0;

        private string CurrentFileName => csvFile is not null ? csvFile.Name : string.Empty;

        private bool IsBusy { get; set; }

        private bool ImportHasRun { get; set; }

        private bool WasSimulatedRun { get; set; } = true;

        private bool ShoulSendInvites { get; set; }

        private bool DisablePreviewButton
        {
            get
            {
                if (csvFile is null || IsBusy) return true;

                if (ImportResult is null) return false; // waiting on run

                return !WasSimulatedRun;
            }
        }

        private bool DisableImportButton
        {
            get
            {
                if (csvFile is null || IsBusy) return true;

                if (ImportResult is null) return false; // waiting on run

                return ImportResult.RecordsImportedCount == 0 || ImportResult.HasFatalError || ImportHasRun;
            }
        }

        private string ImportResultsClass => ImportResult is null ? $"mt-2 p-1 {HIDE_CLASS}" : "mt-2 p-1";

        private string ErrorMessagesDownloadClass
        {
            get
            {
                if (ImportResult is null) return HIDE_CLASS;

                return ImportResult.RecordsInErrorCount > 0 ? string.Empty : HIDE_CLASS;
            }
        }

        private string ErrorRecordsDownloadClass
        {
            get
            {
                if (ImportResult is null) return HIDE_CLASS;

                return ImportResult.RecordsInErrorCount > 0 ? string.Empty : HIDE_CLASS;
            }
        }

        private string InviteBoxClass { get; set; } = HIDE_CLASS;

        private string SucessfulRecordsMessage
        {
            get
            {
                if (ImportResult is null) return string.Empty;

                return WasSimulatedRun ? $"Records to import : {ImportResult.RecordsImportedCount}." : $"Records imported : {ImportResult.RecordsImportedCount}.";
            }
        }

        private string ErrorRecordsMessage
        {
            get
            {
                if (ImportResult is null) return string.Empty;

                // Exception error, display message returned
                if (ImportResult.Errors.Count() == 1 && ImportResult.Errors.First().Line == 0) return ImportResult.Errors.First().ErrorMessage;

                if (ImportResult.RecordsInErrorCount == 0) return string.Empty;

                return WasSimulatedRun ? $"Records with errors : {ImportResult.RecordsInErrorCount}." : $"Records NOT imported : {ImportResult.RecordsInErrorCount}.";
            }
        }

        private MarkupString SampleContent
        {
            get
            {
                var sampleContent = string.Empty;

                if (HasHeaderRecord)
                {
                    sampleContent += $"{GetSampleHeader()}\n";
                }

                sampleContent += GetSampleContent();

                return (MarkupString)sampleContent.Replace(Environment.NewLine, "<BR/>");
            }
        }

        private string GetSampleHeader()
        {
            var header = string.Empty;
            foreach (var field in RecordDefinitions.OrderBy(d => d.ColumnOrder))
            {
                if (!string.IsNullOrWhiteSpace(header)) header += Delimiter;
                header += field.FieldName;
            }

            return header;
        }

        private string GetSampleContent()
        {
            var sampleRows = new CustomerImportDto[]
            {
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
                        DateOfBirth = DateTime.Parse("1937-03-02"),
                        Employment = 4,
                        ResidentialStatus = 1
                    },
                    new CustomerImportDto()
                    {
                        Title = "Dr",
                        Forename = "Cassady",
                        Surname = "HinAton",
                        Nationality = 2,
                        Telephone = "07624 157575",
                        Email = "hinton-cassady@aol.net",
                        AddressLine = "762-9200 Donec St.",
                        City = "Kington",
                        PostCode = "LJ8 5UJ",
                        DateOfBirth = DateTime.Parse("1939-07-23"),
                        Employment = 2,
                        ResidentialStatus = 3
                    }
            };

            Type t = typeof(CustomerImportDto);
            PropertyInfo[] props = t.GetProperties();

            var contents = string.Empty;
            foreach (var row in sampleRows)
            {
                var rowContent = string.Empty;
                foreach (var field in RecordDefinitions.OrderBy(d => d.ColumnOrder))
                {
                    if (!string.IsNullOrWhiteSpace(rowContent)) rowContent += Delimiter;
                    if (props.Any(p => p.Name == field.FieldName))
                    {
                        rowContent += GetPropertyValue(row, props.First(p => p.Name == field.FieldName), 25);
                    }
                }
                if (!string.IsNullOrWhiteSpace(contents)) contents += "\n";
                contents += $"{rowContent}";
            }

            return contents;
        }

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private void SaveFile(IBrowserFile file)
        {
            csvFile = file;

            ImportResult = null;

            ImportHasRun = false;

            InviteBoxClass = HIDE_CLASS;

            MudDialog.StateHasChanged();
        }

        private async Task PreviewFile()
        {
            IsBusy = true;

            ImportRequest request = await BuildImportRequest();
            request.IsSimulatedRun = WasSimulatedRun = true;

            ImportResult = await CustomerService.Import(request);

            IsBusy = false;

            InviteBoxClass = HasValidRecords ? string.Empty : HIDE_CLASS;
        }

        private async Task ImportFromFile()
        {
            IsBusy = true;

            ImportRequest request = await BuildImportRequest();
            request.IsSimulatedRun = WasSimulatedRun = false;

            ImportResult = await CustomerService.Import(request);

            IsBusy = false;

            if (ImportResult.RecordsImportedCount > 0)
            {
                ImportHasRun = true;

                Snackbar.Add("Import has finished", Severity.Success);

                InviteBoxClass = HIDE_CLASS;

                return;
            }
        }

        private async Task<ImportRequest> BuildImportRequest()
        {

            string fileContent = string.Empty;

            var contentStream = csvFile.OpenReadStream();

            using (var streamReader = new StreamReader(contentStream))
            {
                fileContent = await streamReader.ReadToEndAsync();
            }

            var request = new ImportRequest()
            {
                BrokerId = BrokerId,
                HasHeaderRecord = HasHeaderRecord,
                Delimiter = Delimiter,
                FileName = csvFile.Name,
                CsvFile = fileContent,
                SendAppInviteToCustomers = ShoulSendInvites,
                RecordDefinitions = RecordDefinitions.ToArray()
            };

            return request;
        }

        private async Task SaveErrorMessages()
        {
            var messages = string.Join(Environment.NewLine, ImportResult.Errors.Select(e => $"Line {e.Line} : {e.ErrorMessage}"));

            byte[] fileContent = Encoding.UTF8.GetBytes(messages);
            await Extensions.Extensions.SaveAs(JSRuntime, GetErrorFileName(csvFile.Name, "Error Messages"), fileContent);
        }

        private async Task SaveRecordsInError()
        {
            byte[] fileContent = Encoding.UTF8.GetBytes(ImportResult.RecordsInError);
            await Extensions.Extensions.SaveAs(JSRuntime, GetErrorFileName(csvFile.Name, "Records In Error"), fileContent);
        }

        private string GetErrorFileName(string name, string toAppend)
        {
            var fileName = name.Substring(0, name.LastIndexOf("."));

            return $"{fileName} {toAppend}.csv";

        }

        private string GetPropertyValue(CustomerImportDto dto, PropertyInfo property, byte maxLength = 18)
        {
            var typeName = property.PropertyType.FullName;

            if (typeName.Contains("DateTime"))
            {
                var dateValue = property.GetValue(dto);

                return dateValue == null ? string.Empty : ((DateTime)dateValue).ToString("yyyy-MM-dd");
            }

            var value = property.GetValue(dto).ToString();
            return value.Length > maxLength ? value.Substring(0, 15) + "..." : value;
        }

        private async Task OpenDefaultsDialog()
        {
            var dialogParams = new DialogParameters()
            {
                { "Definitions" , RecordDefinitions.OrderBy(d => d.ColumnOrder) },
                { "HasHeaderRecord", HasHeaderRecord}
            };

            await DialogService.Show<ClientImportDefinitionDialog>("Columns", dialogParams).Result;
        }

        private string GetRowStyle(CustomerImportDto record, int index)
        {
            return ImportResult.Errors.Any(e => e.Line == record.RecordNumber) ? "background-color: #FD846A;" : string.Empty;
        }

        private string GetErrorMessagesFor(CustomerImportDto record)
        {
            var errorMessages = ImportResult.Errors.Where(e => e.Line == record.RecordNumber).Select(e => e.ErrorMessage).ToArray();
            return string.Join(" ", errorMessages);
        }
    }
}