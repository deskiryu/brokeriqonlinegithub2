using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Dto.Enum;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.AppSettings;
using BrokerIQ.Online.Server.Shared;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using MudBlazor;

namespace BrokerIQ.Online.Pages
{
    public class PensionEditBase : ComponentBase
    {
        [Inject]
        public IPensionService PensionService { get; set; }

        [Inject]
        public IPensionDocumentService SupportingDocumentService { get; set; }

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

        public string LoadFileStatus { get; set; }

        protected int MaxAllowedFiles { get; set; }

        public string SpinnerVisible { get; set; }

        public Pension Pension { get; set; }

        protected string Message = string.Empty;

        protected string StatusClass = string.Empty;

        protected bool Saved;

        public bool IsAdmin { get; set; }

        public IEnumerable<Broker> Brokers { get; set; }

        public Broker Broker { get; set; }

        public Customer Customer { get; set; }

        public string DragEnterStyle { get; set; }

        [Required]
        public int BrokerListId = 1;

        [Parameter]
        public string PensionId { get; set; }

        [Parameter]
        public string CustomerId { get; set; }

        protected int pensionId;

        protected int customerId;

        public bool SendNotification { get; set; } = true;

        protected string FormId = "PensionForm";

        protected string HoverClass;

        public PensionEditBase()
        {
            Pension = new Pension()
            {
                SupportingDocuments = new List<PensionDocument>()
            };
        }

        protected override void OnInitialized()
        {
            SpinnerVisible = "display:none";
            fileUploadSettings = this.FileUploadSettingsOption.Value;
            SendNotification = true;
            MaxAllowedFiles = fileUploadSettings.MaxAllowedFiles;
        }

