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
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Components;
using BrokerIQ.Online.Server.Extensions;
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

        [Inject]
        private IBrokerStaffService BrokerStaffService { get; set; }

        [Parameter]
        public User User { get; set; }

        [Parameter]
        public int BrokerId { get; set; }

        private const string HIDE_CLASS = "d-none";

        private string TitleFileName => string.IsNullOrWhiteSpace(CurrentFileName) ? string.Empty : $" from {CurrentFileName}";

        public IEnumerable<ImportRecordDefinitionDto> RecordDefinitions { get; set; } = new List<ImportRecordDefinitionDto>(){
           new ImportRecordDefinitionDto() { ColumnOrder = 0, FieldName = "Email", Active=true, Required=true, DefaultValue=string.Empty },
           new ImportRecordDefinitionDto() { ColumnOrder = 1, FieldName = "Title", Active=true, Required=false, DefaultValue=string.Empty },
           new ImportRecordDefinitionDto() { ColumnOrder = 2, FieldName = "Forename", Active=true, Required=true, DefaultValue=string.Empty  },
           new ImportRecordDefinitionDto() { ColumnOrder = 3, FieldName = "Surname", Active=true, Required=true, DefaultValue=string.Empty  },
           new ImportRecordDefinitionDto() { ColumnOrder = 4, FieldName = "Telephone", Active=true, Required=false, DefaultValue=string.Empty },
           new ImportRecordDefinitionDto() { ColumnOrder = 5, FieldName = "AddressLine", Active=true, Required=false, DefaultValue=string.Empty  },
           new ImportRecordDefinitionDto() { ColumnOrder = 6, FieldName = "City", Active=true, Required=false, DefaultValue=string.Empty },
           new ImportRecordDefinitionDto() { ColumnOrder = 7, FieldName = "PostCode", Active=true, Required=false, DefaultValue=string.Empty },
           new ImportRecordDefinitionDto() { ColumnOrder = 8, FieldName = "DateOfBirth", Active=true, Required=false, DefaultValue=string.Empty }
        };

        private IBrowserFile csvFile;

        private ImportResponse ImportPreview { get; set; }

        public bool HasHeaderRecord { get; set; } = true;

        public string Delimiter { get; set; } = ",";

        private string FileContent { get; set; } = string.Empty;

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

        private bool FatalErrorOccurred => ErrorRecordsMessage.Contains("Fatal");

        private string ImportResultsClass => ImportPreview is null ? $"mt-2 p-1 {HIDE_CLASS}" : "mt-2 p-1";

        private string ImportOptionsClass => FatalErrorOccurred ? $"ma-1 {HIDE_CLASS}" : "ma-1";

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

        public IEnumerable<Online.Models.BrokerStaff> AssignableStaff { get; set; }

        public int SelectedStaffId { get; set; }

        protected override async Task OnInitializedAsync()
        {
            AssignableStaff = Array.Empty<Online.Models.BrokerStaff>();

            AssignableStaff = (await BrokerStaffService.GetBrokerStaffbyBrokerId(BrokerId))
                .Where(s => s.StaffTypeId == Dto.Enum.StaffTypeEnum.Admin || s.StaffTypeId == Dto.Enum.StaffTypeEnum.Advisor)
                .ToArray();
        }

        private string GetSampleHeader()
        {
            return Import.GetSampleHeader(RecordDefinitions, Delimiter);
        }

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private async Task SaveFile(IBrowserFile file)
        {
            csvFile = file;

            FileContent = string.Empty;

            ImportPreview = null;

            ImportHasRun = false;

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
        }

        private void UpdateDefinitionsFrom(string[] headerFields)
        {
            if (headerFields == null || headerFields.Length == 0) return;

            foreach (var field in RecordDefinitions)
            {
                field.Active = false;
            }

            for (int i = 0; i < headerFields.Length; i++)
            {
                var field = RecordDefinitions.FirstOrDefault(f => f.FieldName.ToLower().Equals(headerFields[i].ToLower()));
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
            var assignedName = SelectedStaffId > 0 ? AssignableStaff.First(s => s.Id == SelectedStaffId).FullName : string.Empty;

            var parameters = new DialogParameters
            {
                { "ContentText", $"{ImportPreview.RecordsImportedCount} new customers will be created. OK to proceed ?" },
                { "AdditionalText", string.IsNullOrWhiteSpace(assignedName) ? "Customers will not be assigned to anyone." : $"Customer will be assigned to {assignedName}"  },
                { "ButtonText", "Confirm" },
                { "Color", Color.Success },
            };

            var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };

            IsBusy = true;

            var result = await DialogService.Show<ConfirmationDialog>("Confirm", parameters, dialogOptions).Result;

            if (!result.Canceled)
            {
                ImportRequest request = await BuildImportRequest();
                request.IsSimulatedRun = WasSimulatedRun = false;

                ImportPreview = await CustomerService.Import(request);

                if (ImportPreview.RecordsImportedCount > 0)
                {
                    Snackbar.Add($"Import successfully created {ImportPreview.RecordsImportedCount} clients", Severity.Success);

                    MudDialog.Close();
                }
            }

            IsBusy = false;
        }

        private async Task<ImportRequest> BuildImportRequest()
        {

            if (string.IsNullOrWhiteSpace(FileContent))
            {
                var contentStream = csvFile.OpenReadStream();

                using var streamReader = new StreamReader(contentStream);
                FileContent = await streamReader.ReadToEndAsync();
            }

            var request = new ImportRequest()
            {
                BrokerId = BrokerId,
                HasHeaderRecord = HasHeaderRecord,
                Delimiter = Delimiter,
                FileName = csvFile.Name,
                CsvFile = FileContent,
                SendAppInviteToCustomers = ShoulSendInvites,
                RecordDefinitions = RecordDefinitions.ToArray(),
                AssignToStaffId = SelectedStaffId != 0 ? SelectedStaffId : null
            };

            return request;
        }

        private async Task SaveErrorMessages()
        {
            var messages = string.Join("<br/>", ImportPreview.Errors.Select(e => $"Line {e.Line} : {e.ErrorMessage}"));

            await Extensions.Extensions.PreviewFileText(JSRuntime, messages);
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

        private async Task OpenDefaultsDialog()
        {
            var parameters = new DialogParameters()
            {
                { "Definitions" , RecordDefinitions.OrderBy(d => d.ColumnOrder) },
                { "HasHeaderRecord" , HasHeaderRecord },
                { "Delimiter" , Delimiter }
            };

            var options = new DialogOptions()
            {
                MaxWidth = MaxWidth.Medium,
                FullWidth = true
            };

            await DialogService.Show<ClientImportDefinitionDialog>("Columns", parameters, options).Result;
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

        private MarkupString GetSampleContent()
        {
            var sampleData = new CustomerImportDto[] {
                new CustomerImportDto()
                {
                    Title = "Dr",
                    Forename = "Graham",
                    Surname = "Morales",
                    Telephone = "070 9711 7201",
                    Email = "m-graham@aol.couk",
                    AddressLine = "343-4795 Lectus Avenue",
                    City = "Devizes",
                    PostCode = "RD8Q 6FA",
                    DateOfBirth = DateTime.Parse("1997-03-02")
                },
                new CustomerImportDto()
                {
                    Title = "Mrs",
                    Forename = "Cassady",
                    Surname = "HinAton",
                    Telephone = "07624 157575",
                    Email = "hinton-cassady@aol.net",
                    AddressLine = "762-9200 Donec St.",
                    City = "Kington",
                    PostCode = "LJ8 5UJ",
                    DateOfBirth = DateTime.Parse("1979-07-23")
                }
            };

            var result = string.Empty;
            foreach (var customer in sampleData)
            {
                Type t = customer.GetType();
                PropertyInfo[] props = t.GetProperties();

                var line = string.Empty;
                foreach (var item in RecordDefinitions.Where(d => d.Active).OrderBy(v => v.ColumnOrder))
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

        private async Task PreviewSampleFile()
        {
            var sampleContent = GetSampleHeader();
            sampleContent += "<hr />";
            sampleContent += GetSampleContent();
            await Extensions.Extensions.PreviewFileText(JSRuntime, sampleContent);
        }

        private async Task DownloadSampleFile()
        {
            var sampleContent = GetSampleHeader();
            sampleContent += "\n";
            sampleContent += GetSampleContent();
            sampleContent = sampleContent.Replace("<br/>", "\n");

            await Extensions.Extensions.SaveAs(JSRuntime, "import_sample.csv", Encoding.Unicode.GetBytes(sampleContent));
        }
    }
}