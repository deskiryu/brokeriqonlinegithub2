using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Pages
{
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using AutoMapper;
    
    using Microsoft.AspNetCore.Components;
    using Models;
    using MudBlazor;
    using BrokerIQ.Dto.Enum;
    using Services.Interface;
    
    using BrokerIQ.Online.Server.Extensions;
    using Microsoft.JSInterop;
    using BrokerIQ.Online.Server.AppSettings;
    using Microsoft.Extensions.Options;
    using Microsoft.AspNetCore.Components.Forms;
    using BrokerIQ.Online.Server.Shared;

    public class MortgageEditBase : ComponentBase
    {
        private int id;
        private string mortgageId;

        private int customerId;
        private string strCustomerId;

        [Inject]
        public IMortgageService MortgageService { get; set; }

        [Inject]
        public IMortgageDocumentService SupportingDocumentService { get; set; }

        [Inject] 
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IAccountService AccountService { get; set; }

        [Inject]
        public IBrokerService BrokerService { get; set; }

        [Inject]
        public IDialogService DialogService { get; set; }

        [Inject]
        public INotificationService NotificationService { get; set; }

        [Inject]
        public ICustomerService CustomerService { get; set; }

        [Inject]
        public IOptions<FileUploadSettings> FileUploadSettingsOption { get; set; }
        private FileUploadSettings fileUploadSettings { get; set; }

        protected List<(IBrowserFile, byte[])> LoadedFiles = new();

        public Mortgage Mortgage { get; set; }

        protected string Message = string.Empty;
        protected string StatusClass = string.Empty;
        protected bool Saved;
        public bool IsAdmin { get; set; }
        public IEnumerable<Broker> Brokers { get; set; }
        public Broker Broker{ get; set; }

        public string DragEnterStyle { get; set; }

        [Required]
        public int BrokerListId = 1;

        [Required]
        public int MortgageType = 1;

        [Required]
        public int MortgageRateType = 1;

        [Parameter]
        public string MortgageId {
            get => this.mortgageId;
            set
            {
                this.mortgageId = value;
                this.id = int.Parse(value);
            }
        }

        [Parameter]
        public string CustomerId
        {
            get => this.strCustomerId;
            set
            {
                this.strCustomerId = value;
                this.customerId = int.Parse(value);
            }
        }

        public string SpinnerVisible { get; set; }
        public string LoadFileStatus { get; set; }
        public bool SendNotification { get; set; }


        public MortgageEditBase()
        {
            Mortgage = new Mortgage();
            Mortgage.SupportingDocuments = new List<MortgageDocument>();
        }

        protected override async Task OnInitializedAsync()
        {
            SpinnerVisible = "display:none";
            fileUploadSettings = this.FileUploadSettingsOption.Value;
            SendNotification = true;
        }

        protected override async Task OnParametersSetAsync()
        {
            var user = await AccountService.GetUser();
            IsAdmin = user.IsAdmin;
            if (user.IsBroker || user.IsBrokerStaff)
            {
                var broker = user.MasterBrokerId;

                BrokerListId = broker;
                try
                {
                    Broker = await BrokerService.GetBroker(broker);
                }
                catch
                {
                    StatusClass = "alert-danger";
                    Message = "Something went wrong getting broker details";
                    Saved = true;
                }
            }
            else
            {
                try
                {
                    Brokers = await BrokerService.GetBrokers();
                }
                catch
                {
                    StatusClass = "alert-danger";
                    Message = "Something went wrong getting broker details";
                    Saved = true;
                }        
            }

            try
            {
                if (this.id > 0)
                {
                    Mortgage = (await MortgageService.GetMortgage(this.id));
                    BrokerListId = Mortgage.BrokerId;
                    MortgageType = (int)Mortgage.MortgageType;
                    MortgageRateType = (int)Mortgage.MortgageRateType;
                }
            }
            catch
            {
                StatusClass = "alert-danger";
                Message = "Something went wrong getting mortgage details";
                Saved = true;
            }
            
        }

        protected void HandleInvalidSubmit()
        {
            StatusClass = "alert-danger";
            Message = "There are some validation errors. Please try again.";
        }

        protected async Task HandleValidSubmit()
        {
            Mortgage.MortgageType = (MortgageEnum)MortgageType;
            Mortgage.MortgageRateType = (MortgageRateEnum)MortgageRateType;
            Mortgage.BrokerId = this.BrokerListId;

            if (IsAdmin)
            {

            }
            if (Mortgage.Id == 0)
            {
                Mortgage.CustomerId = this.customerId;

                if (Mortgage.EndDate == null || Mortgage.EndDate == DateTime.MinValue)
                {
                    Mortgage.EndDate = DateTime.Now;
                }

                if (Mortgage.PromotionalEndDate == null || Mortgage.PromotionalEndDate == DateTime.MinValue)
                {
                    Mortgage.PromotionalEndDate = DateTime.Now;
                }

                if (Mortgage.PotentialEndDate == null || Mortgage.PotentialEndDate == DateTime.MinValue)
                {
                    Mortgage.PotentialEndDate = DateTime.Now;
                }
                //Midnight
                Mortgage.EndDate = new DateTime(Mortgage.EndDate.Value.Year, Mortgage.EndDate.Value.Month, Mortgage.EndDate.Value.Day, 0, 0, 0);
                Mortgage.PromotionalEndDate = new DateTime(Mortgage.PromotionalEndDate.Value.Year, Mortgage.PromotionalEndDate.Value.Month, Mortgage.PromotionalEndDate.Value.Day, 0, 0, 0);
                Mortgage.PotentialEndDate = new DateTime(Mortgage.PotentialEndDate.Value.Year, Mortgage.PotentialEndDate.Value.Month, Mortgage.PotentialEndDate.Value.Day, 0, 0, 0);

                var customer = new Customer();
                try
                {
                    customer = await CustomerService.GetCustomer(customerId);
                }
                catch
                {
                    StatusClass = "alert-danger";
                    Message = "Something went wrong getting the client. Please try again.";
                    Saved = true;
                    return;
                }


                var dialogParams = new DialogParameters();
                if (customer.EmailConfirmed && SendNotification == true)
                {
                    dialogParams.Add("Message", $"Mortgage will be added and a notification will be sent to {customer.Name} about this new mortgage.");
                }
                else if (!customer.EmailConfirmed)
                {
                    dialogParams.Add("Message", $"Mortgage will be added however a notification will be NOT be sent to {customer.Name} about this new mortgage as their email address is not confirmed");
                }
                else
                {
                    dialogParams.Add("Message", $"Mortgage will be added however a notification will be NOT be sent to {customer.Name}.");
                }

                var fileNamesAndMemoryStreams = new List<(string, MemoryStream)>();
                var fileNamesAndBytes = new List<(string, byte[])>();
                foreach (var file in LoadedFiles)
                {
                    if (file.Item1.Size > this.fileUploadSettings.MaxFileSize)
                    {
                        dialogParams.Add("Oversize", "true");
                        continue;
                    }
                    var loopMemoryStream = new MemoryStream(file.Item2);
                    fileNamesAndMemoryStreams.Add((file.Item1.Name, loopMemoryStream));
                }

                bool agreed;
                if (LoadedFiles.Any())
                {

                    dialogParams.Add("Filenames", fileNamesAndMemoryStreams.Select(x => x.Item1).ToList());
                    dialogParams.Add("MemoryStreams", fileNamesAndMemoryStreams.Select(x => x.Item2).ToList());
                    var result = await DialogService.Show<FilesConfirmDialog>("Mortgage Add", dialogParams).Result;
                    agreed = !result.Cancelled;
                    if (agreed)
                    {
                        foreach (var file in fileNamesAndMemoryStreams)
                        {
                            fileNamesAndBytes.Add((file.Item1, file.Item2.ToArray()));
                        }
                    }
                }
                else
                {
                    var result = await DialogService.Show<ConfirmCancelDialog>("Mortgage Add", dialogParams).Result;
                    agreed = !result.Cancelled;
                }

                if (agreed)
                {
                    try
                    {
                        await MortgageService.AddMortgage(Mortgage, fileNamesAndBytes);
                    }
                    catch
                    {
                        StatusClass = "alert-danger";
                        Message = "Something went wrong adding the new Mortgage. Please try again.";
                        Saved = true;
                        return;
                    }

                    try
                    {
                        if (SendNotification)
                        {
                            await SendMessageNotification(customer);
                        }
                    }
                    catch
                    {

                    }

                    StatusClass = "alert-success";
                    Message = "New Mortgage added successfully.";
                    Saved = true;
                }

            }
            else
            {
                //Midnight
                if (Mortgage.EndDate.HasValue)
                {
                    Mortgage.EndDate = new DateTime(Mortgage.EndDate.Value.Year, Mortgage.EndDate.Value.Month, Mortgage.EndDate.Value.Day, 0, 0, 0);
                }

                if (Mortgage.PromotionalEndDate.HasValue)
                {
                    Mortgage.PromotionalEndDate = new DateTime(Mortgage.PromotionalEndDate.Value.Year, Mortgage.PromotionalEndDate.Value.Month, Mortgage.PromotionalEndDate.Value.Day, 0, 0, 0);
                }

                if (Mortgage.PotentialEndDate.HasValue)
                {
                    Mortgage.PotentialEndDate = new DateTime(Mortgage.PotentialEndDate.Value.Year, Mortgage.PotentialEndDate.Value.Month, Mortgage.PotentialEndDate.Value.Day, 0, 0, 0);
                }




                try
                {
                    await MortgageService.UpdateMortgage(Mortgage);
                }
                catch
                {
                    StatusClass = "alert-danger";
                    Message = "Something went wrong updating the Mortgage. Please try again.";
                    Saved = true;
                    return;
                }

                StatusClass = "alert-success";
                Message = "Mortgage updated successfully.";
                Saved = true;
            }
        }

        protected async Task DeleteMortgage()
        {
            var dialogParams = new DialogParameters();
            dialogParams.Add("Message", "Are you sure you want to delete this mortgage?");
            var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Cancelled)
            {
                try
                {
                    await MortgageService.DeleteMortgage(Mortgage.Id);
                }
                catch
                {
                    StatusClass = "alert-danger";
                    Message = "Something went wrong deleting the Mortgage. Please try again.";
                    Saved = true;
                    return;
                }             

                StatusClass = "alert-success";
                Message = "Deleted successfully";
                Saved = true;
            }

        }

        protected void NavigateToOverview()
        {
            NavigationManager.NavigateTo($"/clientdetail/{CustomerId}");
        }

        protected async Task DeleteMortgageFile(Guid id)
        {
            try
            {
                var dialogParams = new DialogParameters();
                dialogParams.Add("Message", "Are you sure you want to delete this mortgage document?");
                var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
                if (!result.Cancelled)
                {
                    await SupportingDocumentService.DeleteMortgageFile(id);
                    StatusClass = "alert-success";
                    Message = "Deleted successfully";
                    Saved = true;
                }

            }
            catch
            {
                StatusClass = "alert-danger";
                Message = "Something went wrong deleting the mortgage file. Please try again.";
                Saved = false;
                return;
            }
        }

        protected void DeleteMortgageFile(MortgageDocument doc)
        {
            Mortgage.SupportingDocuments.Remove(doc);
            var loadedtoRemove = LoadedFiles.FirstOrDefault(x => x.Item1.Name == doc.FileName);
            if (loadedtoRemove.Item1 != null && loadedtoRemove.Item2 != null)
            {
                Array.Clear(loadedtoRemove.Item2, 0, loadedtoRemove.Item2.Length);
                LoadedFiles.Remove(loadedtoRemove);
            }
        }

        protected async Task UploadMortgageFile(string filename, byte[] dataBytes)
        {
            if (id == 0)
            {
                StatusClass = "alert-danger";
                Message = "Save mortgage details before uploading document";
                Saved = true;
                return;
            }

            MortgageDocument sdoc = new MortgageDocument();
            sdoc.MortgageId = this.id;
            sdoc.FileName = filename;
            sdoc.SupportingDocumentType = DocumentTypeEnum.PDF;
            sdoc.File = dataBytes;

            bool succeeded = false;
            try
            {
                succeeded = await SupportingDocumentService.UploadMortgageFile(sdoc);
            }
            catch
            {

            }

            if (succeeded)
            {
                StatusClass = "alert-success";
                Message = "Uploaded successfully";
            }
            else
            {
                StatusClass = "alert-danger";
                Message = "Something went wrong adding the new Mortgage Document. Please try again.";
            }

            Saved = true;
        }

        public DateTimeOffset? EndDate
        {
            get { return GetDTtoDTO(Mortgage.EndDate); }
            set => Mortgage.EndDate = SetDTtoDTO(value);
        }
        
        public DateTimeOffset? PotentialEndDate
        {
            get { return GetDTtoDTO(Mortgage.PotentialEndDate); }
            set => Mortgage.PotentialEndDate = SetDTtoDTO(value);
        }

        public DateTimeOffset? PromotionalEndDate
        {
            get { return GetDTtoDTO(Mortgage.PromotionalEndDate); }
            set => Mortgage.PromotionalEndDate = SetDTtoDTO(value);
        }

        public DateTimeOffset? GetDTtoDTO(DateTime? datetimeIn)
        {
            if (Mortgage != null && datetimeIn != null && datetimeIn.Value != default(DateTime))
            {
                var localTime1 = DateTime.SpecifyKind(datetimeIn.Value, DateTimeKind.Local);
                DateTimeOffset localTime2 = localTime1;
                return localTime2;
            }
            else
            {
                return DateTimeOffset.Now;
            }
        }

        public DateTime SetDTtoDTO(DateTimeOffset? value)
        {
            return (value.HasValue ? value.Value : DateTime.MinValue).ToLocalTime().DateTime;
        }

        protected async Task LoadFiles(InputFileChangeEventArgs e)
        {
            bool success = true;
            var alreadyUploaded = Mortgage.SupportingDocuments.Count();
            var remainingFiles = fileUploadSettings.MaxAllowedFiles - alreadyUploaded;
            if (e.FileCount > remainingFiles)
            {
                var dialogParams = new DialogParameters();
                dialogParams.Add("Message", $"A maximum of five documents can be shown in the app");
                success = false;
                await DialogService.Show<AlertDialog>("Send Notification", dialogParams).Result;

            }
            foreach (var file in e.GetMultipleFiles(remainingFiles))
            {
                try
                {
                    var ext = Path.GetExtension(file.Name);
                    if (ext != ".pdf")
                    {
                        throw new Exception("Pdf files only");
                    }
                    LoadedFiles.Add((file, await GetFileBytes(file)));
                }
                catch (Exception ex)
                {
                    LoadFileStatus = ex.Message;
                    success = false;
                    break;
                }
            }
            if (success)
            {
                if (Mortgage.Id > 0)
                {
                    await UploadFiles();
                }
                else
                {
                    Mortgage.SupportingDocuments.Clear();
                    foreach (var file in LoadedFiles)
                    {
                        Mortgage.SupportingDocuments.Add(new MortgageDocument
                        {
                            FileName = file.Item1.Name,
                            SupportingDocumentType = DocumentTypeEnum.PDF
                        });
                    }
                    StateHasChanged();
                }
            }
        }

        protected async Task UploadFiles()
        {
            SpinnerVisible = "display:block";
            try
            {
                if (LoadedFiles != null && LoadedFiles.Any())
                {
                    var dialogParams = new DialogParameters();

                    var customer = await CustomerService.GetCustomer(customerId);
                    dialogParams.Add("Client", customer.Name);

                    var fileNames = new List<string>();
                    var memoryStreams = new List<MemoryStream>();

                    foreach (var file in LoadedFiles)
                    {
                        if (file.Item1.Size > this.fileUploadSettings.MaxFileSize)
                        {
                            dialogParams.Add("Oversize", "true");
                            continue;
                        }
                        var loopMemoryStream = new MemoryStream(file.Item2);
                        fileNames.Add(file.Item1.Name);
                        memoryStreams.Add(loopMemoryStream);
                    }

                    dialogParams.Add("Filenames", fileNames);
                    dialogParams.Add("MemoryStreams", memoryStreams);

                    var result = await DialogService.Show<FilesConfirmDialog>("Send Notification", dialogParams).Result;
                    if (!result.Cancelled)
                    {
                        SpinnerVisible = "display:block";
                        StateHasChanged();
                        for (int i = 0; i < memoryStreams.Count(); i++)
                        {
                            if (fileNames.Count() > i)
                            {
                                await UploadMortgageFile(fileNames[i], memoryStreams[i].ToArray());
                            }
                            LoadFileStatus = $"Finished loading {i + 1} of {memoryStreams.Count} : {fileNames[i]}";
                        }

                        await SendMessageNotification(customer, upload: true);
                    }
                }
            }
            catch (Exception ex)
            {
                LoadFileStatus = "Something went wrong, please try again";
            }
            finally
            {
                LoadedFiles.Clear();
                SpinnerVisible = "display:none";
                StateHasChanged();
            }
        }

        private string GetMessageMortgageAdded(string customerName, string brokerName, string mortgageName)
        {
            var messageToSend = $"{customerName}, your broker {brokerName} has added a new {mortgageName} mortgage to your BrokerIQ app.";
            return messageToSend;
        }

         private string GetMessageDocumentUploaded (string customerName, string brokerName, string mortgageName)
        {
            var messageToSend = $"{customerName}, your broker {brokerName} has added new mortgage documents to your BrokerIQ app.";
            return messageToSend;
        }


        private async Task SendMessageNotification(Customer customer, bool upload=false)
        {
             var brokerId = 0;
             var brokerName = "";
             var mortgageName = ""
;
            if (IsAdmin)
            {
                brokerName += Brokers.FirstOrDefault(x => x.Id == Mortgage.BrokerId)?.Name ?? "";
                brokerId = Brokers.FirstOrDefault(x => x.Id == Mortgage.BrokerId)?.Id ?? 0;
            }
            else
            {
                brokerName += Broker?.Name ?? "";
                brokerId = Broker?.Id??0;
            }

           if(Mortgage!=null && Mortgage.MortgageType>0)
            {
                mortgageName += Mortgage.MortgageType.GetDisplayName();
            }

            var messageToSend = "";
            if(upload)
            {
                messageToSend = GetMessageDocumentUploaded(customer.FirstName,brokerName,mortgageName);
            }
            else
            {
                messageToSend = GetMessageMortgageAdded(customer.FirstName,brokerName,mortgageName);
            }

            try
            {
                await NotificationService.SendMessageNotification(messageToSend, new List<int> { customerId }, brokerId, updateAppAlert:false);
            }
            catch
            {

            }

        }

        private async Task<byte[]> GetFileBytes(IBrowserFile file)
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            await using var fileStream = new FileStream(path, FileMode.Create);
            await file.OpenReadStream(file.Size).CopyToAsync(fileStream);
            var bytes = new byte[file.Size];
            fileStream.Position = 0;
            await fileStream.ReadAsync(bytes);
            fileStream.Close();
            File.Delete(path);
            return bytes;
        }
    }
}
