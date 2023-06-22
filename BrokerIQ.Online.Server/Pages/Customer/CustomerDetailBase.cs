using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Pages
{
    using System.IO;
    using BrokerIQ.Dto.CreateDto;
    using BrokerIQ.Dto.Enum;
    using BrokerIQ.Dto.Models;
    using BrokerIQ.Online.Server.AppSettings;
    using BrokerIQ.Online.Server.Extensions;
    using BrokerIQ.Online.Server.Models;
    using BrokerIQ.Online.Server.Shared;
    using BrokerIQ.Online.Services.Interface;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;
    using Microsoft.Extensions.Options;
    using Microsoft.JSInterop;
    using Models;
    using MudBlazor;

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
        public IDocumentsRequirementService DocumentsRequirementService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IMenuPlanService MenuPlanService { get; set; }

        [Inject]
        public INoteService NoteService { get; set; }

        [Inject]
        public IChatService ChatService { get; set; }

        [Inject]
        public IBrokerDefinedMessageService BrokerDefinedMessageService { get; set; }

        [Inject]
        public IOptions<FileUploadSettings> FileUploadSettingsOption { get; set; }

        [Inject]
        protected IJSRuntime js { get; set; }

        private FileUploadSettings fileUploadSettings { get; set; }

        public Customer Customer { get; set; }

        public int CustomerCategory { get; set; }

        public CustomerDocumentDto CustomerProfilePicture { get; set; }

        public IEnumerable<Broker> Brokers { get; set; }

        public string BrokerName { get; set; }

        public bool BrokerHasWhiteLabel { get; set; }
        public bool BrokerHasWhiteLabelAndIsInsuranceOnly { get; set; }

        public IEnumerable<Broker> CustomerBrokers { get; set; }

        public IEnumerable<CustomerDocument> CustomerDocuments { get; set; }

        protected HashSet<CustomerDocument> SelectedItemsCustomerDocuments = new HashSet<CustomerDocument>();

        public DocumentsRequirement DocumentsRequirement { get; set; }

        public IEnumerable<Note> Notes { get; set; }

        public Chat Chat { get; set; }

        public string DragEnterStyle { get; set; }

        protected List<IBrowserFile> LoadedChatFiles = new();

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

        public int LastUnReadChat { get; set; }

        public bool BadgeDot { get; set; }

        public MudBlazor.Color ChatBadgeColour { get; set; }

        public Dictionary<int, string> DocumentTypeEnumValues = new Dictionary<int, string>();

        public Dictionary<DocuVaultTypeEnum, int> RequestedDocuments = new Dictionary<DocuVaultTypeEnum, int>();

        public List<DefinedMessagesDto> MergedMessages = new();

        public string SelectedTemplateMessage { get; set; }

        public DateTime? SelectedTemplateDateReplacement { get; set; }

        public CustomerCategoryEnum[] CustomerCategoriesByRelevance;

        private System.Threading.Timer timer;

        public MudSelect<string> TemplateSelect { get; set; }

        protected MudDatePicker NoteFilterFrom { get; set; }

        protected MudDatePicker NoteFilterTo { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var user = await AccountService.GetUser();
            IsAdmin = user.IsAdmin;
            ClearUnReadChat();

            CustomerCategoriesByRelevance = Extensions.BuildCustomerCategoriesByRelevance();

            try
            {
                Customer = await CustomerService.GetCustomer(int.Parse(CustomerId));
                CustomerCategory = (int)Customer.CustomerCategory;
                CustomerProfilePicture = await CustomerDocumentService.GetProfilePicture(int.Parse(CustomerId));
                CustomerDocuments = await CustomerDocumentService.Get(int.Parse(CustomerId));
                DocumentsRequirement = await DocumentsRequirementService.Get(int.Parse(CustomerId));

                await SetNotesFromInterval(DateTime.UtcNow.AddMonths(-6), DateTime.UtcNow);

                foreach (var item in Enum.GetValues(typeof(DocuVaultTypeEnum)).Cast<DocuVaultTypeEnum>())
                {
                    if (item == DocuVaultTypeEnum.ProfilePicture)
                    {
                        continue;
                    }
                    DocumentTypeEnumValues.Add((int)item, item.GetDisplayName());
                    RequestedDocuments.Add(item, 0);
                }

                BrokerHasWhiteLabel = false;
                BrokerHasWhiteLabelAndIsInsuranceOnly = false;
                if (!IsAdmin)
                {
                    var broker = await BrokerService.GetBroker(user.MasterBrokerId);
                    BrokerName = broker.Name;
                    BrokerHasWhiteLabel = broker.BrokerIdentifier != null && broker.BrokerIdentifier.IdentifierFound;
                    BrokerHasWhiteLabelAndIsInsuranceOnly = BrokerHasWhiteLabel && broker.BrokerIdentifier != null && broker.BrokerIdentifier.InsuranceOnly;
                    await PopulateBrokerDefinedMessages();
                }
                else
                {
                    BrokerHasWhiteLabel = true;
                }
            }
            catch
            {
                StatusClass = "alert-danger";
                Message = "Something went wrong getting customer details";
                Saved = true;
            }

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
                UpdateChat(firstTime: true);
                timer = new System.Threading.Timer(async _ =>  // async void
                {
                    await UpdateChat();
                }, null, 0, 5000);
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
            fileUploadSettings = this.FileUploadSettingsOption.Value;

        }

        protected async Task UpdateChat(bool firstTime = false)
        {
            int latestUnreadchat = await ChatService.GetUnRead(Customer.Id);
            if (firstTime || LastUnReadChat + latestUnreadchat != LastUnReadChat)
            {
                UnReadChat += latestUnreadchat;
                Chat = await ChatService.Get(Customer.Id);
                ChatBadgeColour = UnReadChat > 0 ? MudBlazor.Color.Error : MudBlazor.Color.Transparent;
                BadgeDot = UnReadChat == 0;
                LastUnReadChat = latestUnreadchat;
                await InvokeAsync(StateHasChanged);
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
                        succeeded = await NotificationService.SendMessageNotification(selectedNotification, targetsId, user.MasterBrokerId);
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

        protected async Task ResendEmailCustomer()
        {
            var dialogParams = new DialogParameters();
            dialogParams.Add("Message", $"A verify email will be sent to {Customer.Name}. Continue? ");
            var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Cancelled)
            {
                var resent = await AccountService.ResendEmail(Customer.EmailAddress);
                if (resent)
                {
                    var responseParams = new DialogParameters();
                    responseParams.Add("Message", "Resent successfully");
                    await DialogService.Show<AlertDialog>("Information", responseParams).Result;
                }
                else
                {
                    var responseParams = new DialogParameters();
                    responseParams.Add("Message", "The resend email failed.");
                    await DialogService.Show<AlertDialog>("Information", responseParams).Result;
                }
                NavigationManager.NavigateTo($"/clientlist");
            }
        }

        protected async Task NewNote()
        {
            bool succeeded = false;
            var result = await DialogService.Show<NoteEditDialog>("New Note").Result;
            if (!result.Cancelled)
            {
                var message = result.Data.ToString();

                try
                {
                    if (!string.IsNullOrEmpty(message))
                    {
                        succeeded = (await NoteService.SaveNote(message, Customer.Id)).Id > 0;
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
            if (!result.Cancelled)
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
                            await RefreshNotesWithDialogMessage(false, "Something went wrong updating the Note. Please try again.");
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

        protected async Task InsertTemplateMessage()
        {
            int selectedMessageEnum = 0;
            int.TryParse(SelectedTemplateMessage, out selectedMessageEnum);

            var message = MergedMessages.FirstOrDefault(m => m.BrokerDefinedMessageEnumValue == (BrokerDefinedMessageEnum)selectedMessageEnum);

            if (message != null)
            {
                if (message.BrokerDefinedMessage.Contains("INSERT_DATE"))
                {
                    if (SelectedTemplateDateReplacement.HasValue)
                    {
                        DateTime value = SelectedTemplateDateReplacement.Value;
                        message.BrokerDefinedMessage = message.BrokerDefinedMessage.Replace("INSERT_DATE", value.ToShortDateString());
                    }
                    else
                    {
                        var dialogParams = new DialogParameters();
                        dialogParams.Add("Message", "Template requires a DATE to be inserted into message. Please select one from the date picker.");
                        var result = await DialogService.Show<AlertDialog>("Warning", dialogParams).Result;
                        return;
                    }
                }

                try
                {
                    ChatDocument attachment = new ChatDocument();
                    attachment.FileName = message.FileName;
                    attachment.File = message.File;

                    await NewChat(message.BrokerDefinedMessage, attachment);

                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("InsertTemplateMessage: exception - " + ex.Message);
                }
            }
            else
            {
                var dialogParams = new DialogParameters();
                dialogParams.Add("Message", "Please select a template from the dropdown menu.");
                var result = await DialogService.Show<AlertDialog>("Warning", dialogParams).Result;
                return;
            }
        }

        protected async Task NewChat(string messageToshow = "", ChatDocument defaultAttachment = null)
        {
            bool succeeded = false;

            var fileAttached = false;
            var sdoc = new ChatDocument();
            var memoryStream = new MemoryStream();
            var dialogParams = new DialogParameters();

            try
            {
                if (LoadedChatFiles.Any())
                {
                    var fileName = "";

                    var file = LoadedChatFiles[0];
                    if (file != null)
                    {
                        fileAttached = true;
                        fileName = LoadedChatFiles[0].Name;

                        if (file.Size > this.fileUploadSettings.MaxFileSize)
                        {
                            dialogParams.Add("Oversize", "true");
                            fileAttached = false;
                        }
                        else
                        {
                            await file.OpenReadStream(this.fileUploadSettings.MaxFileSize).CopyToAsync(memoryStream);

                            sdoc.FileName = fileName;
                            sdoc.File = memoryStream.ToArray();
                        }
                    }
                }

                if (defaultAttachment != null)
                {
                    sdoc.FileName = defaultAttachment.FileName;
                    sdoc.File = defaultAttachment.File;
                    memoryStream = new MemoryStream(sdoc.File);

                    fileAttached = true;
                }

                dialogParams.Add("Filenames", new List<string>{
                                sdoc.FileName
                            });
                dialogParams.Add("MemoryStreams", new List<MemoryStream> {
                                memoryStream
                            });
            }
            catch
            {
                fileAttached = false;
            }

            dialogParams.Add("PrePopulatedMessage", messageToshow);

            var result = await DialogService.Show<MessageSendDialog>("Send Chat", dialogParams).Result;
            if (!result.Cancelled)
            {
                var message = result.Data.ToString();

                try
                {
                    if (!string.IsNullOrEmpty(message))
                    {
                        if (fileAttached)
                        {
                            succeeded = (await ChatService.SendWithDoc(message, Customer.Id, sdoc));
                        }
                        else
                        {
                            succeeded = (await ChatService.Send(message, Customer.Id));
                        }
                    }
                }
                catch
                {
                    succeeded = false;
                }
            }
            else
            {
                return;
            }


            if (succeeded)
            {
                await RefreshChatWithDialogMessage(succeeded, "Message sent successfully");
                LoadedChatFiles.Clear();

                TemplateSelect.SelectedValues = new string[] { };
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
                await RefreshNotes();
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

        protected async Task DeleteSelectedDocumentUpload()
        {
            var dialogParams = new DialogParameters();
            if (SelectedItemsCustomerDocuments.Any())
            {
                dialogParams.Add("Message", $"Are you sure you want to delete the selected client documents?");
                var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
                if (!result.Cancelled)
                {
                    foreach (var custDoc in SelectedItemsCustomerDocuments)
                    {
                        await DeleteDocumentUpload(custDoc, showDialog: false);
                    }

                    CustomerDocuments = await CustomerDocumentService.Get(Customer.Id);
                    SelectedItemsCustomerDocuments.Clear();
                    StateHasChanged();
                }

            }
        }

        protected async Task DeleteDocumentUpload(CustomerDocument doc, bool showDialog = true)
        {
            var proceed = true;
            if (showDialog)
            {
                var dialogParams = new DialogParameters();
                dialogParams.Add("Message", $"Are you sure you want to delete this client document?");
                var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
                proceed = !result.Cancelled;
            }
            if (proceed)
            {
                var deleted = await CustomerDocumentService.DeleteCustomerDocument(doc.Id);
                if (deleted)
                {
                    if (showDialog)
                    {
                        var responseParams = new DialogParameters();
                        responseParams.Add("Message", "Deleted successfully");
                        await DialogService.Show<AlertDialog>("Information", responseParams).Result;

                        CustomerDocuments = await CustomerDocumentService.Get(Customer.Id);
                        StateHasChanged();
                    }

                }
                else
                {
                    if (showDialog)
                    {
                        var responseParams = new DialogParameters();
                        responseParams.Add("Message", "The document did not delete.");
                        await DialogService.Show<AlertDialog>("Information", responseParams).Result;
                    }
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
            LoadedChatFiles.Clear();
            foreach (var file in e.GetMultipleFiles(1))
            {
                try
                {
                    var ext = Path.GetExtension(file.Name);
                    if (ext != ".pdf")
                    {
                        throw new Exception("Pdf files only");
                    }
                    LoadedChatFiles.Add(file);
                }
                catch (Exception ex)
                {
                    LoadFileStatus = ex.Message;

                    break;
                }
            }
        }

        protected async Task DeleteChatDocument()
        {
            LoadedChatFiles.Clear();
        }

        protected async Task SubmitDocumentRequirements()
        {
            List<CreateDocumentsCheckDto> documentsRequiredList = new List<CreateDocumentsCheckDto>();
            bool requirementSet = false;
            string requirementsString = String.Empty;
            foreach (var req in RequestedDocuments)
            {
                if (req.Value > 0)
                {
                    CreateDocumentsCheckDto requirement = new CreateDocumentsCheckDto
                    {
                        DocuVaultType = req.Key,
                        RequiredCount = req.Value
                    };
                    documentsRequiredList.Add(requirement);
                    requirementSet = true;
                    requirementsString += $"{req.Key.GetDisplayName()}: {req.Value}\n";
                }
            }

            if (requirementSet)
            {
                var dialogParams = new DialogParameters();
                dialogParams.Add("Message", $"Are you sure you want to set the document requirements as the following?\n{requirementsString}");
                var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
                if (!result.Cancelled)
                {
                    DocumentsRequirement = await DocumentsRequirementService.Create(int.Parse(CustomerId), documentsRequiredList);
                    StateHasChanged();
                }
            }
            else
            {
                var dialogParams = new DialogParameters();
                dialogParams.Add("Message", "No document requirements have been set.");
                var result = await DialogService.Show<AlertDialog>("Warning", dialogParams).Result;
            }
        }

        protected async Task DeleteDocumentRequirements()
        {
            var dialogParams = new DialogParameters();
            dialogParams.Add("Message", $"Are you sure you want to delete the document requirements currently set?");
            var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Cancelled)
            {
                await DocumentsRequirementService.Delete(DocumentsRequirement.Id);
                DocumentsRequirement = null;

                // reset display
                foreach (var key in RequestedDocuments.Keys.ToList())
                {
                    RequestedDocuments[key] = 0;
                }
                StateHasChanged();
            }
        }

        private async Task PopulateBrokerDefinedMessages()
        {
            MergedMessages = new List<DefinedMessagesDto>();
            BrokerDefinedMessage definedMessages = await BrokerDefinedMessageService.Get();

            foreach (BrokerDefinedMessageEnum enumVal in Enum.GetValues(typeof(BrokerDefinedMessageEnum)))
            {
                if (enumVal.IsSystemMessage()) continue;

                var message = definedMessages.BrokerDefinedMessages.FirstOrDefault(m => m.BrokerDefinedMessageEnumValue == enumVal);

                if (message == null) continue;

                if (String.IsNullOrWhiteSpace(message.BrokerDefinedMessage))
                {
                    // display default
                    message.BrokerDefinedMessage = enumVal.GetDisplayName().Replace("INSERT_CLIENT_NAME", Customer.FirstName).Replace("INSERT_BROKER_NAME", BrokerName);
                }
                else
                {
                    // display broker defined message
                    message.BrokerDefinedMessage = message.BrokerDefinedMessage.Replace("INSERT_CLIENT_NAME", Customer.FirstName).Replace("INSERT_BROKER_NAME", BrokerName);
                }

                MergedMessages.Add(message);
            }
        }

        protected void HandleInvalidCustomerCategory()
        {

        }

        protected async Task HandleValidCustomerCategory()
        {
            try
            {
                await CustomerService.SetCustomerCategory(Customer.Id, (CustomerCategoryEnum)CustomerCategory);
            }
            catch
            {
                StatusClass = "alert-danger";
                Message = "Something went wrong setting the category. Please try again.";
                Saved = true;
                return;
            }
        }

        public void Dispose()
        {
            timer?.Dispose();
        }

        protected void ClearUnReadChat()
        {
            UnReadChat = 0;
            ChatBadgeColour = MudBlazor.Color.Transparent;
            BadgeDot = true;
        }

        protected async Task RefreshNotes()
        {
            DateTime startDate = NoteFilterFrom?.Date != null ? NoteFilterFrom.Date.Value : DateTime.UtcNow.AddMonths(-6);
            DateTime endDate = NoteFilterTo?.Date != null ? NoteFilterTo.Date.Value : DateTime.UtcNow;

            if (startDate > endDate)
            {
                var dialogParams = new DialogParameters();
                dialogParams.Add("Message", "Please ensure that the From date is earlier than the To date.");
                await DialogService.Show<AlertDialog>("Invalid Interval", dialogParams).Result;

                return;
            }

            await SetNotesFromInterval(startDate, endDate);

            StateHasChanged();
        }

        private async Task SetNotesFromInterval(DateTime startDate, DateTime endDate)
        {
            Notes = await NoteService.GetNotesByBrokerId(Customer.Id);

            Notes = Notes.Where(n => n.DateTaken >= startDate && n.DateTaken <= endDate.Add(new TimeSpan(23, 59, 59)))
                         .OrderByDescending(n => n.DateTaken);
        }

        protected async Task ViewSelectedDocumentUpload()
        {
            foreach (var custDoc in SelectedItemsCustomerDocuments)
            {
                await ViewDocumentUpload(custDoc);
            }
        }

        protected async Task SaveSelectedDocumentUpload()
        {
            foreach (var custDoc in SelectedItemsCustomerDocuments)
            {
                await SaveDocumentUpload(custDoc);
            }
            SelectedItemsCustomerDocuments.Clear();
        }

        protected async Task ViewDocumentUpload(CustomerDocument doc)
        {
            if (doc.SupportingDocumentType == DocumentTypeEnum.JPEG || doc.SupportingDocumentType == DocumentTypeEnum.PNG)
            {
                memoryStream = new MemoryStream(doc.File);
                await PreviewImage();
            }
            else
            {
                await PreviewPdf(doc);
            }
        }

        protected async Task ViewDocumentUpload(ChatDocument doc)
        {
            var converted = new CustomerDocument
            {
                SupportingDocumentType = doc.SupportingDocumentType,
                File = doc.File,

            };
            await ViewDocumentUpload(converted);
        }

        protected async Task SaveDocumentUpload(CustomerDocument doc)
        {
            if (doc.SupportingDocumentType == DocumentTypeEnum.JPEG || doc.SupportingDocumentType == DocumentTypeEnum.PNG)
            {
                imageFileName = doc.Description + ".jpeg";
                imageData = doc.File;
                await SaveImage();
            }
            else
            {
                await DownloadPdf(doc);
            }
        }

        async Task PreviewImage()
        {
            await Extensions.PreviewFile(js, memoryStream, true);
        }

        async Task SaveImage()
        {
            await Extensions.SaveAs(js, imageFileName, imageData);
        }

        async Task DownloadPdf(CustomerDocument sdoc)
        {
            await Extensions.SaveAs(js, sdoc.FileName, sdoc.File);
        }

        async Task PreviewPdf(CustomerDocument sdoc)
        {
            var memoryStream = new MemoryStream(sdoc.File);
            await Extensions.PreviewFile(js, memoryStream);
        }
    }
}
