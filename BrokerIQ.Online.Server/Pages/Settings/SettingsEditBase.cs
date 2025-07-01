using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.IO;
using BrokerIQ.Dto.Enum;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Shared;
using BrokerIQ.Online.Server.AppSettings;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using MudBlazor;
using BrokerIQ.Online.Services.Interface;
using BrokerIQ.Online.Models.Account;
using System.Linq;
using Microsoft.AspNetCore.WebUtilities;

namespace BrokerIQ.Online.Pages
{
    public class MessageElement
    {
        public int Index { get; set; }
        public string Message { get; set; }
        public string Prompt { get; set; }
        public string FileName { get; set; }
        public byte[] FileContent { get; set; }

        public MessageElement GetCopy()
        {
            return new MessageElement()
            {
                Index = this.Index,
                Message = this.Message,
                Prompt = this.Prompt,
                FileName = this.FileName
            };
        }
    }

    public class SettingsEditBase : ComponentBase
    {
        [Inject]
        public IBrokerDefinedMessageService BrokerDefinedMessageService { get; set; }

        [Inject]
        public IAccountService AccountService { get; set; }

        [Inject]
        public IBrokerService BrokerService { get; set; }

        [Inject]
        public IBrokerStaffService BrokerStaffService { get; set; }

        [Inject]
        public IDialogService DialogService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public ISnackbar Snackbar { get; set; }

        [Inject]
        public IOptions<FileUploadSettings> FileUploadSettingsOption { get; set; }

        protected FileUploadSettings fileUploadSettings { get; set; }

        [Inject]
        public IOptions<TutorialVideos> TutorialVideosOption { get; set; }

        protected TutorialVideos tutorialVideos { get; set; }

        public List<MessageElement> MessageElements = new List<MessageElement>();

        protected List<IBrowserFile> SelectedFiles = new();

        protected bool IsCurrentFileToBeRemoved = false;

        protected string HoverClass;

        protected MudTabs Tabs;

        protected void OnDragEnter(DragEventArgs e) => HoverClass = "drag-file-hover";

        protected void OnDragLeave(DragEventArgs e) => HoverClass = string.Empty;

        private int id;

        protected User User { get; set; }

        protected Broker Broker { get; set; }

        public BrokerStaff BrokerStaff { get; set; }

        public List<Broker> Brokers { get; set; }

        public bool IsAdmin { get; set; }

        public bool IsMinorAdmin { get; set; }

        public bool IsBrokerStaff { get; set; }

        public UpdatePassword MyUpdatePassword { get; set; }

        protected bool isShowOld;
        protected InputType PasswordInputOld = InputType.Password;
        protected string PasswordInputIconOld = Icons.Material.Filled.VisibilityOff;

        protected bool isShowNew;
        protected InputType PasswordInputNew = InputType.Password;
        protected string PasswordInputIconNew = Icons.Material.Filled.VisibilityOff;

        protected bool isShowNewConfirm;
        protected InputType PasswordInputNewConfirm = InputType.Password;
        protected string PasswordInputIconNewConfirm = Icons.Material.Filled.VisibilityOff;

