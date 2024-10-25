using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BrokerIQ.Dto;
using BrokerIQ.Dto.CreateDto;
using BrokerIQ.Dto.Enum;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server;
using BrokerIQ.Online.Server.AppSettings;
using BrokerIQ.Online.Server.Extensions;
using BrokerIQ.Online.Server.Models;
using BrokerIQ.Online.Server.Pages.Customer.Components;
using BrokerIQ.Online.Server.Services.Interface;
using BrokerIQ.Online.Server.Shared;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using MudBlazor;

namespace BrokerIQ.Online.Pages
{
    public class CustomerDetailBase : ComponentBase
    {
        [Inject]
        NavigationManager Navigator { get; set; }

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
        public IBrokerIntegrationService BrokerIntegrationService { get; set; }

        [Inject]
        public ICalendlyService CalendlyService { get; set; }

        [Inject]
        public IOptions<CalendlySettings> CalendlySettings { get; set; }

        [Inject]
        public IDocumentVaultTypeService DocumentVaultTypeService { get; set; }

        [Inject]
        public ICustomerAppointmentService CustomerAppointmentService { get; set; }

        [Inject]
        public IOptions<FileUploadSettings> FileUploadSettingsOption { get; set; }

        [Inject]
        public ISnackbar Snackbar { get; set; }

        [Inject]
        protected IJSRuntime js { get; set; }

        [Parameter]
        public string CustomerId { get; set; }

        [Inject]
        public IOptions<TutorialVideos> TutorialVideosOption { get; set; }

        protected TutorialVideos tutorialVideos { get; set; }

        protected User User { get; set; }

        protected const int DefaultMonthsToShow = -1;

        private const string DEFAULT_UPLOAD_CLASS = "col-sm-6 mt-1";

        private FileUploadSettings fileUploadSettings { get; set; }

        public Broker Broker { get; set; }

        public Customer Customer { get; set; }

        public Customer Connection { get; set; }

        public int CustomerCategory { get; set; }

        public IEnumerable<Broker> Brokers { get; set; }

        public IEnumerable<Broker> CustomerBrokers { get; set; }

        public IEnumerable<CustomerDocument> CustomerDocuments { get; set; }

        protected HashSet<CustomerDocument> SelectedItemsCustomerDocuments = new HashSet<CustomerDocument>();

        public DocumentsRequirement DocumentsRequirement { get; set; }

        public IEnumerable<Note> Notes { get; set; }

        public Chat Chat { get; set; }

        protected List<(IBrowserFile, byte[])> LoadedChatFiles = new();

        public string SpinnerVisible { get; set; }

        public string LoadFileStatus { get; set; }

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

        public bool ChatBadgeDot { get; set; }

        public MudBlazor.Color ChatBadgeColour { get; set; }

        public IEnumerable<DocumentVaultTypeDto> DocumentTypeValues = Array.Empty<DocumentVaultTypeDto>();

        public Dictionary<int, int> RequestedDocuments = new Dictionary<int, int>();

        public List<BrokerDefinedMessageDto> MergedMessages = new();

        public BrokerDefinedMessageDto SelectedTemplateMessage { get; set; }

        public DateTime? SelectedTemplateDateReplacement { get; set; }

        public TimeSpan? SelectedTemplateTimeReplacement { get; set; }

        public CustomerCategoryEnum[] CustomerCategoriesByRelevance;

        private System.Threading.Timer timer;
        private System.Threading.Timer timerUploads;

        public MudSelect<string> TemplateSelect { get; set; }

        protected MudDatePicker NoteFilterFrom { get; set; }

        protected MudDatePicker NoteFilterTo { get; set; }

        protected DateTime? noteFilterStartDate = DateTime.UtcNow.AddMonths(DefaultMonthsToShow);

        protected DateTime? noteFilterEndDate = DateTime.UtcNow;

        public DateTime InitialLatestUploadDate { get; set; }

        public int NewClientUploadsCount { get; set; }

        public bool ShouldShowAsDot { get; set; }

