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

        private ImportResponse ImportPreview { get; set; }

        public bool HasHeaderRecord { get; set; } = true;

        public string Delimiter { get; set; } = ",";

        public bool HasValidRecords => ImportPreview is not null && ImportPreview.RecordsImportedCount > 0;

        private string CurrentFileName => csvFile is not null ? csvFile.Name : string.Empty;

        private bool IsBusy { get; set; }

        private bool ImportHasRun { get; set; }

        private bool WasSimulatedRun { get; set; } = true;

        private bool ShoulSendInvites { get; set; }

        private bool DisableImportButton
        {
            get
            {
                if (csvFile is null || IsBusy) return true;

                if (ImportPreview is null) return false; // waiting on run

                return ImportPreview.RecordsImportedCount == 0 || ImportPreview.HasFatalError || ImportHasRun;
            }
        }

        private string ImportResultsClass => ImportPreview is null ? $"mt-2 p-1 {HIDE_CLASS}" : "mt-2 p-1";

        private string ErrorMessagesDownloadClass
        {
            get
            {
                if (ImportPreview is null) return HIDE_CLASS;

                return ImportPreview.RecordsInErrorCount > 0 ? string.Empty : HIDE_CLASS;
            }
        }

        private string ErrorRecordsDownloadClass
        {
            get
            {
                if (ImportPreview is null) return HIDE_CLASS;

                return ImportPreview.RecordsInErrorCount > 0 ? string.Empty : HIDE_CLASS;
            }
        }

        private string InviteBoxClass { get; set; } = HIDE_CLASS;

        private string SucessfulRecordsMessage
        {
            get
            {
                if (ImportPreview is null) return string.Empty;

                return WasSimulatedRun ? $"Records to import : {ImportPreview.RecordsImportedCount}." : $"Records imported : {ImportPreview.RecordsImportedCount}.";
            }
        }

        private string ErrorRecordsMessage
        {
            get
            {
                if (ImportPreview is null) return string.Empty;

                // Exception error, display message returned
                if (ImportPreview.Errors.Count() == 1 && ImportPreview.Errors.First().Line == 0) return ImportPreview.Errors.First().ErrorMessage;

                if (ImportPreview.RecordsInErrorCount == 0) return string.Empty;

                return WasSimulatedRun ? $"Records with errors : {ImportPreview.RecordsInErrorCount}." : $"Records NOT imported : {ImportPreview.RecordsInErrorCount}.";
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

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private async Task SaveFile(IBrowserFile file)
        {
            csvFile = file;

            ImportPreview = null;

            ImportHasRun = false;

            InviteBoxClass = HIDE_CLASS;

            await PreviewFile();

            MudDialog.StateHasChanged();
        }

        private async Task PreviewFile()
        {
            IsBusy = true;

            ImportRequest request = await BuildImportRequest();
            request.IsSimulatedRun = WasSimulatedRun = true;

            ImportPreview = await CustomerService.Import(request);

            if (HasHeaderRecord) UpdateDefinitionsFrom(ImportPreview.HeaderFields);

            IsBusy = false;

            InviteBoxClass = HasValidRecords ? string.Empty : HIDE_CLASS;
        }

        private void UpdateDefinitionsFrom(string[] headerFields)
        {
            foreach (var field in RecordDefinitions)
            {
                field.Active = false;
            }

            for (int i = 0; i < headerFields.Length; i++)
            {
                var field = RecordDefinitions.FirstOrDefault(f => f.FieldName == headerFields[i]);
                if (field == null) continue;

                field.Active = true;
                field.ColumnOrder = i;
            }

            var next = RecordDefinitions.Max(d => d.ColumnOrder) + 1;
            foreach (var field in RecordDefinitions.Where(d => !d.Active))
            {
                field.ColumnOrder = next++;
            }
        }

        private async Task ImportFromFile()
        {
            IsBusy = true;

            ImportRequest request = await BuildImportRequest();
            request.IsSimulatedRun = WasSimulatedRun = false;

            ImportPreview = await CustomerService.Import(request);

            IsBusy = false;

            if (ImportPreview.RecordsImportedCount > 0)
            {
                Snackbar.Add("Import has finished", Severity.Success);

                InviteBoxClass = HIDE_CLASS;

                MudDialog.Close();
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
            var messages = string.Join(Environment.NewLine, ImportPreview.Errors.Select(e => $"Line {e.Line} : {e.ErrorMessage}"));

            byte[] fileContent = Encoding.UTF8.GetBytes(messages);
            await Extensions.Extensions.SaveAs(JSRuntime, GetErrorFileName(csvFile.Name, "Error Messages"), fileContent);
        }

        private async Task SaveRecordsInError()
        {
            byte[] fileContent = Encoding.UTF8.GetBytes(ImportPreview.RecordsInError);
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

            var propertyValue = property.GetValue(dto);
            var value = propertyValue is not null ? property.GetValue(dto).ToString() : string.Empty;
            return value.Length > maxLength ? value.Substring(0, 15) + "..." : value;
        }

        private async Task OpenDefaultsDialog()
        {
            var dialogParams = new DialogParameters()
            {
                { "Definitions" , RecordDefinitions.OrderBy(d => d.ColumnOrder) }
            };

            await DialogService.Show<ClientImportDefinitionDialog>("Columns", dialogParams).Result;
        }

        private string GetRowStyle(CustomerImportDto record, int index)
        {
            return ImportPreview.Errors.Any(e => e.Line == record.RecordNumber) ? "background-color: #FD846A;" : string.Empty;
        }

        private string GetErrorMessagesFor(CustomerImportDto record)
        {
            var errorMessages = ImportPreview.Errors.Where(e => e.Line == record.RecordNumber).Select(e => e.ErrorMessage).ToArray();
            return string.Join(" ", errorMessages);
        }

        private async Task DownloadSampleFile()
        {
            var sampleContent = GetSampleHeader();
            sampleContent += "\n\n\n\n\n\n\nInstructions:\n";
            sampleContent += "1. Fill in the data using the headers as guides as to what goes where\n";
            sampleContent += "2. Remove these instructions from the file if still here when all the data is ready\n";
            sampleContent += "3. Upload the file\n";

            byte[] fileContent = Encoding.UTF8.GetBytes(sampleContent);
            await Extensions.Extensions.SaveAs(JSRuntime, "customer_template.csv", fileContent);
        }

    }
}