        protected override async Task OnParametersSetAsync()
        {
            customerId = Int32.Parse(CustomerId);
            Customer = await CustomerService.GetCustomer(customerId);

            pensionId = Int32.Parse(PensionId);

            var user = await AccountService.GetUser();
            IsAdmin = user.IsAdmin;
            if (user.IsBroker || user.IsAdminStaff || user.IsBrokerStaff)
            {
                var brokerId = user.MasterBrokerId;

                BrokerListId = brokerId;
                try
                {
                    Broker = await BrokerService.GetBroker(brokerId);
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

                    BrokerListId = Customer.ChosenBrokerId;
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
                if (pensionId > 0)
                {
                    Pension = (await PensionService.Get(pensionId));
                }
            }
            catch
            {
                StatusClass = "alert-danger";
                Message = "Something went wrong getting Pension details";
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
            if (Pension.Id == 0)
            {
                Pension.CustomerId = Customer.Id;

                var dialogParams = new DialogParameters();
                if (Customer.EmailConfirmed && SendNotification == true)
                {
                    dialogParams.Add("Message", $"Pension will be added and a notification will be sent to {Customer.Name} about this new Pension.");
                }
                else if (!Customer.EmailConfirmed)
                {
                    dialogParams.Add("Message", $"Pension will be added however a notification will be NOT be sent to {Customer.Name} about this new Pension as their email address is not confirmed");
                }
                else
                {
                    dialogParams.Add("Message", $"Pension will be added however a notification will be NOT be sent to {Customer.Name}.");
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
                    var result = await DialogService.Show<FilesConfirmDialog>("Pension Add", dialogParams).Result;
                    agreed = !result.Canceled;
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
                    var result = await DialogService.Show<ConfirmCancelDialog>("Pension Add", dialogParams).Result;
                    agreed = !result.Canceled;
                }

                if (agreed)
                {
                    try
                    {
                        await PensionService.Add(Pension, IsAdmin ? this.BrokerListId : Customer.ChosenBrokerId, fileNamesAndBytes);
                    }
                    catch
                    {
                        StatusClass = "alert-danger";
                        Message = "Something went wrong adding the new Pension. Please try again.";
                        Saved = true;
                        return;
                    }

                    try
                    {
                        if (SendNotification)
                        {
                            await SendMessageNotification(Customer);
                        }
                    }
                    catch
                    {

                    }

                    StatusClass = "alert-success";
                    Message = "New Pension added successfully.";
                    Saved = true;
                }
            }
            else
            {
                try
                {
                    await PensionService.Update(Pension);
                }
                catch
                {
                    StatusClass = "alert-danger";
                    Message = "Something went wrong updating the Pension. Please try again.";
                    Saved = true;
                    return;
                }

                StatusClass = "alert-success";
                Message = "Pension updated successfully.";
                Saved = true;
            }
        }

        protected async Task DeletePension()
        {
            var dialogParams = new DialogParameters();
            dialogParams.Add("Message", "Are you sure you want to delete this pension?");
            var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Canceled)
            {
                try
                {
                    await PensionService.Delete(Pension.Id);
                }
                catch
                {
                    StatusClass = "alert-danger";
                    Message = "Something went wrong deleting the Pension. Please try again.";
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

        private string GetMessagePensionAdded(string customerName, string brokerName, string PensionName)
        {
            var messageToSend = $"{customerName}, your broker {brokerName} has added a new {PensionName} Pension to your app.";
            return messageToSend;
        }

        private async Task SendMessageNotification(Customer customer, bool upload = false)
        {
            var brokerId = 0;
            var brokerName = "";
            var PensionName = ""
;
            if (IsAdmin)
            {
                brokerName += Brokers.FirstOrDefault(x => x.Id == Customer.ChosenBrokerId)?.Name ?? "";
                brokerId = Brokers.FirstOrDefault(x => x.Id == Customer.ChosenBrokerId)?.Id ?? 0;
            }
            else
            {
                brokerName += Broker?.Name ?? "";
                brokerId = Broker?.Id ?? 0;
            }

            var messageToSend = "";
            if (upload)
            {
                messageToSend = GetMessageDocumentUploaded(customer.FirstName, brokerName, PensionName);
            }
            else
            {
                messageToSend = GetMessagePensionAdded(customer.FirstName, brokerName, PensionName);
            }

            try
            {
                await NotificationService.SendMessageNotification(messageToSend, new List<int> { customerId }, brokerId, updateAppAlert: false);
            }
            catch
            {

            }
        }

        private string GetMessageDocumentUploaded(string customerName, string brokerName, string PensionName)
        {
            var messageToSend = $"{customerName}, your broker {brokerName} has added new Pension documents to your app.";
            return messageToSend;
        }

        protected async Task DeletePensionFile(Guid id)
        {
            try
            {
                var dialogParams = new DialogParameters();
                dialogParams.Add("Message", "Are you sure you want to delete this Pension document?");
                var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
                if (!result.Canceled)
                {
                    await SupportingDocumentService.DeletePensionFile(id);
                    StatusClass = "alert-success";
                    Message = "Deleted successfully";
                    Saved = true;
                }

            }
            catch
            {
                StatusClass = "alert-danger";
                Message = "Something went wrong deleting the Pension file. Please try again.";
                Saved = false;
                return;
            }
        }

        protected void OnDragEnter(DragEventArgs e) => HoverClass = "drag-file-hover";

        protected void OnDragLeave(DragEventArgs e) => HoverClass = string.Empty;

        protected async Task LoadFiles(InputFileChangeEventArgs e)
        {
            bool success = true;
            var alreadyUploaded = Pension.SupportingDocuments.Count();
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
                if (Pension.Id > 0)
                {
                    await UploadFiles();
                }
                else
                {
                    Pension.SupportingDocuments.Clear();
                    foreach (var file in LoadedFiles)
                    {
                        Pension.SupportingDocuments.Add(new PensionDocument
                        {
                            FileName = file.Item1.Name,
                            SupportingDocumentType = DocumentTypeEnum.PDF
                        });
                    }
                    StateHasChanged();
                }
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
                    if (!result.Canceled)
                    {
                        SpinnerVisible = "display:block";
                        StateHasChanged();
                        for (int i = 0; i < memoryStreams.Count(); i++)
                        {
                            if (fileNames.Count() > i)
                            {
                                await UploadPensionFile(fileNames[i], memoryStreams[i].ToArray());
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

        protected async Task UploadPensionFile(string filename, byte[] dataBytes)
        {
            if (pensionId == 0)
            {
                StatusClass = "alert-danger";
                Message = "Save Pension details before uploading document";
                Saved = true;
                return;
            }

            var sdoc = new PensionDocument()
            {
                PensionId = this.pensionId,
                FileName = filename,
                SupportingDocumentType = DocumentTypeEnum.PDF,
                File = dataBytes
            };

            bool succeeded = false;
            try
            {
                await PensionService.Update(Pension);
                succeeded = await SupportingDocumentService.UploadPensionFile(sdoc);
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
                Message = "Something went wrong adding the new Pension Document. Please try again.";
            }

            Saved = true;
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

    }
}
