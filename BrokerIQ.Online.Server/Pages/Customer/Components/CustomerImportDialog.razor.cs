using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Dto;
using BrokerIQ.Dto.Request;
using BrokerIQ.Dto.Response;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Customer.Components
{
    public partial class CustomerImportDialog : ComponentBase
    {
        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Inject]
        public ISnackbar Snackbar { get; set; }

        [Inject]
        public ICustomerService CustomerService { get; set; }

        [Parameter]
        public int BrokerId { get; set; }

        private IBrowserFile csvFile;

        private IEnumerable<ImportError> ImportErrors = Array.Empty<ImportError>(); 

        private string CurrentFileClass => csvFile is not null ? "visible" : "invisible";

        private string CurrentFileName => csvFile is not null ? csvFile.Name : string.Empty;

        private bool IsBusy { get; set; }

        private bool DisableImportButton => csvFile is null || IsBusy || ImportErrors.Any();

        private string ErrorListClass => ImportErrors.Any() ? "mt-2 p-1 visible" : "mt-2 p-1 invisible";

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

            ImportResponse importResult = await CustomerService.Import(new ImportRequest() { BrokerId = BrokerId, FileName = csvFile.Name, CsvFile = fileContent });

            IsBusy = false;

            if (importResult is not null && importResult.WasSucessfull)
            {
                Snackbar.Add("Import was sucessfull", Severity.Success);

                MudDialog.Close();

                return;
            }

            ImportErrors = importResult.Errors;
        }
    }
}