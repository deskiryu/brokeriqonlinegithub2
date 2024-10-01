using System;
using System.Collections.Generic;
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

        public IEnumerable<ImportRecordDefinitionDto> RecordDefinitions { get; set; } = new List<ImportRecordDefinitionDto>(){
           new ImportRecordDefinitionDto() { ColumnOrder = 0, FieldName = "Title", Active=true, Required=true, DefaultValue=string.Empty },
           new ImportRecordDefinitionDto() { ColumnOrder = 1, FieldName = "Forename", Active=true, Required=true, DefaultValue="Unknown" },
           new ImportRecordDefinitionDto() { ColumnOrder = 2, FieldName = "Surname", Active=true, Required=true, DefaultValue="Unknown" },
           new ImportRecordDefinitionDto() { ColumnOrder = 3, FieldName = "Nationality", Active=true, Required=true, DefaultValue="1" },
           new ImportRecordDefinitionDto() { ColumnOrder = 4, FieldName = "Telephone", Active=true, Required=true, DefaultValue=string.Empty },
           new ImportRecordDefinitionDto() { ColumnOrder = 5, FieldName = "Email", Active=true, Required=true, DefaultValue=string.Empty },
           new ImportRecordDefinitionDto() { ColumnOrder = 6, FieldName = "AddressLine", Active=true, Required=true, DefaultValue="Unknown Address" },
           new ImportRecordDefinitionDto() { ColumnOrder = 7, FieldName = "City", Active=true, Required=true, DefaultValue=string.Empty },
           new ImportRecordDefinitionDto() { ColumnOrder = 8, FieldName = "PostCode", Active=true, Required=true, DefaultValue=string.Empty },
           new ImportRecordDefinitionDto() { ColumnOrder = 9, FieldName = "DateOfBirth", Active=true, Required=true, DefaultValue=DateTime.UtcNow.AddYears(-30).ToString("yyyy-MM-dd") },
           new ImportRecordDefinitionDto() { ColumnOrder = 10, FieldName = "Employment", Active=true, Required=true, DefaultValue="9" },
           new ImportRecordDefinitionDto() { ColumnOrder = 11, FieldName = "ResidentialStatus", Active=true, Required=true, DefaultValue="2" }
        };

        private IBrowserFile csvFile;

        private ImportResponse ImportResult { get; set; }

        public bool HasHeaderRecord { get; set; } = true;

        public string Delimiter { get; set; } = ";";

        public bool HasValidRecords => ImportResult is not null && ImportResult.RecordsImportedCount > 0;

        private string CurrentFileClass => csvFile is not null ? string.Empty : HIDE_CLASS;

        private string CurrentFileName => csvFile is not null ? csvFile.Name : string.Empty;

        private bool IsBusy { get; set; }

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

                return ImportResult.RecordsImportedCount == 0 || ImportResult.HasFatalError;
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

        public string InviteBoxClass { get; set; } = HIDE_CLASS;

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private void SaveFile(IBrowserFile file)
        {
            csvFile = file;

            ImportResult = null;

            InviteBoxClass = HIDE_CLASS;

            StateHasChanged();
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

        private async Task SaveSampleFile()
        {
            var sampleContent = string.Empty;

            if (HasHeaderRecord)
            {
                var header = $"Title{Delimiter}Forename{Delimiter}Surname{Delimiter}Nationality{Delimiter}Telephone{Delimiter}Email{Delimiter}AddressLine{Delimiter}City{Delimiter}PostCode{Delimiter}DateOfBirth{Delimiter}Employment{Delimiter}ResidentialStatus\n";
                sampleContent = header;
            }

            sampleContent += @"Dr{Delimiter}Graham{Delimiter}Morales{Delimiter}1{Delimiter}070 9711 7201{Delimiter}m-graham@aol.couk{Delimiter}343-4795 Lectus Avenue{Delimiter}Devizes{Delimiter}RD8Q 6FA{Delimiter}1937-03-02{Delimiter}4{Delimiter}1
Dr{Delimiter}Cassady{Delimiter}HinAton{Delimiter}2{Delimiter}07624 157575{Delimiter}hinton-cassady@aol.net{Delimiter}762-9200 Donec St.{Delimiter}Kington{Delimiter}LJ8 5UJ{Delimiter}1939-07-23{Delimiter}2{Delimiter}3";
            sampleContent = sampleContent.Replace("{Delimiter}", Delimiter);

            byte[] fileContent = Encoding.UTF8.GetBytes(sampleContent);
            await Extensions.Extensions.SaveAs(JSRuntime, "Sample.csv", fileContent);
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

        private string GetPropertyValue(CustomerImportDto dto, PropertyInfo property)
        {
            var typeName = property.PropertyType.FullName;

            if (typeName.Contains("DateTime"))
            {
                var dateValue = property.GetValue(dto);

                return dateValue == null ? string.Empty : ((DateTime)dateValue).ToString("yyyy MMM dd");
            }

            var value = property.GetValue(dto).ToString();
            return value.Length > 18 ? value.Substring(0, 15) + "..." : value;
        }

        private async Task OpenDefaultsDialog()
        {
            var dialogParams = new DialogParameters()
            {
                { "Definitions" , RecordDefinitions }
            };

            var result = await DialogService.Show<ClientImportDefinitionDialog>("Set definition", dialogParams).Result;

            if (!result.Canceled)
            {
                RecordDefinitions = result.Data as IEnumerable<ImportRecordDefinitionDto>;
            }
        }
    }
}