        public Color UploadsBadgeColor { get; set; }

        protected string UploadSectionClass { get; set; } = DEFAULT_UPLOAD_CLASS;

        protected string HoverClass;

        private int CurrentRequirementsId = 0;

        protected string EditRequirementsHidden { get; set; } = string.Empty;

        protected string CurrentRequirementsHidden { get; set; } = string.Empty;

        protected void OnDragEnter(DragEventArgs e) => HoverClass = "drag-file-hover";

        protected void OnDragLeave(DragEventArgs e) => HoverClass = string.Empty;

        protected MudTabs Tabs;
        protected int MyMaxAllowedFiles { get; set; }

        protected string CalendlyLoginUri { get; private set; }

        protected bool CalendlyAccessIsAllowed { get; set; } = false;

        protected bool IsConnectedToCalendly => CalendlyUser != null;

        protected CalendlyUserDto CalendlyUser { get; set; }

        protected int LastChatPageLoaded { get; set; }

        protected int ChatPageSize { get; set; } = 25;

        protected bool AllChatMessagesLoaded { get; set; }

        protected override async Task OnInitializedAsync()
        {
            tutorialVideos = TutorialVideosOption.Value;

            User = await AccountService.GetUser();
            fileUploadSettings = this.FileUploadSettingsOption.Value;
            MyMaxAllowedFiles = fileUploadSettings.MaxAllowedFiles;

            ClearUnReadChat();

            CustomerCategoriesByRelevance = Extensions.GetAllCustomerCategories();

            try
            {
                Customer = await CustomerService.GetCustomer(int.Parse(CustomerId));
                CustomerCategory = (int)Customer.CustomerCategory;

                Connection = await CustomerService.GetConnection(Customer.Id);

                CustomerDocuments = await GetCustomerDocuments();
                ResetUploadsBadge();

                await SetNotesFromInterval(DateTime.UtcNow.AddMonths(DefaultMonthsToShow), DateTime.UtcNow);

                if (!User.IsAdmin)
                {
                    Broker = await BrokerService.GetBroker(User.MasterBrokerId, true);

                    await PopulateBrokerDefinedMessages();

                    if (!Broker.ProvidesMortgageServices && !Broker.ProvidesPensionServices)
                    {
                        CustomerCategoriesByRelevance = Extensions.GetFilteredCustomerCategories(new int[] { 0, 2 });

                    }

                    if (Broker.ProvidesBusinessInsuranceServices)
                    {
                        MyMaxAllowedFiles = MyMaxAllowedFiles * 2;
                    }
                }
                else
                {
                    Broker = await BrokerService.GetBroker(Customer.ChosenBrokerId, true);
                }

                await SetupDocumentRequirementSection();
            }
            catch
            {
                StatusClass = "alert-danger";
                Message = "Something went wrong getting customer details";
                Saved = true;
            }

            if (User.IsAdmin)
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

            if (!User.IsAdmin)
            {
                await UpdateChat(firstTime: true);

                timer = new System.Threading.Timer(async _ =>  // async void
                {
                    await UpdateChat();

                }, null, 5000, 5000);

                timerUploads = new System.Threading.Timer(async _ =>  // async void
                {
                    await UpdateCustomerUploads();
                }, null, 60000, 60000);

                var integrations = await BrokerIntegrationService.GetBrokerIntegrations();

                CalendlyAccessIsAllowed = integrations.Any(i => i.Integration == IntegrationEnum.Calendly);

                await SetUserCalendlyDetails();

                CalendlyLoginUri = $"{CalendlySettings.Value.BaseAuthUri}/oauth/authorize?client_id={CalendlySettings.Value.ClientId}&response_type=code&redirect_uri={CalendlySettings.Value.BiqReturnUri}";
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

        private async Task<IEnumerable<CustomerDocument>> GetCustomerDocuments()
        {
            return (await CustomerDocumentService.Get(Customer.Id)).Data;
        }

        private async Task SetupDocumentRequirementSection()
        {
            DocumentTypeValues = await DocumentVaultTypeService.GetAllForBroker(Broker.Id);

            DocumentsRequirement = await DocumentsRequirementService.Get(Customer.Id);
            if (DocumentsRequirement != null)
            {
                CurrentRequirementsId = DocumentsRequirement.Id;
            }

            foreach (var type in DocumentTypeValues)
            {
                RequestedDocuments.Add(type.Id, 0);
            }

            SetRequirementVisibility();
        }

        private async Task SetUserCalendlyDetails()
        {
            CalendlyUser = await CustomerAppointmentService.GetUser();
        }

        private void SetRequirementVisibility()
        {
            EditRequirementsHidden = DocumentsRequirement == null ? string.Empty : "display:none;";
            CurrentRequirementsHidden = DocumentsRequirement != null ? string.Empty : "display:none;";
        }

        protected async Task UpdateChat(bool firstTime = false)
        {
            var response = await ChatService.GetUnRead(Customer.Id);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                Navigator.NavigateTo($"account/logout");
                return;
            }

            if (LastChatPageLoaded == 0)
            {
                Chat = await LoadChatMessages();
            }

            if (firstTime || LastUnReadChat + response.Data != LastUnReadChat)
            {
                UnReadChat += response.Data;
                ChatBadgeColour = UnReadChat > 0 ? MudBlazor.Color.Error : MudBlazor.Color.Transparent;
                ChatBadgeDot = UnReadChat == 0;
                LastUnReadChat = response.Data;

                await InvokeAsync(StateHasChanged);
            }
        }

        protected async Task UpdateCustomerUploads()
        {
            var response = await CustomerDocumentService.Get(Customer.Id);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                Navigator.NavigateTo($"account/logout");
                return;
            }

            CustomerDocuments = response.Data;
            DocumentsRequirement = await DocumentsRequirementService.Get(Customer.Id);

            NewClientUploadsCount = CustomerDocuments.Count(d => d.CreatedDate > InitialLatestUploadDate);
            ShouldShowAsDot = NewClientUploadsCount == 0;
            UploadsBadgeColor = ShouldShowAsDot ? Color.Transparent : Color.Error;

            await InvokeAsync(StateHasChanged);
        }

