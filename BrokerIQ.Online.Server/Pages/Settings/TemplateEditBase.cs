using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Pages
{
    using System;
    using System.IO;
    using System.Linq;
    using BrokerIQ.Dto.Enum;
    using BrokerIQ.Dto.Models;
    using BrokerIQ.Online.Models;
    using BrokerIQ.Online.Server.AppSettings;
    using BrokerIQ.Online.Server.Extensions;
    using BrokerIQ.Online.Server.Models;
    using BrokerIQ.Online.Server.Shared;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;
    using Microsoft.Extensions.Options;
    using MudBlazor;
    using Services.Interface;

    public class MessageElement
    {
        public int Index { get; set; }
        public string Message { get; set; }
        public string Prompt { get; set; }
        public string FileName { get; set; }

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

    public class TemplateEditBase : ComponentBase
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
        public IOptions<FileUploadSettings> FileUploadSettingsOption { get; set; }

        protected FileUploadSettings fileUploadSettings { get; set; }

        public List<MessageElement> MessageElements = new List<MessageElement>();

        protected List<IBrowserFile> SelectedFiles = new();

        protected bool IsCurrentFileToBeRemoved = false;

        private int id;

        public Broker Broker { get; set; }

        public BrokerStaff BrokerStaff { get; set; }

        [Parameter]
        public string BrokerId { get; set; }

        public bool IsAdmin { get; set; }

        public bool IsMinorAdmin { get; set; }

        public bool IsBrokerStaff { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var user = await AccountService.GetUser();
                IsAdmin = user.IsAdmin;
                IsMinorAdmin = user.IsMinorAdmin;
                IsBrokerStaff = user.IsBrokerStaff;
                if (IsAdmin || IsMinorAdmin)
                {
                    id = Int32.Parse(BrokerId);
                    if (id > 0)
                    {
                        Broker = (await BrokerService.GetBroker(id));
                    }
                }
                else
                {
                    if (IsBrokerStaff)
                    {
                        var brokerStaffId = 0;
                        brokerStaffId = Int32.Parse(user.Id);

                        if (brokerStaffId > 0)
                        {
                            BrokerStaff = await BrokerStaffService.GetBrokerStaff(brokerStaffId);
                        }

                    }
                    if (user.MasterBrokerId > 0)
                    {
                        Broker = (await BrokerService.GetBroker(user.MasterBrokerId));
                    }
                }
            }
            catch
            {
                NavigationManager.NavigateTo($"account/logout");
            }
            await PopulateBrokerDefinedMessages();

            fileUploadSettings = this.FileUploadSettingsOption.Value;
        }

        protected async Task PopulateBrokerDefinedMessages()
        {
            BrokerDefinedMessage definedMessages = await BrokerDefinedMessageService.Get();
            MessageElements = new List<MessageElement>();

            foreach (BrokerDefinedMessageEnum enumVal in Enum.GetValues(typeof(BrokerDefinedMessageEnum)))
            {
                if (enumVal.IsSystemMessage()) continue;

                var message = definedMessages.BrokerDefinedMessages.FirstOrDefault(m => m.BrokerDefinedMessageEnumValue == enumVal);

                var element = new MessageElement()
                {
                    Index = (int)enumVal,
                    Message = message == null ? enumVal.GetDisplayName() : message.BrokerDefinedMessage,
                    Prompt = enumVal.GetDisplayPrompt(),
                    FileName = message?.FileName
                };

                MessageElements.Add(element);
            }

            StateHasChanged();
        }

        protected void LoadFiles(InputFileChangeEventArgs e)
        {
            SelectedFiles.Clear();

            try
            {
                var ext = Path.GetExtension(e.File.Name);
                if (ext != ".pdf")
                {
                    throw new Exception("Pdf files only");
                }

                SelectedFiles.Add(e.File);
            }
            catch (Exception ex)
            {
            }
        }

        protected async void CommitMessage(object element)
        {
            // update defined messages in database
            List<DefinedMessagesDto> definedMessages = new List<DefinedMessagesDto>();

            var message = new DefinedMessagesDto
            {
                BrokerDefinedMessageEnumValue = (BrokerDefinedMessageEnum)((MessageElement)element).Index,
                BrokerDefinedMessage = ((MessageElement)element).Message,
            };

            var uploadedFile = SelectedFiles.FirstOrDefault();

            if (uploadedFile != null)
            {
                message.FileName = uploadedFile.Name;

                var contents = new MemoryStream(); ;
                await uploadedFile.OpenReadStream(fileUploadSettings.MaxFileSize).CopyToAsync(contents);
                message.File = contents.ToArray();
            }

            definedMessages.Add(message);
            await BrokerDefinedMessageService.UpdateOrCreate(definedMessages);
            await PopulateBrokerDefinedMessages();

            SelectedFiles.Clear();
            IsCurrentFileToBeRemoved = false;
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
    }
}