        protected override async Task OnInitializedAsync()
        {
            tutorialVideos = TutorialVideosOption.Value;

            try
            {
                User = await AccountService.GetUser();
                IsAdmin = User.IsAdmin;
                IsMinorAdmin = User.IsMinorAdmin;
                IsBrokerStaff = User.IsBrokerStaff || User.IsAdminStaff;

                if(IsAdmin) Brokers = (await BrokerService.GetBrokers()).ToList();

                LoadBroker();

                MyUpdatePassword = new UpdatePassword();
            }
            catch
            {
                NavigationManager.NavigateTo($"account/logout");
            }

            fileUploadSettings = this.FileUploadSettingsOption.Value;


        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);

            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("pipedrive", out var pipedriveSuccess))
            {
                if(pipedriveSuccess == "True")
                    Snackbar.Add("Pipedrive connection is ON", Severity.Success);
                else
                    Snackbar.Add("Pipedrive connection failed", Severity.Error);
            }
        }

        private async void LoadBroker(int brokerId = 0)
        {
            Broker = null; //to trigger the binding in the message control

            if (IsAdmin && brokerId > 0)
            {
                Broker = await BrokerService.GetBroker(brokerId, eagerload: true);

                StateHasChanged();

                return;
            }

            if (IsBrokerStaff && int.TryParse(User.Id, out int brokerStaffId))
            {
                BrokerStaff = await BrokerStaffService.GetBrokerStaff(brokerStaffId);
            }

            if (User.MasterBrokerId > 0)
            {
                Broker = await BrokerService.GetBroker(User.MasterBrokerId, eagerload: true);
            }

            StateHasChanged();
        }

        protected void LoadFiles(InputFileChangeEventArgs e)
        {
            SelectedFiles.Clear();

            try
            {
                var ext = Path.GetExtension(e.File.Name);
                if (!ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Pdf files only");
                }

                SelectedFiles.Add(e.File);
            }
            catch (Exception ex)
            {
            }
        }

        protected void DeleteDefinedDocument()
        {
            SelectedFiles.Clear();
        }

        protected void RemoveCurrentFile()
        {
            IsCurrentFileToBeRemoved = true;
        }

        protected bool GetEmailPreference(EmailNotificationPreferencesEnum enpm, bool staff = false)
        {
            var BrokerPrefAsInt = (int)Broker.EmailNotificationPreferences;
            if (staff)
            {
                BrokerPrefAsInt = (int)BrokerStaff.EmailNotificationPreferences;
            }
            return (BrokerPrefAsInt & (int)enpm) == (int)enpm;
        }

        protected async Task SetEmailPreference(EmailNotificationPreferencesEnum enpm, bool staff = false)
        {
            var BrokerPrefAsInt = (int)Broker.EmailNotificationPreferences;
            if (staff)
            {
                BrokerPrefAsInt = (int)BrokerStaff.EmailNotificationPreferences;
            }
            if ((BrokerPrefAsInt & (int)enpm) == (int)enpm)
            {
                //already set
                BrokerPrefAsInt &= ~((int)enpm);
            }
            else
            {
                BrokerPrefAsInt |= (int)enpm;
            }
            if (staff)
            {
                BrokerStaff.EmailNotificationPreferences = (EmailNotificationPreferencesEnum)BrokerPrefAsInt;
            }
            else
            {
                Broker.EmailNotificationPreferences = (EmailNotificationPreferencesEnum)BrokerPrefAsInt;
            }

        }

        protected bool GetMobilePreference(MobileNotificationPreferencesEnum enpm, bool staff = false)
        {
            var BrokerPrefAsInt = (int)Broker.MobileNotificationPreferences;
            if (staff)
            {
                BrokerPrefAsInt = (int)BrokerStaff.MobileNotificationPreferences;
            }
            return (BrokerPrefAsInt & (int)enpm) == (int)enpm;
        }

        protected async Task SetMobilePreference(MobileNotificationPreferencesEnum enpm, bool staff = false)
        {
            var BrokerPrefAsInt = (int)Broker.MobileNotificationPreferences;
            if (staff)
            {
                BrokerPrefAsInt = (int)BrokerStaff.MobileNotificationPreferences;
            }
            if ((BrokerPrefAsInt & (int)enpm) == (int)enpm)
            {
                //already set
                BrokerPrefAsInt &= ~((int)enpm);
            }
            else
            {
                BrokerPrefAsInt |= (int)enpm;
            }
            if (staff)
            {
                BrokerStaff.MobileNotificationPreferences = (MobileNotificationPreferencesEnum)BrokerPrefAsInt;
            }
            else
            {
                Broker.MobileNotificationPreferences = (MobileNotificationPreferencesEnum)BrokerPrefAsInt;
            }

        }

        protected async Task HandleValidSubmit()
        {
            var dialogParams = new DialogParameters();
            try
            {
                await this.BrokerService.UpdateBroker(Broker);
            }
            catch
            {
                dialogParams.Add("Message", $"Preference did not save");
                await DialogService.Show<AlertDialog>("Notification Preferences", dialogParams).Result;
                return;
            }

            dialogParams.Add("Message", $"Saved the preferences");
            await DialogService.Show<AlertDialog>("Notification Preferences", dialogParams).Result;

        }

        protected bool GetEmailPreferenceStaff(EmailNotificationPreferencesEnum enpm)
        {
            return GetEmailPreference(enpm, staff: true);
        }

        protected async Task SetEmailPreferenceStaff(EmailNotificationPreferencesEnum enpm)
        {
            SetEmailPreference(enpm, staff: true);
        }

        protected bool GetMobilePreferenceStaff(MobileNotificationPreferencesEnum enpm)
        {
            return GetMobilePreference(enpm, staff: true);
        }

        protected async Task SetMobilePreferenceStaff(MobileNotificationPreferencesEnum enpm)
        {
            SetMobilePreference(enpm, staff: true);
        }

        protected async Task HandleValidSubmitBrokerStaff()
        {
            var dialogParams = new DialogParameters();
            try
            {
                await this.BrokerStaffService.UpdateBrokerStaff(BrokerStaff);
            }
            catch
            {
                dialogParams.Add("Message", $"Preference did not save");
                await DialogService.Show<AlertDialog>("Notification Preferences", dialogParams).Result;
                return;
            }

            dialogParams.Add("Message", $"Saved the preferences");
            await DialogService.Show<AlertDialog>("Notification Preferences", dialogParams).Result;

        }

        protected bool ShowReminderSection()
        {
            if (Broker is null) return false; // disabled while broker is not set

            if (Broker.BrokerIdentifier is null || !Broker.BrokerIdentifier.IdentifierFound) return true;

            return Broker.BrokerIdentifier.HasReminders;
        }

        protected bool ShowAppointmentSection()
        {
            if (Broker is null) return false; // disabled while broker is not set

            if (Broker.BrokerIdentifier is null || !Broker.BrokerIdentifier.IdentifierFound) return true;

            return Broker.BrokerIdentifier.HasAppointments;
        }

        public void OnBrokerChanged(int brokerId)
        {
            LoadBroker(brokerId);
        }
    }
}