        protected void ResetUploadsBadge()
        {
            InitialLatestUploadDate = CustomerDocuments.Any() ? CustomerDocuments.Max(d => d.CreatedDate) : new DateTime(1900, 1, 1);
            NewClientUploadsCount = 0;
            ShouldShowAsDot = true;
            UploadsBadgeColor = Color.Transparent;
        }

        protected async Task ChatBrokerChanged()
        {
            LastChatPageLoaded = 0;
            Chat = await ChatService.GetPaged(Customer.Id, BrokerListId, ++LastChatPageLoaded, ChatPageSize);
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

                if (!result.Canceled)
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
                succeeded = await CustomerService.SetProfilePicture(Customer.Id, dataBytes);
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

        protected async Task NewNote()
        {
            var dialogParams = new DialogParameters
            {
                { "Text", string.Empty },
                { "HasNoteReminder", false },
                { "ReminderDateTime", DateTime.UtcNow.Date.Add(TimeSpan.FromDays(7))},
            };

            var result = await DialogService.Show<NoteEditDialog>("New Note", dialogParams).Result;
            if (!result.Canceled)
            {
                var data = result.Data as NoteEditDialog.NoteDetail;

                try
                {
                    await NoteService.SaveNote(data.Text, data.ReminderDate, Customer.Id);

                    Snackbar.Add("Note was saved", Severity.Success);
                }
                catch
                {
                    Snackbar.Add("Unable to save note. Please try again", Severity.Error);
                }

                await SetNotesFromInterval(noteFilterStartDate.Value, noteFilterEndDate.Value);

                StateHasChanged();
            }
        }

        protected async Task EditNote(int id)
        {
            var note = Notes.FirstOrDefault(x => x.Id == id);

            var dialogParams = new DialogParameters
            {
                { "Text", note.Message },
                { "HasNoteReminder", note.ReminderDate != null },
                { "ReminderDateTime", note.ReminderDate.HasValue ? note.ReminderDate.Value : null}
            };

            var result = await DialogService.Show<NoteEditDialog>("Edit Note", dialogParams).Result;
            if (!result.Canceled)
            {
                var data = result.Data as NoteEditDialog.NoteDetail;
                try
                {
                    await NoteService.UpdateNote(data.Text, data.ReminderDate, note.Id);

                    Snackbar.Add("Note was saved", Severity.Success);
                }
                catch
                {
                    Snackbar.Add("Unable to save note. Please try again", Severity.Error);
                }

                await SetNotesFromInterval(noteFilterStartDate.Value, noteFilterEndDate.Value);

                StateHasChanged();
            }
        }

        protected async Task DeleteNote(int id)
        {
            var dialogParams = new DialogParameters
            {
                { "Message", $"Are you sure you want to delete this note?" }
            };

            var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Canceled)
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
        }

