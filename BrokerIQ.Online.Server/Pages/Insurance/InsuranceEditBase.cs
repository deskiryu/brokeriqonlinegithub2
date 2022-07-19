using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Pages
{
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using AutoMapper.Mappers;
    using Microsoft.AspNetCore.Components;
    using Models;
    using MudBlazor;
    using BrokerIQ.Dto.Enum;
    using Services.Interface;
    
    using BrokerIQ.Online.Server.Extensions;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.AspNetCore.Components.Forms;
    using BrokerIQ.Online.Server.AppSettings;
    using Microsoft.Extensions.Options;
    using BrokerIQ.Online.Server.Shared;

    public class InsuranceEditBase : ComponentBase
    {
        private int id;
        private string insuranceId;

        private int customerId;
        private string strCustomerId;

        private int? menuPlanId;

        [Inject]
        public IInsuranceService InsuranceService { get; set; }

        [Inject]
        public IInsuranceDocumentService SupportingDocumentService { get; set; }

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

        public Insurance Insurance { get; set; }

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
        public int InsuranceType = 1;

        [Required]
        public int TermType = 0;

        protected List<IBrowserFile> LoadedFiles = new();

        [Parameter]
        public string InsuranceId {
            get => this.insuranceId;
            set
            {
                this.insuranceId = value;
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


        public InsuranceEditBase()
        {
            Insurance = new Insurance();
            Insurance.SupportingDocuments = new List<InsuranceDocument>();
        }

        protected override async Task OnInitializedAsync()
        {
            SpinnerVisible = "display:none";
            fileUploadSettings = this.FileUploadSettingsOption.Value;
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
                    Insurance = (await InsuranceService.GetInsurance(this.id));
                    InsuranceType = (int)Insurance.InsType;
                    TermType = (int)Insurance.TermType;
                    BrokerListId = Insurance.BrokerId;
                }
                else
                {
                    if (user.IsBroker || user.IsBrokerStaff)
                    {
                        Insurance.ContactNumber = Broker?.TelephoneNumber ?? "";
                    }
                }

                var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
                if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("menuplanId", out var _value))
                {
                    var inVal = Convert.ToInt32(_value);
                    this.menuPlanId = inVal > 0 ? inVal : null;
                }
            }
            catch
            {
                StatusClass = "alert-danger";
                Message = "Something went wrong getting insurance details";
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
            Insurance.InsType = (InsuranceEnum)InsuranceType;
            Insurance.TermType = (TermTypeEnum)TermType;
            Insurance.BrokerId = this.BrokerListId;

            if (IsAdmin)
            {

            }
            if (Insurance.Id == 0)
            {
                Insurance.CustomerId = this.customerId;
                if(Insurance.StartDate == DateTime.MinValue)
                {
                    Insurance.StartDate = DateTime.Now;
                }
                if (Insurance.ExpiryDate == DateTime.MinValue)
                {
                    Insurance.ExpiryDate = DateTime.Now;
                }
                if (Insurance.ReviewDate == null || Insurance.ReviewDate == DateTime.MinValue)
                {
                    Insurance.ReviewDate = DateTime.Now;
                }
                //Midnight
                Insurance.StartDate = new DateTime(Insurance.StartDate.Year, Insurance.StartDate.Month, Insurance.StartDate.Day, 0, 0, 0);
                Insurance.ExpiryDate = new DateTime(Insurance.ExpiryDate.Year, Insurance.ExpiryDate.Month, Insurance.ExpiryDate.Day, 0, 0, 0);
                Insurance.ReviewDate = new DateTime(Insurance.ReviewDate.Value.Year, Insurance.ReviewDate.Value.Month, Insurance.ReviewDate.Value.Day, 0, 0, 0);

                Insurance.MenuPlanId = menuPlanId;

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
                if (customer.EmailConfirmed)
                {
                    dialogParams.Add("Message", $"Insurance will be added and a notification will be sent to {customer.Name} about this new insurance.");
                }
                else
                {
                    dialogParams.Add("Message", $"Insurance will be added however a notification will be NOT be sent to {customer.Name} about this new insurance as their email address is not confirmed");
                }

                var fileNamesAndMemoryStreams = new List<(string, MemoryStream)>();
                var fileNamesAndBytes = new List<(string, byte[])>();
                foreach (var file in LoadedFiles)
                {
                    var loopMemoryStream = new MemoryStream();
                    if (file.Size > this.fileUploadSettings.MaxFileSize)
                    {
                        dialogParams.Add("Oversize", "true");
                        continue;
                    }
                    await file.OpenReadStream(this.fileUploadSettings.MaxFileSize).CopyToAsync(loopMemoryStream);
                    fileNamesAndMemoryStreams.Add((file.Name,loopMemoryStream));
                }

                bool agreed;
                if (LoadedFiles.Any())
                {

                    dialogParams.Add("Filenames", fileNamesAndMemoryStreams.Select(x => x.Item1).ToList());
                    dialogParams.Add("MemoryStreams", fileNamesAndMemoryStreams.Select(x => x.Item2).ToList());
                    var result = await DialogService.Show<FilesConfirmDialog>("Insurance Add", dialogParams).Result;
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
                    var result = await DialogService.Show<ConfirmCancelDialog>("Insurance Add", dialogParams).Result;
                    agreed = !result.Cancelled;
                }

                if (agreed)
                {
                    try
                    {
                        await InsuranceService.AddInsurance(Insurance, fileNamesAndBytes);
                    }
                    catch
                    {
                        StatusClass = "alert-danger";
                        Message = "Something went wrong adding the new Insurance. Please try again.";
                        Saved = true;
                        return;
                    }

                    try
                    {
                        await SendMessageNotification(customer);
                    }
                    catch
                    {

                    }

                    StatusClass = "alert-success";
                    Message = "New Insurance added successfully.";
                    Saved = true;
                }

            }
            else
            {
                //Midnight
                Insurance.StartDate = new DateTime(Insurance.StartDate.Year, Insurance.StartDate.Month, Insurance.StartDate.Day, 0, 0, 0);
                Insurance.ExpiryDate = new DateTime(Insurance.ExpiryDate.Year, Insurance.ExpiryDate.Month, Insurance.ExpiryDate.Day, 0, 0, 0);
                Insurance.ReviewDate = new DateTime(Insurance.ReviewDate.Value.Year, Insurance.ReviewDate.Value.Month, Insurance.ReviewDate.Value.Day, 0, 0, 0);

                try
                {
                    await InsuranceService.UpdateInsurance(Insurance);
                }
                catch
                {
                    StatusClass = "alert-danger";
                    Message = "Something went wrong updating the Insurance. Please try again.";
                    Saved = true;
                    return;
                }

                StatusClass = "alert-success";
                Message = "Insurance updated successfully.";
                Saved = true;
            }
        }

        protected async Task DeleteInsurance()
        {
            var dialogParams = new DialogParameters();
            dialogParams.Add("Message", "Are you sure you want to delete this insurance?");
            var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Cancelled)
            {
                try
                {
                    await InsuranceService.DeleteInsurance(Insurance.Id);
                }
                catch
                {
                    StatusClass = "alert-danger";
                    Message = "Something went wrong deleting the Insurance. Please try again.";
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

        protected async Task DeleteInsuranceFile(Guid id)
        {
            try
            {
                var dialogParams = new DialogParameters();
                dialogParams.Add("Message", "Are you sure you want to delete this insurance document?");
                var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
                if (!result.Cancelled)
                {
                    await SupportingDocumentService.DeleteInsuranceFile(id);            
                    StatusClass = "alert-success";
                    Message = "Deleted successfully";
                    Saved = true;
                }
                
            }
            catch
            {
                StatusClass = "alert-danger";
                Message = "Something went wrong deleting the Insurance File. Please try again.";
                Saved = true;
                return;
            }


        }

        protected async Task UploadInsuranceFile(string filename, byte[] dataBytes)
        {
            if (id == 0)
            {
                StatusClass = "alert-danger";
                Message = "Save insurance details before uploading document";
                Saved = true;
                return;
            }

            InsuranceDocument sdoc = new InsuranceDocument();
            sdoc.InsuranceId = this.id;
            sdoc.FileName = filename;
            sdoc.SupportingDocumentType = DocumentTypeEnum.PDF;
            sdoc.File = dataBytes;

            bool succeeded = false;
            try
            {
                succeeded = await SupportingDocumentService.UploadInsuranceFile(sdoc);
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
                Message = "Something went wrong adding the new Insurance Document. Please try again.";
            }

            Saved = true;
        }

        public DateTimeOffset? StartDate
        {
            get { return GetDTtoDTO(Insurance.StartDate); }
            set => Insurance.StartDate = SetDTtoDTO(value);
        }

        public DateTimeOffset? ExpiryDate
        {
            get { return GetDTtoDTO(Insurance.ExpiryDate); }
            set => Insurance.ExpiryDate = SetDTtoDTO(value);
        }

        public DateTimeOffset? ReviewDate
        {
            get {
                var date = Insurance.ReviewDate.HasValue ? Insurance.ReviewDate.Value : DateTime.Today;
                return GetDTtoDTO(date); 
            }
            set => Insurance.ReviewDate = SetDTtoDTO(value);
        }

        public DateTimeOffset? GetDTtoDTO(DateTime datetimeIn)
        {
            if (Insurance != null && datetimeIn != default(DateTime))
            {
                var localTime1 = DateTime.SpecifyKind(datetimeIn, DateTimeKind.Local);
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
            var alreadyUploaded = Insurance.SupportingDocuments.Count();
            var remainingFiles = fileUploadSettings.MaxAllowedFiles - alreadyUploaded;
            if(e.FileCount > remainingFiles)
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
                    LoadedFiles.Add(file);
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
                if(Insurance.Id > 0)
                {
                    await UploadFiles();
                }
                else
                {
                    Insurance.SupportingDocuments.Clear();
                    foreach (var file in LoadedFiles)
                    {
                        Insurance.SupportingDocuments.Add(new InsuranceDocument
                            {
                             FileName = file.Name,
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

                    foreach(var file in LoadedFiles)
                    {
                        var loopMemoryStream = new MemoryStream();
                        if(file.Size> this.fileUploadSettings.MaxFileSize)
                        {
                            dialogParams.Add("Oversize", "true");
                            continue;
                        }
                        await file.OpenReadStream(this.fileUploadSettings.MaxFileSize).CopyToAsync(loopMemoryStream);
                        fileNames.Add(file.Name);   
                        memoryStreams.Add(loopMemoryStream);
                    }

                    dialogParams.Add("Filenames", fileNames);
                    dialogParams.Add("MemoryStreams", memoryStreams);

                    var result = await DialogService.Show<FilesConfirmDialog>("Send Notification", dialogParams).Result;
                    if (!result.Cancelled)
                    {
                        SpinnerVisible = "display:block";
                        StateHasChanged();
                        for(int i=0; i<memoryStreams.Count(); i++)
                        {
                            if (fileNames.Count() > i)
                            {
                                await UploadInsuranceFile(fileNames[i], memoryStreams[i].ToArray());
                            }
                            LoadFileStatus = $"Finished loading {i+1} of {memoryStreams.Count} : {fileNames[i]}";
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

        private string GetMessageInsuranceAdded(string customerName, string brokerName, string insuranceName)
        {
            var messageToSend = $"{customerName}, your broker {brokerName} has added a new {insuranceName} insurance to your BrokerIQ app.";
            return messageToSend;
        }

         private string GetMessageDocumentUploaded (string customerName, string brokerName, string insuranceName)
        {
            var messageToSend = $"{customerName}, your broker {brokerName} has added new {insuranceName} policy documents to your BrokerIQ app.";
            return messageToSend;
        }

        private async Task SendMessageNotification(Customer customer, bool upload=false)
        {
             var brokerId = 0;
             var brokerName = "";
             var insuranceName = "";

            if (IsAdmin)
            {
                brokerName += Brokers.FirstOrDefault(x => x.Id == Insurance.BrokerId)?.Name ?? "";
                brokerId = Brokers.FirstOrDefault(x => x.Id == Insurance.BrokerId)?.Id ?? 0;
            }
            else
            {
                brokerName += Broker?.Name ?? "";
                brokerId = Broker?.Id??0;
            }

           if(Insurance!=null && Insurance.InsType>0)
            {
                insuranceName += Insurance.InsType.GetDisplayName();
            }

            var messageToSend = "";
            if(upload)
            {
                messageToSend = GetMessageDocumentUploaded(customer.FirstName,brokerName,insuranceName);
            }
            else
            {
                messageToSend = GetMessageInsuranceAdded(customer.FirstName,brokerName,insuranceName);
            }

            try
            {
                await NotificationService.SendMessageNotification(messageToSend, new List<int> { customerId }, brokerId);
            }
            catch
            {

            }

        }

        protected Task OnValueChanged(int value)
        {
            if (Enum.IsDefined(typeof(InsuranceEnum), value)) 
            { 
                Insurance.InsType = (InsuranceEnum)value;
                InsuranceType = value;
                if (!Insurance.IsTermTypeInsurance)
                {
                    TermType = (int)TermTypeEnum.None;
                    Insurance.TermType = TermTypeEnum.None;
                    Insurance.ShowTermYears = false;
                    Insurance.ShowTermAmount = false;
                    Insurance.ShowDeferredPeriodWeeks = false;

                }
                StateHasChanged();
            }
            return Task.CompletedTask;
        }


    }
}
