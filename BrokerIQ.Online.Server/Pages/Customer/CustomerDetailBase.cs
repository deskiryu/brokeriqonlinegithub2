using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using BrokerIQ.Dto.Dto;
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
        public IMapper mapper { get; set; }

        [Inject]
        protected IJSRuntime js { get; set; }

        [Parameter]
        public string CustomerId { get; set; }

        [Inject]
        public IOptions<TutorialVideos> TutorialVideosOption { get; set; }

        [Inject]
        public IPipedriveService PipedriveService { get; set; }

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

        public List<BrokerDefinedMessage> MergedMessages = new();

        public BrokerDefinedMessage SelectedTemplateMessage { get; set; }

        public string SelectedTemplateMessagePreview { get; set; }

        public bool ShowInsertDate { get; set; }

        public bool ShowInsertTime { get; set; }

        public DateTime? SelectedTemplateDateReplacement { get; set; }

        public TimeSpan? SelectedTemplateTimeReplacement { get; set; }

        public bool ShowTemplatePdf { get; set; }

        public string TemplatePdfName { get; set; }

        public CustomerCategoryEnum[] CustomerCategoriesByRelevance;

        private System.Threading.Timer timer;
        private System.Threading.Timer timerUploads;

        public MudAutocomplete<BrokerDefinedMessage> TemplateAutoComplete { get; set; }

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

        protected bool AllDraftChatMessagesLoaded { get; set; }

        protected bool ShowScheduledChat { get; set; } = false;

        protected string ChatButtonStyle => ShowScheduledChat ? $"color:{Colors.Shades.Black};" : string.Empty;

        protected string ScheduledChatButtonStyle => ShowScheduledChat ? string.Empty : $"color:{Colors.Shades.Black};";

        public bool IsZippingFiles { get; set; }

        public bool DisableSelectedFilesButton => !SelectedItemsCustomerDocuments.Any() || IsZippingFiles;

        protected PipedriveAccessDetailsDto PipedriveDetails { get; set; }

        protected bool IsSyncing { get; set; }

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

                DocumentTypeValues = await DocumentVaultTypeService.GetAllForBroker(Customer.ChosenBrokerId);

                if (!User.IsAdmin)
                {
                    Broker = await BrokerService.GetBroker(User.MasterBrokerId, true);

                    await PopulateBrokerDefinedMessages();

                    if (!Broker.ProvidesMortgageServices && !Broker.ProvidesWealthServices)
                    {
                        CustomerCategoriesByRelevance = Extensions.GetFilteredCustomerCategories(new int[] { 0, 2 });

                    }

                    if (Broker.ProvidesBusinessInsuranceServices)
                    {
                        MyMaxAllowedFiles *= 2;
                    }

                    PipedriveDetails = await BrokerService.GetBrokerPipedriveDetails(Broker.Id);

                }
                else
                {
                    Broker = await BrokerService.GetBroker(Customer.ChosenBrokerId, true);
                }
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
                await UpdateChat();
                timer = new System.Threading.Timer(async _ =>  // async void
                {
                    await UpdateChat();
                }, null, 5000, 5000);

                timerUploads = new System.Threading.Timer(async _ =>  // async void
                {
                    await UpdateCustomerUploads();
                }, null, 10000, 60000);

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

        private bool IsInChatTab()
        {
            if (Tabs is null) return false;

            return Tabs.ActivePanel.ID?.ToString() == "pn_chat";
        }

        private async Task<IEnumerable<CustomerDocument>> GetCustomerDocuments()
        {
            List<CustomerDocument> documents = new List<CustomerDocument>();
            documents.AddRange((await CustomerDocumentService.Get(Customer.Id)).Data);

            if (Connection != null)
            {
                var connectionDocuments = await CustomerDocumentService.Get(Connection.Id);
                if (connectionDocuments.Data != null) documents.AddRange(connectionDocuments.Data);
            }

            return documents;
        }

        private async Task SetUserCalendlyDetails()
        {
            CalendlyUser = await CustomerAppointmentService.GetUser();
        }

        protected async Task UpdateChat(bool loadMessages = true)
        {
            var response = await ChatService.GetUnRead(Customer.Id);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                Navigator.NavigateTo($"account/logout");
                return;
            }

            if (IsInChatTab())
            {
                if (loadMessages && LastChatPageLoaded > 0)
                {
                    var newMessages = await LoadNewMessages();

                    Chat.Messages = newMessages.Concat(Chat.Messages).OrderByDescending(m => m.Id).ToList();
                }
            }
            else
            {
                if (loadMessages && LastChatPageLoaded == 0)
                {
                    Chat = await LoadChatMessages();
                }
                else
                {
                    UnReadChat = response.Data;
                    ChatBadgeColour = UnReadChat > 0 ? MudBlazor.Color.Error : MudBlazor.Color.Transparent;
                    ChatBadgeDot = UnReadChat == 0;
                }
            }

            await InvokeAsync(StateHasChanged);
        }

        protected async Task UpdateCustomerUploads()
        {
            var tmpSelectedIds = SelectedItemsCustomerDocuments.Select(d => d.Id).ToArray();

            var response = await CustomerDocumentService.Get(Customer.Id);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                Navigator.NavigateTo($"account/logout");
                return;
            }

            CustomerDocuments = await GetCustomerDocuments();
            DocumentsRequirement = await DocumentsRequirementService.Get(Customer.Id);

            NewClientUploadsCount = CustomerDocuments.Count(d => d.CreatedDate > InitialLatestUploadDate);
            ShouldShowAsDot = NewClientUploadsCount == 0;
            UploadsBadgeColor = ShouldShowAsDot ? Color.Transparent : Color.Error;

            SelectedItemsCustomerDocuments.Clear();
            foreach (var document in CustomerDocuments)
            {
                if (tmpSelectedIds.Contains(document.Id)) SelectedItemsCustomerDocuments.Add(document);
            }

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
                    }
                    ;
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

            var dialogOptions = new DialogOptions() { MaxWidth = MaxWidth.Medium, FullWidth = true };

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

        protected async Task InsertDateToTemplateMessage(DateTime? dateIn)
        {
            SelectedTemplateDateReplacement = dateIn;
            UpdatePreviewWithTimes();
        }

        protected void InsertTimeToTemplateMessage(TimeSpan? timeIn)
        {
            SelectedTemplateTimeReplacement = timeIn;
            UpdatePreviewWithTimes();
        }

        private void UpdatePreviewWithTimes()
        {
            if (SelectedTemplateMessage != null)
            {
                SelectedTemplateMessagePreview = SelectedTemplateMessage.Message;
                if (SelectedTemplateMessage.Message.Contains("INSERT_DATE"))
                {
                    if (SelectedTemplateDateReplacement.HasValue)
                    {
                        SelectedTemplateMessagePreview = SelectedTemplateMessagePreview.Replace("INSERT_DATE", SelectedTemplateDateReplacement.Value.ToBiqDateString());
                    }
                }

                if (SelectedTemplateMessage.Message.Contains("INSERT_TIME"))
                {
                    if (SelectedTemplateTimeReplacement.HasValue)
                    {
                        SelectedTemplateMessagePreview = SelectedTemplateMessagePreview.Replace("INSERT_TIME", SelectedTemplateTimeReplacement.Value.ToBiqTimeString());
                    }
                }
            }
        }

        protected async Task<bool> CheckTemplateMessage()
        {
            bool readyToGo = true;
            if (SelectedTemplateMessage != null)
            {
                if (SelectedTemplateMessagePreview.Contains("INSERT_DATE") || SelectedTemplateMessagePreview.Contains("INSERT_TIME"))
                {
                    var dialogParams = new DialogParameters
                    {
                        { "Message", "Template requires a the date and/or time to be inserted into message. Please select from the date and time pickers." }
                    };
                    readyToGo = false;
                    await DialogService.Show<AlertDialog>("Warning", dialogParams).Result;
                }
                SelectedTemplateMessage.ConvertedMessage = SelectedTemplateMessagePreview;
            }
            return readyToGo;
        }

        protected async Task NewChat()
        {
            bool succeeded = false;

            if (!await CheckTemplateMessage())
            {
                return;
            }

            string messageToshow = "";
            ChatDocument defaultAttachment = null;

            if (SelectedTemplateMessage != null)
            {
                messageToshow = SelectedTemplateMessage.ConvertedMessage;

                try
                {

                    if (!string.IsNullOrEmpty(SelectedTemplateMessage.FileName))
                    {
                        defaultAttachment = new ChatDocument()
                        {
                            FileName = SelectedTemplateMessage.FileName,
                            File = SelectedTemplateMessage.File
                        };
                    }

                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("InsertTemplateMessage: exception - " + ex.Message);
                }
            }

            var filesAttached = false;
            var filenames = new List<string>();
            var memoryStreams = new List<MemoryStream>();
            var sdoc = new ChatDocument();
            var dialogParams = new DialogParameters();

            try
            {
                if (defaultAttachment != null)
                {
                    filesAttached = true;
                    sdoc.FileName = defaultAttachment.FileName;
                    sdoc.File = defaultAttachment.File;
                    filenames.Add(sdoc.FileName);
                    memoryStreams.Add(new MemoryStream(sdoc.File));
                }

                if (LoadedChatFiles.Any())
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
                filesAttached = false;
            }

            dialogParams.Add("AllowScheduledMessage", true);
            dialogParams.Add("PrePopulatedMessage", messageToshow);
            var dialogOptions = new DialogOptions() { MaxWidth = MaxWidth.Small, FullWidth = true };

            var result = await DialogService.Show<MessageSendDialog>("Send Chat", dialogParams, dialogOptions).Result;
            if (result.Canceled) return;

            var message = (MessageSendDialog.MessageSendModel)result.Data;

            try
            {
                if (message.IsDelayedMessage)
                {
                    succeeded = await CreateDraftMessage(defaultAttachment, filenames, memoryStreams, message);
                }
                else
                {
                    if (filesAttached)
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
                                succeeded = (await ChatService.SendWithDoc(noNotification ? string.Empty : message.MessageToSend, Customer.Id, file, noNotification));
                            }
                        }
                    }
                    else
                    {
                        succeeded = await ChatService.Send(message.MessageToSend, Customer.Id);
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

            if (succeeded)
            {
                await RefreshChatWithMessage(succeeded, message.IsDelayedMessage ? "Scheduled message created" : "Message sent successfully");
                await TemplateAutoComplete.Clear();
                UploadSectionClass = DEFAULT_UPLOAD_CLASS;
                SelectedTemplateDateReplacement = null;
                SelectedTemplateTimeReplacement = null;
            }
            else
            {
                await RefreshChatWithMessage(succeeded, message.IsDelayedMessage ? "Unable to create scheduled message" : "Something went wrong sending the message.Please try again.");
            }
        }

        protected async Task ClearAutoComplete()
        {
            await TemplateAutoComplete.Clear();
            SelectedTemplateDateReplacement = null;
            SelectedTemplateTimeReplacement = null;
            ShowTemplatePdf = false;
            TemplatePdfName = string.Empty;
            SelectedTemplateMessage.FileName = string.Empty;
            ShowInsertDate = false;
            ShowInsertTime = false;

        }

        private async Task<bool> CreateDraftMessage(ChatDocument defaultAttachment, List<string> filenames, List<MemoryStream> memoryStreams, MessageSendDialog.MessageSendModel message)
        {
            List<ChatDocument> draftDocuments = BuildDraftDocuments(filenames, memoryStreams);

            var succeeded = draftDocuments.Any() ? await ChatService.CreateDraftWithDocs(message.MessageToSend, Customer.Id, draftDocuments, message.ToBeSentOn.Value) :
            (await ChatService.SendDraft(message.MessageToSend, Customer.Id, message.ToBeSentOn.Value));

            return succeeded;
        }

        private static List<ChatDocument> BuildDraftDocuments(List<string> filenames, List<MemoryStream> memoryStreams)
        {
            var draftDocuments = new List<ChatDocument>();

            if (filenames.Count == memoryStreams.Count)
            {
                for (int i = 0; i < memoryStreams.Count; i++)
                {
                    var file = new ChatDocument
                    {
                        FileName = filenames[i],
                        File = memoryStreams[i].ToArray(),
                        SupportingDocumentType = DocumentTypeEnum.PDF
                    };

                    draftDocuments.Add(file);
                }
            }

            return draftDocuments;
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
        private async Task RefreshChatWithMessage(bool success, string message)
        {
            if (success)
            {
                await UpdateChat();
                StateHasChanged();

                Snackbar.Add(message, Severity.Success);
            }

            Snackbar.Add(message, Severity.Error);
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

        private async Task PopulateBrokerDefinedMessages()
        {
            MergedMessages = new List<BrokerDefinedMessage>();
            var templates = mapper.Map<List<BrokerDefinedMessage>>(await BrokerDefinedMessageService.GetAllForCurrentBroker());

            foreach (var template in templates)
            {
                if (!string.IsNullOrWhiteSpace(template.Message))
                {
                    // display broker defined message
                    template.Message = template.Message
                        .Replace("INSERT_CLIENT_NAME", Connection != null ? $"{Customer.FirstName} and {Connection.FirstName}" : Customer.FirstName)
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
            if (!SelectedItemsCustomerDocuments.Any()) return;

            if (SelectedItemsCustomerDocuments.Count == 1)
            {
                await SaveDocumentUpload(SelectedItemsCustomerDocuments.First());
            }
            else
            {
                await SaveZipFile();
            }

            SelectedItemsCustomerDocuments.Clear();
        }

        private async Task SaveZipFile()
        {
            IsZippingFiles = true;
            await Task.Delay(250);

            var zipName = $"{Customer.Name}-{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.zip";
            using (MemoryStream ms = new MemoryStream())
            {
                //required: using System.IO.Compression;
                using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    var previousNames = new List<string>();

                    foreach (var file in SelectedItemsCustomerDocuments)
                    {
                        var fileName = file.FileName;
                        var nameIndex = 2;

                        while (previousNames.Contains(fileName))
                        {
                            var name = file.FileName[..file.FileName.LastIndexOf(".")];
                            var extension = file.FileName[file.FileName.LastIndexOf(".")..];

                            fileName = $"{name}_{nameIndex++}{extension}";
                        }

                        var entry = zip.CreateEntry(fileName);
                        using (var fileStream = new MemoryStream(file.File))
                        using (var entryStream = entry.Open())
                        {
                            fileStream.CopyTo(entryStream);
                        }

                        previousNames.Add(fileName);
                    }
                }
                await Extensions.SaveAs(js, zipName, ms.ToArray());
            }

            IsZippingFiles = false;
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

        public async Task OnCustomerChange()
        {
            Customer = await CustomerService.GetCustomer(int.Parse(CustomerId));

            Connection = await CustomerService.GetConnection(Customer.Id);

            StateHasChanged();
        }

        protected async Task ShowCalendlyPopup()
        {
            if (!IsLoggedIntoCalendly())
            {
                // User has never logged in, or was unable to refresh token after expiration
                NavigationManager.NavigateTo(CalendlyLoginUri);

                return;
            }

            var thisPage = DotNetObjectReference.Create(this);
            await js.InvokeVoidAsync("PassPageComponent", thisPage);

            await js.InvokeVoidAsync("showCalendlyPopup", CalendlyUser.SchedulingReference, Customer.Name, Customer.EmailAddress);
        }

        protected bool IsLoggedIntoCalendly()
        {
            return CalendlyUser != null && !string.IsNullOrWhiteSpace(CalendlyUser.AccessToken) && !string.IsNullOrWhiteSpace(CalendlyUser.RefreshToken);
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
            if (Broker == null) return new Chat();

            var chat = await ChatService.GetPaged(Customer.Id, Broker.Id, ++LastChatPageLoaded, ChatPageSize, IsInChatTab());

            AllChatMessagesLoaded = !chat.MoreMessagesAvailable;

            return chat;
        }

        private async Task<IList<ChatMessage>> LoadNewMessages()
        {
            var MaxId = Chat.Messages.Max(m => m.Id);
            return (await ChatService.GetPaged(Customer.Id, Broker.Id, 1, ChatPageSize, IsInChatTab())).Messages.Where(m => m.Id > MaxId).ToList();
        }

        protected async Task<IEnumerable<BrokerDefinedMessage>> OnTemplateFilter(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return MergedMessages;

            return MergedMessages.Where(mm => mm.Prompt.ToLower().Contains(value.ToLower())).ToArray();
        }

        protected void OnComboValueChanged(string itemResponse)
        {
            SelectedTemplateTimeReplacement = null;
            SelectedTemplateDateReplacement = null;
            SelectedTemplateMessagePreview = string.Empty;

            if (!string.IsNullOrEmpty(itemResponse))
            {
                SelectedTemplateMessage = MergedMessages.FirstOrDefault(mm => mm.Prompt.ToLower().Contains(itemResponse.ToLower()));
                if (SelectedTemplateMessage != null)
                {
                    SelectedTemplateMessagePreview = SelectedTemplateMessage.Message;
                    ShowInsertDate = SelectedTemplateMessage.Message.Contains("INSERT_DATE");
                    ShowInsertTime = SelectedTemplateMessage.Message.Contains("INSERT_TIME");
                    ShowTemplatePdf = !string.IsNullOrEmpty(SelectedTemplateMessage.FileName);
                    TemplatePdfName = !string.IsNullOrEmpty(SelectedTemplateMessage.FileName) ? SelectedTemplateMessage.FileName : "";
                }
            }
        }

        protected void ChatButtonClicked()
        {
            ShowScheduledChat = false;
        }

        protected void ScheduledChatButtonClicked()
        {
            ShowScheduledChat = true;
        }

        protected async Task EditDraftMessage(ChatDraftMessage draft)
        {
            var dialogParams = new DialogParameters()
            {
                { "PrePopulatedMessage", draft.Message},
                { "AllowScheduledMessage", true }
            };

            var filenames = new List<string>();
            var streams = new List<MemoryStream>();

            if (draft.ChatDocuments.Any())
            {
                for (int i = 0; i < draft.ChatDocuments.Count(); i++)
                {
                    var document = draft.ChatDocuments.ElementAt(i);
                    filenames.Add(document.FileName);
                    streams.Add(new MemoryStream(document.File));
                }

                dialogParams.Add("FileNames", filenames);
                dialogParams.Add("MemoryStreams", streams);
            }

            if (draft.ToBeSentOn is not null)
            {
                dialogParams.Add("MessageSendDate", draft.ToBeSentOn.Value.Date);
                dialogParams.Add("MessageSendTime", draft.ToBeSentOn.Value.TimeOfDay);
            }

            var dialogOptions = new DialogOptions() { MaxWidth = MaxWidth.Small, FullWidth = true };

            var result = await DialogService.Show<MessageSendDialog>("Send Chat", dialogParams, dialogOptions).Result;

            if (result.Canceled) return;

            var message = (MessageSendDialog.MessageSendModel)result.Data;

            draft.Message = message.MessageToSend;
            draft.ToBeSentOn = message.ToBeSentOn;
            draft.ChatDocuments = BuildDraftDocuments(filenames, streams);

            if (await UpdateDraftMessage(draft))
            {
                await RefreshChatWithMessage(true, "Draft update successfully");

                UploadSectionClass = DEFAULT_UPLOAD_CLASS;
            }
            else
            {
                await RefreshChatWithMessage(false, "Something went wrong updating the draft. Please try again.");
            }
        }

        private async Task<bool> UpdateDraftMessage(ChatDraftMessage draft)
        {
            var succeeded = await ChatService.UpdateDraftWithDocs(draft);

            return succeeded;
        }

        protected async void DeleteDraftMessage(ChatDraftMessage message)
        {
            if (await ChatService.DeleteDraft(message))
            {
                Chat.DraftMessages.Remove(message);
                StateHasChanged();

                Snackbar.Add("Draft message was deleted", Severity.Success);
            }
        }

        protected async void SyncWithPipedrive()
        {
            IsSyncing = true;

            try
            {
                if (Customer.PipedriveId is null)
                {
                    var customer = await PipedriveService.SyncCustomer(Customer.Id);

                    if(customer == null)
                    {
                        Snackbar.Add("Sync was unable to create customer, stopping sync.", Severity.Error);

                        IsSyncing = false;
                        StateHasChanged();
                        return;
                    }

                    Customer = customer;
                }

                await PipedriveService.SyncChatMessages(Chat.Id);

                Snackbar.Add("Message sync has concluded.", Severity.Success);
            }
            catch (Exception ex)
            {
                Snackbar.Add("Sync Failed.Please try again in a few minutes.", Severity.Error);
            }
            finally
            {
                IsSyncing = false;
                StateHasChanged();
            }
        }
    }
}
