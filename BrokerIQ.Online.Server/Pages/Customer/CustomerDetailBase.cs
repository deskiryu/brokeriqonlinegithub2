using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Pages
{
    using Microsoft.AspNetCore.Components;
    using Models;
    using BrokerIQ.Dto.Enum;
    using BrokerIQ.Dto.Models;
    using BrokerIQ.Online.Services.Interface;
    using MudBlazor;
    using System.IO;
    using BrokerIQ.Online.Server.Shared;
    using BrokerIQ.Online.Server.Extensions;
    using Microsoft.AspNetCore.Components.Forms;

    public class CustomerDetailBase : ComponentBase
    {
        [Inject]
        public ICustomerService CustomerService { get; set; }

        [Inject]
        public IBrokerService BrokerService { get; set; }

        [Inject]
        public IAccountService AccountService { get; set; }

        [Inject]
        public ICustomerDocumentService CustomerDocumentService { get; set; }

        [Inject]
        public IDialogService DialogService { get; set; }

        [Inject]
        public INotificationService NotificationService { get; set; }

        [Inject]
        public IAlertService AlertService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IMenuPlanService MenuPlanService { get; set; }

        [Inject]
        public INoteService NoteService { get; set; }

        [Inject]
        public IChatService ChatService { get; set; }

        public Customer Customer { get; set; }

        public CustomerDocumentDto CustomerProfilePicture { get; set; }

        public IEnumerable<Broker> Brokers { get; set; }

        public IEnumerable<Broker> CustomerBrokers { get; set; }

        public IEnumerable<CustomerDocumentDto> CustomerDocuments { get; set; }

        public IEnumerable<Note> Notes { get; set; }

        public Chat Chat { get; set; }

        public string DragEnterStyle { get; set; }

        protected List<IBrowserFile> LoadedFiles = new();

        public string SpinnerVisible { get; set; }
        public string LoadFileStatus { get; set; }

        [Parameter]
        public string CustomerId { get; set; }
        public bool IsAdmin { get; set; }

        protected string Message = string.Empty;
        protected string StatusClass = string.Empty;
        protected bool Saved;

        protected string allNotification;
        protected string selectedNotification;

        protected MemoryStream memoryStream = new MemoryStream();
        protected string imageFileName { get; set; }
        protected byte[] imageData { get; set; }

        public int BrokerListId = 0;

        public int UnReadChat { get; set; }

        public bool BadgeDot { get; set; }

        public MudBlazor.Color ChatBadgeColour { get; set; }

        public Dictionary<int,string> DocumentTypeEnumValues = new Dictionary<int, string>();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                Customer = await CustomerService.GetCustomer(int.Parse(CustomerId));
                CustomerProfilePicture = await CustomerDocumentService.GetProfilePicture(int.Parse(CustomerId));
                CustomerDocuments = await CustomerDocumentService.Get(int.Parse(CustomerId));
                Notes = await NoteService.GetNotesByBrokerId(Customer.Id);
                foreach (var item in Enum.GetValues(typeof(DocuVaultTypeEnum)).Cast<DocuVaultTypeEnum>())
                {
                    if (item == DocuVaultTypeEnum.ProfilePicture)
                    {
                        continue;
                    }
                    DocumentTypeEnumValues.Add((int)item, item.GetDisplayName());
                }
            }
            catch
            {
                StatusClass = "alert-danger";
                Message = "Something went wrong getting customer details";
                Saved = true;
            }


            var user = await AccountService.GetUser();
            IsAdmin = user.IsAdmin;
            if (IsAdmin)
            {
                try
                {
                    Brokers = await BrokerService.GetBrokers();
                }
                catch
                {
                    StatusClass = "alert-danger";
                    Message = "Something went wrong getting customer details";
                    Saved = true;
                }
            }

            if (!IsAdmin)
            {
                UnReadChat = await ChatService.GetUnRead(Customer.Id);
                Chat = await ChatService.Get(Customer.Id);
                ChatBadgeColour = UnReadChat > 0 ? MudBlazor.Color.Error : MudBlazor.Color.Transparent;
                BadgeDot = UnReadChat == 0 ;
            }
            else
            {
                if (Customer != null)
                {
                    if (Customer.ConnectedBrokers != null && Customer.ConnectedBrokers.Any())
                    {
                        CustomerBrokers = Brokers.Where(x => Customer.ConnectedBrokers.Contains(x.Id)).ToList();
                    }
                }
            }


        }

        protected async Task ChatBrokerChanged()
        {
            Chat = await ChatService.Get(Customer.Id, BrokerListId);
        }

        protected async Task SendNotificationToSelected()
        {
            await OpenNotificationDialog();
        }

        protected async Task OpenNotificationDialog(bool sendAll = false)
        {

            if (Customer.EmailConfirmed)
            {                
                var dialogParams = new DialogParameters();

                if (string.IsNullOrEmpty(selectedNotification))
                {
                    dialogParams.Add("Message", $"Please enter a notification to send.");
                    await DialogService.Show<AlertDialog>("Send Notification", dialogParams).Result;
                    return;
                }

                if (selectedNotification.Length > 299)
                {
                    dialogParams.Add("Message", $"Your notification is too long. It needs to be less than 300 letters.");
                    await DialogService.Show<AlertDialog>("Send Notification", dialogParams).Result;
                    return;
                }


                dialogParams.Add("Notification", selectedNotification);

                var targetsName = new List<string>();
                targetsName.Add(Customer.Name);

                //var longlist = string.Join(",", targets);

                dialogParams.Add("Users", targetsName);
                dialogParams.Add("areBrokers", false);
                var result = await DialogService.Show<ScrollableDialog>("Send Notification", dialogParams).Result;

                if (!result.Cancelled)
                {
                    var targetsId = new List<int>();
                    targetsId.Add(Customer.Id);
                    var user = await AccountService.GetUser();


                    var succeeded = false;
                    try
                    {
                        succeeded = await NotificationService.SendMessageNotification(selectedNotification, targetsId,user.MasterBrokerId);
                    }
                    catch
                    {

                    }

                    if (succeeded)
                    {
                        AlertService.Alert(new Models.AlertBIQ
                        {
                            AutoClose = true,
                            Message = "Notification Sent"
                        });
                    }
                    else
                    {
                        AlertService.Error("Notification sending failed");
                    };
                }
            }
            else
            {
                var dialogParams = new DialogParameters();
                dialogParams.Add("Message", $"A notification will NOT be sent to {Customer.Name} as they have not confirmed their email address.");
                var result = await DialogService.Show<AlertDialog>("Send Notification", dialogParams).Result;
            }
        }

        protected void NavigateToOverview()
        {
            // we are already on the correct client's detail page, reset property after saving note
            Saved = false;
        }

        protected async Task DeleteProfilePicture(Guid id)
        {
            var suceeded = false;
            try
            {
                suceeded = await CustomerDocumentService.DeleteProfilePicture(id);
            }
            catch
            {
                StatusClass = "alert-danger";
                Message = "Something went wrong getting insurance details";
                Saved = false;
            }

            if (suceeded)
            {
                StatusClass = "alert-success";
                Message = "Deleted successfully";
            }

            Saved = true;
        }

        protected async Task UploadProfilePicture(string filename, byte[] dataBytes)
        {
            if (Customer == null)
            {
                StatusClass = "alert-danger";
                Message = "No client found";
                Saved = true;
                return;
            }

            CustomerDocument sdoc = new CustomerDocument();
            sdoc.CustomerId = Customer.Id;
            sdoc.FileName = filename;
            sdoc.SupportingDocumentType = DocumentTypeEnum.PNG;
            sdoc.File = dataBytes;

            bool succeeded = false;
            try
            {
                succeeded = await CustomerDocumentService.UploadProfilePicture(sdoc);
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
                Message = "Something went wrong adding the profile picture. Please try again.";
            }

            Saved = true;
        }

        protected async Task DeleteCustomer()
        {
            var dialogParams = new DialogParameters();
            dialogParams.Add("Message", $"Are you absolutely sure you want to delete this client {Customer.Name}? This is a PERMANENT DELETE and cannot be undone.");
            var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Cancelled)
            {
                var deleted = await CustomerService.DeleteCustomer(Customer.Id);
                if (deleted)
                {
                    var responseParams = new DialogParameters();
                    responseParams.Add("Message", "Deleted successfully");
                    await DialogService.Show<AlertDialog>("Information", responseParams).Result;
                }
                else
                {
                    var responseParams = new DialogParameters();
                    responseParams.Add("Message", "The client did not delete.");
                    await DialogService.Show<AlertDialog>("Information", responseParams).Result;
                }
                NavigationManager.NavigateTo($"/clientlist");
            }
        }

        protected async Task NewNote()
        {
            bool succeeded = false;
            var result = await DialogService.Show<NoteEditDialog>("New Note").Result;
            if(!result.Cancelled)
            {
                var message = result.Data.ToString();

                try
                {
                    if (!string.IsNullOrEmpty(message))
                    {
                       succeeded = (await NoteService.SaveNote(message, Customer.Id)).Id>0;
                    }
                }
                catch
                {

                }
            }
            else
            {
                return;
            }

            if (succeeded)
            {
                await RefreshNotesWithDialogMessage(succeeded, "Note added successfully");
            }
            else
            {
                await RefreshNotesWithDialogMessage(succeeded, "Something went wrong adding the note. Please try again.");
            }
        }

        protected async Task EditNote(int id)
        {                
            bool succeeded = false;
            var note = Notes.FirstOrDefault(x => x.Id == id);
            var dialogParams = new DialogParameters();
            dialogParams.Add("Message", note.Message);
            
            var result = await DialogService.Show<NoteEditDialog>("Edit Note", dialogParams).Result;
            if(!result.Cancelled)
            {
                var message = result.Data.ToString();
                try
                {
                    if (!string.IsNullOrEmpty(message))
                    {
                        try
                        {
                            succeeded = (await NoteService.UpdateNote(message, note.Id)).Id > 0;
                        }
                        catch
                        {
                            await RefreshNotesWithDialogMessage(false,"Something went wrong updating the Note. Please try again.");
                        }
                    }
                }
                catch
                {

                }
            }
            else
            {
                return;
            }

            if (succeeded)
            {
                await RefreshNotesWithDialogMessage(succeeded, "Note updated successfully");
            }
            else
            {
                await RefreshNotesWithDialogMessage(succeeded, "Something went wrong updating the note. Please try again");
            }
        }

        protected async Task DeleteNote(int id)
        {
            var dialogParams = new DialogParameters();
            dialogParams.Add("Message", $"Are you sure you want to delete this note?");
            var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Cancelled)
            {
                var deleted = await NoteService.Delete(id);
                if (deleted)
                {
                    await RefreshNotesWithDialogMessage(deleted, "Deleted successfully");
                }
                else
                {
                    await RefreshNotesWithDialogMessage(deleted, "The note did not delete.");
                }
            }
            else
            {
                return;
            }
        }

        protected async Task NewChat()
        {
            bool succeeded = false;
            var result = await DialogService.Show<MessageSendDialog>("Send Message").Result;
            if (!result.Cancelled)
            {
                var message = result.Data.ToString();

                try
                {
                    if (!string.IsNullOrEmpty(message))
                    {
                        if (LoadedFiles.Any())
                        {
                            var fileName = "";
                            var memoryStream = new MemoryStream();
                            var file = LoadedFiles[0];
                            if(file != null)
                            {
                                fileName = LoadedFiles[0].Name;
                                await file.OpenReadStream(1024*1024).CopyToAsync(memoryStream);

                                var sdoc = new ChatDocument();
                                sdoc.FileName = fileName;
                                sdoc.File = memoryStream.ToArray();
                                succeeded = (await ChatService.Send(message, Customer.Id, sdoc));
                            }
                            




                        }
                        else
                        {
                            succeeded = (await ChatService.Send(message, Customer.Id));
                        }

                    }
                }
                catch
                {

                }
            }
            else
            {
                return;
            }


            if (succeeded)
            {
                await RefreshChatWithDialogMessage(succeeded, "Message sent successfully");
            }
            else
            {
                await RefreshChatWithDialogMessage(succeeded, "Something went wrong sending the message.Please try again.");
            }
        }

        /// <summary>
        /// Refreshes notes displayed on webpage if desired. Displays appropriate dialog message.
        /// </summary>
        /// <param name="success">Success of prior API call</param>
        /// <param name="message">Message to be displayed in dialog</param>
        private async Task RefreshNotesWithDialogMessage(bool success, string message)
        {
            if (success)
            {
                Notes = await NoteService.GetNotesByBrokerId(Customer.Id);
                StateHasChanged();
            }
            var responseParams = new DialogParameters();
            responseParams.Add("Message", message);
            await DialogService.Show<AlertDialog>("Information", responseParams).Result;
        }

        /// <summary>
        /// Refreshes chat displayed on webpage if desired. Displays appropriate dialog message.
        /// </summary>
        /// <param name="success">Success of prior API call</param>
        /// <param name="message">Message to be displayed in dialog</param>
        private async Task RefreshChatWithDialogMessage(bool success, string message)
        {
            if (success)
            {
                Chat = await ChatService.Get(Customer.Id);
                StateHasChanged();
            }
            var responseParams = new DialogParameters();
            responseParams.Add("Message", message);
            await DialogService.Show<AlertDialog>("Information", responseParams).Result;
        }


        protected async Task DeleteDocumentUpload(CustomerDocumentDto doc)
        {
            var dialogParams = new DialogParameters();
            dialogParams.Add("Message", $"Are you sure you want to delete this client document?");
            var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Cancelled)
            {
                var deleted = await CustomerDocumentService.DeleteCustomerDocument(doc.Id);
                if (deleted)
                {
                    var responseParams = new DialogParameters();
                    responseParams.Add("Message", "Deleted successfully");
                    await DialogService.Show<AlertDialog>("Information", responseParams).Result;

                    CustomerDocuments = await CustomerDocumentService.Get(Customer.Id);
                    StateHasChanged();

                }
                else
                {
                    var responseParams = new DialogParameters();
                    responseParams.Add("Message", "The document did not delete.");
                    await DialogService.Show<AlertDialog>("Information", responseParams).Result;
                }
            }
        }

        protected async Task LoadFiles(InputFileChangeEventArgs e)
        {
            if (e.FileCount > 1)
            {
                var dialogParams = new DialogParameters();
                dialogParams.Add("Message", $"Only one document per chat message");
                await DialogService.Show<AlertDialog>("Send Notification", dialogParams).Result;

            }
            LoadedFiles.Clear();
            foreach (var file in e.GetMultipleFiles(1))
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

                    break;
                }
            }
        }


    }
}
