using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

using BrokerIQ.Dto.Enum;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.AppSettings;
using BrokerIQ.Online.Server.Extensions;
using BrokerIQ.Online.Server.Pages.Insurance.Components;
using BrokerIQ.Online.Server.Shared;
using BrokerIQ.Online.Services.Interface;

using MudBlazor;

namespace BrokerIQ.Online.Pages
{
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
        public Broker Broker { get; set; }

        public string DragEnterStyle { get; set; }

        [Required]
        public int BrokerListId = 1;

        [Required]
        public int InsuranceType = 1;

        [Required]
        public int TermType = 0;

        [Required]
        public int PaymentMethod = 0;

        protected List<(IBrowserFile, byte[])> LoadedFiles = new();

        [Parameter]
        public string InsuranceId
        {
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

        public bool SendNotification { get; set; }

        public bool BrokerHasWhiteLabelAndIsInsuranceOnly { get; set; }

        public List<(int, string)> ConsumerInsurances { get; set; }

        public List<(int, string)> BusinessInsurances { get; set; }

        protected string FormId = "InsuranceForm";

        protected string HoverClass;

        protected int MyMaxAllowedFiles { get; set; }

        public bool AvailableToClient
        {
            get
            {
                return Insurance.AvailableToClient;
            }
            set
            {
                Insurance.AvailableToClient = value;
                SendNotification = Insurance.AvailableToClient;
            }
        }

        protected void OnDragEnter(DragEventArgs e) => HoverClass = "drag-file-hover";

        protected void OnDragLeave(DragEventArgs e) => HoverClass = string.Empty;

        public InsuranceEditBase()
        {
            Insurance = new Insurance
            {
                SupportingDocuments = new List<InsuranceDocument>(),
                AvailableToClient = true
            };
            var insurancevalues = Enum.GetValues(typeof(InsuranceEnum)).Cast<InsuranceEnum>().ToList();
            var consumerInsuranceValues = insurancevalues.Where(x => (int)x < 1000).OrderBy(y => y.GetOrderValue()).ToList();
            ConsumerInsurances = consumerInsuranceValues.Select(x => ((int)x, x.GetDisplayName())).ToList();
            var businessInsuranceValues = insurancevalues.Where(x => (int)x >= 1000).OrderBy(y => y.GetOrderValue()).ToList();
            BusinessInsurances = businessInsuranceValues.Select(x => ((int)x, x.GetDisplayName())).ToList();
        }

        protected override async Task OnInitializedAsync()
        {
            SpinnerVisible = "display:none";
            fileUploadSettings = this.FileUploadSettingsOption.Value;
            SendNotification = true;
            MyMaxAllowedFiles = fileUploadSettings.MaxAllowedFiles;
        }

        protected override async Task OnParametersSetAsync()
        {
            var user = await AccountService.GetUser();
            IsAdmin = user.IsAdmin;
            if (user.IsBroker || user.IsAdminStaff || user.IsBrokerStaff)
            {
                var brokerId = user.MasterBrokerId;

                BrokerListId = brokerId;
                try
                {
                    Broker = await BrokerService.GetBroker(brokerId);
                    BrokerHasWhiteLabelAndIsInsuranceOnly = false;
                    if (!IsAdmin)
                    {
                        BrokerHasWhiteLabelAndIsInsuranceOnly = Broker.BrokerIdentifier != null && Broker.BrokerIdentifier.IdentifierFound && Broker.BrokerIdentifier.InsuranceOnly;
                        MyMaxAllowedFiles = BrokerHasWhiteLabelAndIsInsuranceOnly ? MyMaxAllowedFiles * 2 : MyMaxAllowedFiles;
                    }
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
                BrokerHasWhiteLabelAndIsInsuranceOnly = true;
                MyMaxAllowedFiles = MyMaxAllowedFiles * 2;
                try
                {
                    Brokers = await BrokerService.GetBrokers();

                    var customer = await CustomerService.GetCustomer(customerId);
                    BrokerListId = customer.ChosenBrokerId;
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
                    PaymentMethod = (int)Insurance.PaymentMethod;
                    BrokerListId = Insurance.BrokerId;
                }
                else
                {
                    InsuranceType = ConsumerInsurances.First().Item1;
                    Insurance.InsType = (InsuranceEnum)ConsumerInsurances.First().Item1;
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
            Insurance.PaymentMethod = (PaymentMethodEnum)PaymentMethod;
            Insurance.BrokerId = this.BrokerListId;

            if (IsAdmin)
            {

            }
            if (Insurance.Id == 0)
            {
                Insurance.CustomerId = this.customerId;
                if (Insurance.StartDate == DateTime.MinValue)
                {
                    Insurance.StartDate = DateTime.Now;
                }
                if (Insurance.ReviewDate == null || Insurance.ReviewDate == DateTime.MinValue)
                {
                    Insurance.ReviewDate = DateTime.Now;
                }
                if (Insurance.RetroactiveDate == null || Insurance.RetroactiveDate == DateTime.MinValue)
                {
                    Insurance.RetroactiveDate = DateTime.Now;
                }
                //Midnight
                Insurance.RetroactiveDate = new DateTime(Insurance.RetroactiveDate.Value.Year, Insurance.RetroactiveDate.Value.Month, Insurance.RetroactiveDate.Value.Day, 0, 0, 0);

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
                if (customer.EmailConfirmed && SendNotification == true)
                {
                    dialogParams.Add("Message", $"Insurance will be added and a notification will be sent to {customer.Name} about this new insurance.");
                }
                else if (!customer.EmailConfirmed)
                {
                    dialogParams.Add("Message", $"Insurance will be added however a notification will be NOT be sent to {customer.Name} about this new insurance as their email address is not confirmed");
                }
                else
                {
                    dialogParams.Add("Message", $"Insurance will be added however a notification will be NOT be sent to {customer.Name}.");
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
                        if (SendNotification)
                        {
                            await SendMessageNotification(customer);
                        }
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
                Insurance.RetroactiveDate = new DateTime(Insurance.RetroactiveDate.Value.Year, Insurance.RetroactiveDate.Value.Month, Insurance.RetroactiveDate.Value.Day, 0, 0, 0);

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

        protected void DeleteInsuranceFile(InsuranceDocument doc)
        {
            Insurance.SupportingDocuments.Remove(doc);
            var loadedtoRemove = LoadedFiles.FirstOrDefault(x => x.Item1.Name == doc.FileName);
            if (loadedtoRemove.Item1 != null && loadedtoRemove.Item2 != null)
            {
                Array.Clear(loadedtoRemove.Item2, 0, loadedtoRemove.Item2.Length);
                LoadedFiles.Remove(loadedtoRemove);
            }
        }

        protected void ClearLoadedFiles()
        {
            foreach (var file in LoadedFiles)
            {
                Array.Clear(file.Item2, 0, file.Item2.Length);
            }
            LoadedFiles.Clear();
            StateHasChanged();
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

            var sdoc = new InsuranceDocument()
            {
                InsuranceId = this.id,
                FileName = filename,
                SupportingDocumentType = DocumentTypeEnum.PDF,
                File = dataBytes
            };

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
            get { return Insurance.ExpiryDate.HasValue && Insurance.ExpiryDate.Value != DateTime.MaxValue ? GetDTtoDTO(Insurance.ExpiryDate.Value) : null; }
            set => Insurance.ExpiryDate = SetDTtoDTO(value);
        }

        public DateTimeOffset? ReviewDate
        {
            get
            {
                return GetDTtoDTO(Insurance.ReviewDate);
            }
            set => Insurance.ReviewDate = SetDTtoDTO(value);
        }

        public DateTimeOffset? RetroactiveDate
        {
            get
            {
                var date = Insurance.RetroactiveDate.HasValue ? Insurance.RetroactiveDate.Value : DateTime.Today;
                return GetDTtoDTO(date);
            }
            set => Insurance.RetroactiveDate = SetDTtoDTO(value);
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
            var remainingFiles = MyMaxAllowedFiles - alreadyUploaded;
            if (e.FileCount > remainingFiles)
            {
                var dialogParams = new DialogParameters();
                dialogParams.Add("Message", $"A maximum of {MyMaxAllowedFiles} documents can be shown in the app");
                success = false;
                await DialogService.Show<AlertDialog>("Send Notification", dialogParams).Result;

            }
            foreach (var file in e.GetMultipleFiles(remainingFiles))
            {
                try
                {
                    var ext = Path.GetExtension(file.Name);
                    if (!ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
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
                if (Insurance.Id > 0)
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
                                await UploadInsuranceFile(fileNames[i], memoryStreams[i].ToArray());
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
                ClearLoadedFiles();
                SpinnerVisible = "display:none";
            }
        }

        private string GetMessageInsuranceAdded(string customerName, string brokerName, string insuranceName)
        {
            var messageToSend = $"{customerName}, your broker {brokerName} has added a new {insuranceName} insurance to your app.";
            return messageToSend;
        }

        private string GetMessageDocumentUploaded(string customerName, string brokerName, string insuranceName)
        {
            var messageToSend = $"{customerName}, your broker {brokerName} has added new {insuranceName} policy documents to your app.";
            return messageToSend;
        }

        private async Task SendMessageNotification(Customer customer, bool upload = false)
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
                brokerId = Broker?.Id ?? 0;
            }

            if (Insurance != null && Insurance.InsType > 0)
            {
                insuranceName += Insurance.InsType.GetDisplayName();
            }

            var messageToSend = "";
            if (upload)
            {
                messageToSend = GetMessageDocumentUploaded(customer.FirstName, brokerName, insuranceName);
            }
            else
            {
                messageToSend = GetMessageInsuranceAdded(customer.FirstName, brokerName, insuranceName);
            }

            try
            {
                await NotificationService.SendMessageNotification(messageToSend, new List<int> { customerId }, brokerId, updateAppAlert: false);
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
                if (!BrokerIQ.Dto.Extensions.Extensions.IsTermTypeInsurance(Insurance.InsType))
                {
                    TermType = (int)TermTypeEnum.None;
                    Insurance.TermType = TermTypeEnum.None;
                    Insurance.ShowTermYears = false;
                    Insurance.ShowTermAmount = false;
                    Insurance.ShowDeferredPeriodWeeks = false;

                }
                if (!BrokerIQ.Dto.Extensions.Extensions.IsSecondTermTypeInsurance(Insurance.InsType))
                {
                    Insurance.ShowSecondTermAmount = false;
                }
                StateHasChanged();
            }
            return Task.CompletedTask;
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

        protected async Task OpenAnalyzerDialog()
        {
            var result = await DialogService.Show<DocumentAnalyzerDialog>("Document Analyzer").Result;

            if (!result.Canceled)
            {
                var data = ((IBrowserFile, Insurance))result.Data;
                LoadedFiles.Clear();
                LoadedFiles.Add((data.Item1, await GetFileBytes(data.Item1)));
                Insurance = data.Item2;
                InsuranceType = (int)Insurance.InsType;
                if (Broker!=null)
                {
                    Insurance.ContactNumber = Broker?.TelephoneNumber ?? "";
                }
            }

            StateHasChanged();
        }
    }
}
