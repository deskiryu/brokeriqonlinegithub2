using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Dto.Request;
using BrokerIQ.Dto.Response;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;
using System.Text;

namespace BrokerIQ.Online.Server.Pages.Customer.Components
{
    public partial class CustomerImportDialog : ComponentBase
    {
        private const string SAMPLE_CONTENT = @"Title;Forename;Surname;Nationality;Telephone;Email;AddressLine;City;PostCode;DateOfBirth;Employment;ResidentialStatus
Dr;Graham;Morales;1;070 9711 7201;m-graham@aol.couk;343-4795 Lectus Avenue;Devizes;RD8Q 6FA;1937-03-02;4;1
Dr;Cassady;Hinton;2;07624 157575;hinton-cassady@aol.net;762-9200 Donec St.;Kington;LJ8 5UJ;1939-07-23;2;3";

        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        public ISnackbar Snackbar { get; set; }

        [Inject]
        public ICustomerService CustomerService { get; set; }

        [Parameter]
        public int BrokerId { get; set; }

        private IBrowserFile csvFile;

        private ImportResponse ImportResult { get; set; }

        public bool HasValidRecords => ImportResult is not null && ImportResult.RecordsImportedCount > 0;

        private string CurrentFileClass => csvFile is not null ? string.Empty : "d-none";

        private string CurrentFileName => csvFile is not null ? csvFile.Name : string.Empty;

        private bool IsBusy { get; set; }

        private bool WasSimulatedRun { get; set; } = true;

        private bool ShoulSendInvites { get; set; }

        private bool DisableImportButton
        {
            get
            {
                if (csvFile is null || IsBusy) return true;

                if (ImportResult is null) return false; // waiting on run

                if (!WasSimulatedRun) return ImportResult is not null; // disable until a new file is selected

                return ImportResult.RecordsImportedCount == 0 || ImportResult.HasFatalError;
            }
        }

        private string ImportResultsClass => ImportResult is null ? "mt-2 p-1 d-none" : "mt-2 p-1";

        private string ErrorRecordsDownloadClass
        {
            get
            {
                if (WasSimulatedRun) return "d-none";

                return ImportResult.RecordsInErrorCount > 0 ? string.Empty : "d-none";
            }
        }

        private string ImportButtonText => ImportResult is null ? "Simulate Import" : "Import";

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

        public string InviteBoxClass { get; set; } = "d-none";

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private void SaveFile(IBrowserFile file)
        {
            csvFile = file;

            ImportResult = null;

            StateHasChanged();
        }

        private async Task ImportFromFile()
        {
            IsBusy = true;

            string fileContent = string.Empty;

            var contentStream = csvFile.OpenReadStream();

            using (var streamReader = new StreamReader(contentStream))
            {
                fileContent = await streamReader.ReadToEndAsync();
            }

            var request = new ImportRequest()
            {
                BrokerId = BrokerId,
                FileName = csvFile.Name,
                CsvFile = fileContent,
                IsSimulatedRun = ImportResult is null,
                SendAppInviteToCustomers = ShoulSendInvites
            };

            ImportResult = await CustomerService.Import(request);

            IsBusy = false;

            if (!request.IsSimulatedRun && ImportResult.RecordsImportedCount > 0)
            {
                Snackbar.Add("Import has finished", Severity.Success);

                WasSimulatedRun = request.IsSimulatedRun;

                InviteBoxClass = "d-none";

                return;
            }

            InviteBoxClass = HasValidRecords ? string.Empty : "d-none";
        }

        private async Task SaveSampleFile()
        {
            byte[] fileContent = Encoding.UTF8.GetBytes(SAMPLE_CONTENT);
            await Extensions.Extensions.SaveAs(JSRuntime, "Sample.csv", fileContent);
        }

        private async Task SaveRecordsInError()
        {
            byte[] fileContent = Encoding.UTF8.GetBytes(ImportResult.RecordsInError);
            await Extensions.Extensions.SaveAs(JSRuntime, "ErrorRecords.csv", fileContent);
        }
    }
}