        protected async Task InsertTemplateMessage()
        {
            if (SelectedTemplateMessage != null)
            {
                if (SelectedTemplateMessage.Message.Contains("INSERT_DATE"))
                {
                    if (SelectedTemplateDateReplacement.HasValue)
                    {
                        DateTime value = SelectedTemplateDateReplacement.Value;
                        SelectedTemplateMessage.Message = SelectedTemplateMessage.Message.Replace("INSERT_DATE", value.ToBiqDateString());
                    }
                    else
                    {
                        var dialogParams = new DialogParameters
                        {
                            { "Message", "Template requires a DATE to be inserted into message. Please select one from the date picker." }
                        };
                        var result = await DialogService.Show<AlertDialog>("Warning", dialogParams).Result;
                        return;
                    }
                }

                if (SelectedTemplateMessage.Message.Contains("INSERT_TIME"))
                {
                    if (SelectedTemplateTimeReplacement.HasValue)
                    {
                        SelectedTemplateMessage.Message = SelectedTemplateMessage.Message.Replace("INSERT_TIME", SelectedTemplateTimeReplacement.Value.ToBiqTimeString());
                    }
                    else
                    {
                        var dialogParams = new DialogParameters
                        {
                            { "Message", "Template requires a TIME to be inserted into message. Please select one from the date picker." }
                        };
                        var result = await DialogService.Show<AlertDialog>("Warning", dialogParams).Result;
                        return;
                    }
                }

                try
                {
                    ChatDocument attachment = null;

                    if (!string.IsNullOrEmpty(SelectedTemplateMessage.FileName))
                    {
                        attachment = new ChatDocument()
                        {
                            FileName = SelectedTemplateMessage.FileName,
                            File = SelectedTemplateMessage.File
                        };
                    }

                    await NewChat(SelectedTemplateMessage.Message, attachment);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("InsertTemplateMessage: exception - " + ex.Message);
                }
            }
            else
            {
                var dialogParams = new DialogParameters
                {
                    { "Message", "Please select a template from the dropdown menu." }
                };
                var result = await DialogService.Show<AlertDialog>("Warning", dialogParams).Result;
                return;
            }
        }

