using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Pages
{
    using System;
    using System.IO;
    using System.Linq;
    using BrokerIQ.Dto.Enum;
    using BrokerIQ.Online.Models;
    using BrokerIQ.Online.Server.Extensions;
    using BrokerIQ.Online.Server.Models;
    using BrokerIQ.Online.Server.Shared;
    using BrokerIQ.Online.Services;
    using Microsoft.AspNetCore.Components;
    using MudBlazor;
    using Services.Interface;

    public class MessageElement
    {
        public MessageElement(int index, string message, string prompt)
        {
            Index = index;
            Message = message;
            Prompt = prompt;
        }

        public int Index { get; set; }
        public string Message { get; set; }
        public string Prompt { get; set; }
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

        public List<MessageElement> BrokerDefinedMessages = new List<MessageElement>();

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
                    if(IsBrokerStaff)
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
        }

        protected async Task PopulateBrokerDefinedMessages()
        {
            BrokerDefinedMessage brokerDefinedMessage = await BrokerDefinedMessageService.Get();
            BrokerDefinedMessages = new List<MessageElement>();

            foreach (BrokerDefinedMessageEnum enumVal in Enum.GetValues(typeof(BrokerDefinedMessageEnum)))
            {
                string message = String.Empty;

                //Only 20 for now
                if (enumVal != BrokerDefinedMessageEnum.TickBoxMessage1 &&
                    enumVal != BrokerDefinedMessageEnum.TickBoxMessage2 &&
                    enumVal != BrokerDefinedMessageEnum.TickBoxMessage3 &&
                    enumVal != BrokerDefinedMessageEnum.TickBoxMessage4 &&
                    enumVal != BrokerDefinedMessageEnum.TickBoxMessage5 &&
                    enumVal != BrokerDefinedMessageEnum.TickBoxMessage6 &&
                    enumVal != BrokerDefinedMessageEnum.TickBoxMessage7 &&
                    enumVal != BrokerDefinedMessageEnum.TickBoxMessage8 &&
                    enumVal != BrokerDefinedMessageEnum.TickBoxMessage9 &&
                    enumVal != BrokerDefinedMessageEnum.TickBoxMessage10 &&
                    enumVal != BrokerDefinedMessageEnum.TickBoxMessage11 &&
                    enumVal != BrokerDefinedMessageEnum.TickBoxMessage12 &&
                    enumVal != BrokerDefinedMessageEnum.TickBoxMessage13 &&
                    enumVal != BrokerDefinedMessageEnum.TickBoxMessage14 &&
                    enumVal != BrokerDefinedMessageEnum.TickBoxMessage15 &&
                    enumVal != BrokerDefinedMessageEnum.TickBoxMessage16 &&
                    enumVal != BrokerDefinedMessageEnum.TickBoxMessage17 &&
                    enumVal != BrokerDefinedMessageEnum.TickBoxMessage18 &&
                    enumVal != BrokerDefinedMessageEnum.TickBoxMessage19 &&
                    enumVal != BrokerDefinedMessageEnum.TickBoxMessage20)
                {
                    continue;
                }

                foreach (var item in brokerDefinedMessage.BrokerDefinedMessages.Where(
                    b => b.BrokerDefinedMessageEnumValue == enumVal && !b.BrokerDefinedMessage.Equals(enumVal.GetDisplayName())))
                {
                    message = item.BrokerDefinedMessage;
                    
                    break;
                }

                if (message == String.Empty)
                {
                    // display default
                    BrokerDefinedMessages.Add(new MessageElement((int)enumVal, enumVal.GetDisplayName(), enumVal.GetDisplayPrompt()));
                }
                else
                {
                    // display broker defined message
                    BrokerDefinedMessages.Add(new MessageElement((int)enumVal, message, enumVal.GetDisplayPrompt()));
                }
            }

            StateHasChanged();
        }

        protected bool GetEmailPreference(EmailNotificationPreferencesEnum enpm, bool staff=false)
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
