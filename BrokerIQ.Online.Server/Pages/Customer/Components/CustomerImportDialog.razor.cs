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
        private const string  SAMPLE_CONTENT = @"Title;Forename;Surname;Nationality;Telephone;Email;AddressLine;City;PostCode;DateOfBirth;Employment;ResidentialStatus
Dr;Graham;Morales;1;070 9711 7201;m-graham@aol.couk;343-4795 Lectus Avenue;Devizes;RD8Q 6FA;1937-03-02;4;1
Dr;Cassady;Hinton;2;07624 157575;hinton-cassady@aol.net;762-9200 Donec St.;Kington;LJ8 5UJ;1939-07-23;2;3";

        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Inject]
        protected IJSRuntime js { get; set; }

        [Inject]
        public ISnackbar Snackbar { get; set; }

        [Inject]
        public ICustomerService CustomerService { get; set; }

        [Parameter]
        public int BrokerId { get; set; }

        private IBrowserFile csvFile;

        private ImportResponse ImportResult { get; set; }

        private string CurrentFileClass => csvFile is not null ? "visible" : "invisible";

        private string CurrentFileName => csvFile is not null ? csvFile.Name : string.Empty;

        private bool IsBusy { get; set; }

        private bool WasSimulatedRun { get; set; } = true;

        private bool DisableImportButton => csvFile is null || IsBusy || !WasSimulatedRun;

        private string SimulationResultClass => ImportResult is null ? "mt-2 p-1 invisible" : "mt-2 p-1 visible";

        private string ErrorRecordsClass => WasSimulatedRun ? "invisible" : "visible";

        private string ImportButtonText => ImportResult is null ? "Simulate Import" : "Import";

        private string SucessfulRecordsMessage => ImportResult is null ? string.Empty : $"Records to import : {ImportResult.RecordsImportedCount}.";

        private string ErrorRecordsMessage
        {
            get
            {
                if (ImportResult is null) return string.Empty;

                // Exception error, display message returned
                if (ImportResult.Errors.Count() == 1 && ImportResult.Errors.First().Line == 0) return ImportResult.Errors.First().ErrorMessage;

                return ImportResult is not null && ImportResult.RecordsInErrorCount > 0 ? $"Records with errors : {ImportResult.RecordsInErrorCount}." : string.Empty;
            }
        }

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private void SaveFile(IBrowserFile file)
        {
            csvFile = file;
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
                IsSimulatedRun = ImportResult is null
            };

            ImportResult = await CustomerService.Import(request);

            IsBusy = false;

            if (!request.IsSimulatedRun && ImportResult.RecordsImportedCount > 0)
            {
                Snackbar.Add("Import has finished", Severity.Success);

                WasSimulatedRun = request.IsSimulatedRun;

                return;
            }
        }

        private async Task SaveSampleFile()
        {
            byte[] fileContent = Encoding.UTF8.GetBytes(SAMPLE_CONTENT);
            await Extensions.Extensions.SaveAs(js, "Sample.csv", fileContent);
        }

        private async Task SaveRecordsInError()
        {
            byte[] fileContent = Encoding.UTF8.GetBytes(ImportResult.RecordsInError);
            await Extensions.Extensions.SaveAs(js, "ErrorRecords.csv", fileContent);
        }
    }
}