        protected async Task NewChat(string messageToshow = "", ChatDocument defaultAttachment = null)
        {
            bool succeeded = false;

            var templateFileAttached = false;
            var filesAttached = false;
            var filenames = new List<string>();
            var memoryStreams = new List<MemoryStream>();

            var sdoc = new ChatDocument();

            var dialogParams = new DialogParameters();

            try
            {
                if (defaultAttachment != null)
                {
                    sdoc.FileName = defaultAttachment.FileName;
                    sdoc.File = defaultAttachment.File;
                    filenames.Add(sdoc.FileName);
                    memoryStreams.Add(new MemoryStream(sdoc.File));
                    templateFileAttached = true;
                }
                else if (LoadedChatFiles.Any())
                {
                    foreach (var file in LoadedChatFiles)
                    {
                        var fileName = "";
                        var memoryStream = new MemoryStream();
                        if (file.Item1 != null)
                        {
                            filesAttached = true;
                            fileName = file.Item1.Name;

                            if (file.Item1.Size > this.fileUploadSettings.MaxFileSize)
                            {
                                dialogParams.Add("Oversize", "true");
                                filesAttached = false;
                                continue;
                            }
                            else
                            {

                                sdoc.FileName = fileName;
                                memoryStream = new MemoryStream(file.Item2);
                            }

                            filenames.Add(sdoc.FileName);
                            memoryStreams.Add(memoryStream);
                        }
                    }
                }
                dialogParams.Add("Filenames", filenames);
                dialogParams.Add("MemoryStreams", memoryStreams);
            }
            catch
            {
                templateFileAttached = false;
            }

            dialogParams.Add("PrePopulatedMessage", messageToshow);
            var dialogOptions = new DialogOptions() { MaxWidth = MaxWidth.Small, FullWidth = true };

            var result = await DialogService.Show<MessageSendDialog>("Send Chat", dialogParams, dialogOptions).Result;
            if (!result.Canceled)
            {
                var message = result.Data.ToString();

                try
                {
                    if (!string.IsNullOrEmpty(message))
                    {
                        if (templateFileAttached)
                        {
                            succeeded = (await ChatService.SendWithDoc(message, Customer.Id, sdoc));
                        }
                        else if (filesAttached)
                        {
                            if (filenames.Count == memoryStreams.Count)
                            {
                                for (int i = 0; i < memoryStreams.Count; i++)
                                {
                                    var noNotification = i > 0;
                                    var file = new ChatDocument
                                    {
                                        FileName = filenames[i],
                                        File = memoryStreams[i].ToArray(),
                                        SupportingDocumentType = DocumentTypeEnum.PDF
                                    };
                                    succeeded = (await ChatService.SendWithDoc(noNotification ? string.Empty : message, Customer.Id, file, noNotification));
                                }
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
                    succeeded = false;
                }
                finally
                {
                    ClearLoadedChatDocuments();
                    SpinnerVisible = "display:none";
                    StateHasChanged();
                }
            }
            else
            {
                return;
            }


            if (succeeded)
            {
                await RefreshChatWithDialogMessage(succeeded, "Message sent successfully");
                TemplateSelect.SelectedValues = new string[] { };
                UploadSectionClass = DEFAULT_UPLOAD_CLASS;
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
                LastChatPageLoaded = 0;
                Chat = await ChatService.GetPaged(Customer.Id, Broker.Id, ++LastChatPageLoaded, ChatPageSize);
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
                var result = await DialogService.Show<Server.Shared.ConfirmCancelDialog>("Warning", dialogParams).Result;
                if (!result.Canceled)
                {
                    foreach (var custDoc in SelectedItemsCustomerDocuments)
                    {
                        await DeleteDocumentUpload(custDoc, showDialog: false);
                    }

                    CustomerDocuments = await GetCustomerDocuments();
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
                var result = await DialogService.Show<Server.Shared.ConfirmCancelDialog>("Warning", dialogParams).Result;
                proceed = !result.Canceled;
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

                        CustomerDocuments = await GetCustomerDocuments();
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
            var alreadyUploaded = LoadedChatFiles.Count();
            var remainingFiles = MyMaxAllowedFiles - alreadyUploaded;
            if (e.FileCount > remainingFiles)
            {
                var dialogParams = new DialogParameters();
                dialogParams.Add("Message", $"A maximum of {MyMaxAllowedFiles} documents can be shown in the app");
                await DialogService.Show<AlertDialog>("Send Notification", dialogParams).Result;

            }
            else
            {
                foreach (var file in e.GetMultipleFiles(remainingFiles))
                {
                    try
                    {
                        var ext = Path.GetExtension(file.Name);
                        if (!ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new Exception("Pdf files only");
                        }
                        LoadedChatFiles.Add((file, await GetFileBytes(file)));
                    }
                    catch (Exception ex)
                    {
                        LoadFileStatus = ex.Message;

                        break;
                    }
                }
            }
            StateHasChanged();

        }

        protected void DeleteChatDocument(string Name)
        {
            var loadedtoRemove = LoadedChatFiles.FirstOrDefault(x => x.Item1.Name == Name);
            if (loadedtoRemove.Item1 != null && loadedtoRemove.Item2 != null)
            {
                Array.Clear(loadedtoRemove.Item2, 0, loadedtoRemove.Item2.Length);
                LoadedChatFiles.Remove(loadedtoRemove);
            }
            StateHasChanged();
        }

        protected void ClearLoadedChatDocuments()
        {
            foreach (var file in LoadedChatFiles)
            {
                Array.Clear(file.Item2, 0, file.Item2.Length);
            }
            LoadedChatFiles.Clear();
            StateHasChanged();
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
                    requirementsString += $"{DocumentTypeValues.FirstOrDefault(t => t.Id == req.Key).Name}: {req.Value}\n";
                }
            }

            if (requirementSet)
            {
                var dialogParams = new DialogParameters
                {
                    { "Message", $"Are you sure you want to set the document requirements as the following?\n{requirementsString}" }
                };
                var result = await DialogService.Show<Server.Shared.ConfirmCancelDialog>("Warning", dialogParams).Result;
                if (!result.Canceled)
                {
                    if (CurrentRequirementsId > 0)
                    {
                        var updatedChecks = new List<DocumentsCheckDto>();
                        foreach (var check in documentsRequiredList)
                        {
                            updatedChecks.Add(new DocumentsCheckDto()
                            {
                                DocumentsRequirementId = CurrentRequirementsId,
                                DocuVaultType = check.DocuVaultType,
                                RequiredCount = check.RequiredCount,
                            });
                        }

                        DocumentsRequirement = await DocumentsRequirementService.Update(CurrentRequirementsId, updatedChecks);
                    }
                    else
                    {
                        DocumentsRequirement = await DocumentsRequirementService.Create(Customer.Id, documentsRequiredList);
                    }

                    await UpdateChat(true);

                    SetRequirementVisibility();

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

        protected void EditDocumentRequirements()
        {
            foreach (var document in DocumentsRequirement.DocumentChecks)
            {
                RequestedDocuments[document.DocuVaultType] = document.RequiredCount;
            }

            CurrentRequirementsId = DocumentsRequirement.Id;
            DocumentsRequirement = null;

            SetRequirementVisibility();

            StateHasChanged();
        }

        protected async Task DeleteDocumentRequirements()
        {
            var dialogParams = new DialogParameters();
            dialogParams.Add("Message", $"Are you sure you want to delete the document requirements currently set?");
            var result = await DialogService.Show<Server.Shared.ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Canceled)
            {
                await DocumentsRequirementService.Delete(DocumentsRequirement.Id);
                DocumentsRequirement = null;
                CurrentRequirementsId = 0;

                // reset display
                foreach (var key in RequestedDocuments.Keys.ToList())
                {
                    RequestedDocuments[key] = 0;
                }

                SetRequirementVisibility();
                StateHasChanged();
            }
        }

        private async Task PopulateBrokerDefinedMessages()
        {
            MergedMessages = new List<BrokerDefinedMessageDto>();
            var templates = await BrokerDefinedMessageService.GetAllForCurrentBroker();

            foreach (var template in templates)
            {
                if (!string.IsNullOrWhiteSpace(template.Message))
                {
                    // display broker defined message
                    template.Message = template.Message
                        .Replace("INSERT_CLIENT_NAME", Customer.FirstName)
                        .Replace("INSERT_BROKER_NAME", $"{Broker?.BrokerFirstName} {Broker?.BrokerLastName}")
                        .Replace("INSERT_COMPANY_NAME", Broker?.Name);
                }

                if (template.WelcomeChat == false)
                {
                    MergedMessages.Add(template);
                }
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

            Customer = await CustomerService.GetCustomer(Customer.Id);
        }

        public void Dispose()
        {
            timer?.Dispose();
        }

        protected void ClearUnReadChat()
        {
            UnReadChat = 0;
            ChatBadgeColour = MudBlazor.Color.Transparent;
            ChatBadgeDot = true;
        }

        protected void FilterStartDateChanged(DateTime? newDate)
        {
            noteFilterStartDate = newDate;
            RefreshNotes();
        }

        protected void FilterEndDateChanged(DateTime? newDate)
        {
            noteFilterEndDate = newDate;
            RefreshNotes();
        }

        protected async Task RefreshNotes()
        {
            DateTime startDate = NoteFilterFrom?.Date != null ? NoteFilterFrom.Date.Value : DateTime.UtcNow.AddMonths(DefaultMonthsToShow);
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

            Notes = Notes.Where(n => n.DateTaken >= startDate.Date && n.DateTaken <= endDate.Add(new TimeSpan(23, 59, 59)))
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

        protected async Task ViewLink(string url)
        {
            await Extensions.OpenLinkInNewTab(js, url);
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

        protected async Task GetInsuranceQuote()
        {
            var parameters = new DialogParameters
            {
                { "Customer", Customer }
            };

            var options = new DialogOptions() { MaxWidth = MaxWidth.Small, FullWidth = true };

            var result = await DialogService.Show<IncomeProtectionQuoteDialog>("Income Protection Quote", parameters, options).Result;

            if (!result.Canceled)
            {
                await UpdateChat(true);
            }
        }

        protected bool ShowGetQuote()
        {
            if (Broker == null || !Broker.HasActiveInsuranceQuoteSubscription) return false;

            if (Customer.HasNeeds) return true;

            return (Customer.Employment == EmploymentEnum.Employed || Customer.Employment == EmploymentEnum.SelfEmployed) &&
                !Customer.Insurances.Any(i => i.InsType == InsuranceEnum.Income && i.ExpiryDate > DateTime.UtcNow);
        }

        public async Task OnCustomerConnectionChange()
        {
            Customer = await CustomerService.GetCustomer(int.Parse(CustomerId));

            Connection = await CustomerService.GetConnection(Customer.Id);

            Tabs.ActivatePanel(0);

            StateHasChanged();
        }

        protected async Task ShowCalendlyPopup()
        {
            if (CalendlyUser == null || string.IsNullOrWhiteSpace(CalendlyUser.AccessToken) || string.IsNullOrWhiteSpace(CalendlyUser.RefreshToken))
            {
                // User has never logged in, or was unable to refresh token after expiration
                NavigationManager.NavigateTo(CalendlyLoginUri);

                return;
            }

            var thisPage = DotNetObjectReference.Create(this);
            await js.InvokeVoidAsync("PassPageComponent", thisPage);

            await js.InvokeVoidAsync("showCalendlyPopup", CalendlyUser.SchedulingReference, Customer.Name, Customer.EmailAddress);
        }

        [JSInvokable]
        public async void CreateNewAppointment(string url)
        {
            await CustomerAppointmentService.Create(new CustomerAppointment()
            {
                CustomerId = Customer.Id,
                ExternalEventId = url
            });
        }

        public async Task DisconnectFromCalendly()
        {
            await CalendlyService.Disconnect();

            await SetUserCalendlyDetails();

            StateHasChanged();
        }

        protected async Task LoadMoreChatMessages()
        {
            Chat chat = await LoadChatMessages();

            if (chat.Messages.Any())
            {
                Chat.Messages = Chat.Messages.Concat(chat.Messages).ToList();
            }

            StateHasChanged();
        }

        private async Task<Chat> LoadChatMessages()
        {
            var chat = await ChatService.GetPaged(Customer.Id, Broker.Id, ++LastChatPageLoaded, ChatPageSize);

            AllChatMessagesLoaded = !chat.MoreMessagesAvailable;

            return chat;
        }

        protected async Task<IEnumerable<BrokerDefinedMessageDto>> OnTemplateFilter(string value)
        {
            return MergedMessages.Where(mm => mm.Prompt.ToLower().Contains(value.ToLower())).ToArray();
        }
    }
}
