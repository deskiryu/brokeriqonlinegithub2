namespace BrokerIQ.Online.Pages
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;
    using Models;
    using MudBlazor;
    
    using Services.Interface;

    public class BrokerEditBase : ComponentBase
    {
        private int id;
        private int customerId;
        private string strBrokerId;

        [Inject]
        public IBrokerService BrokerService { get; set; }

        [Inject]
        public IAccountService AccountService { get; set; }

        [Inject]
        public IAzureService AzureService { get; set; }

        [Inject]
        public IInsuranceDocumentService SupportingDocumentService { get; set; }

        [Inject] 
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IDialogService DialogService { get; set; }

        public Broker Broker { get; set; }

        protected string Message = string.Empty;
        protected string StatusClass = string.Empty;
        protected bool Saved;

        [Parameter]
        public string BrokerId { get; set; }

        public BrokerEditBase()
        {
            Broker = new Broker();
        }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var user = await AccountService.GetUser();
                if (user.IsAdmin)
                {
                    id = Int32.Parse(BrokerId);
                    if (id > 0)
                    {
                        Broker = (await BrokerService.GetBroker(id));
                    }
                }
                else
                {
                    throw new Exception("Bad user");
                }
            }
            catch
            {
                NavigationManager.NavigateTo($"account/logout");
            }
        }

        protected void HandleInvalidSubmit()
        {
            StatusClass = "alert-danger";
            Message = "There are some validation errors. Please try again.";
        }

        protected async Task HandleValidSubmit()
        {
            await BrokerService.UpdateBroker(Broker);
            StatusClass = "alert-success";
            Message = "Broker updated successfully.";
            Saved = true;
        }

        protected void NavigateToOverview()
        {
            NavigationManager.NavigateTo($"/brokerlist");
        }

        public async Task LoadFiles(InputFileChangeEventArgs e)
        {

            try
            {
                var file = e.GetMultipleFiles(1).FirstOrDefault();
                var ext = Path.GetExtension(file.Name);
                if (ext != ".jpeg" && ext != ".jpg")
                {
                    throw new Exception("Jpeg files only");
                }
                if (file != null)
                {
                    var memoryStream = new MemoryStream();
                    await file.OpenReadStream(int.MaxValue).CopyToAsync(memoryStream);
                    Broker.LogoImage = memoryStream.ToArray();
                    var fileName = Broker.Id.ToString() + ".jpeg";
                    memoryStream.Position = 0;
                    await AzureService.TransferLogoStreamToAzureBlob(fileName, memoryStream, Broker.Id);
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                StateHasChanged();
            }
        }

        protected async Task DeleteBroker()
        {
            var dialogParams = new DialogParameters();
            dialogParams.Add("Message", $"Are you absolutely sure you want to delete this broker {Broker.Name}? This is a PERMANENT DELETE and cannot be undone. All links to customers will be lost. Ensure this broker has no insurances or mortgages with customers.");
            var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Cancelled)
            {
                var deleted = await BrokerService.DeleteBroker(Broker.Id);
                if (deleted)
                {
                    var responseParams = new DialogParameters();
                    responseParams.Add("Message", "Deleted successfully");
                    await DialogService.Show<AlertDialog>("Information", responseParams).Result;
                }
                else
                {
                    var responseParams = new DialogParameters();
                    responseParams.Add("Message", "The broker did not delete.");
                    await DialogService.Show<AlertDialog>("Information", responseParams).Result;
                }
                NavigationManager.NavigateTo($"/brokerlist");
            }
        }
    }
}
