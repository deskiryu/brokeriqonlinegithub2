using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using BrokerIQ.Dto.Dto;
using BrokerIQ.Dto.Enum;
using BrokerIQ.Online.Server.Extensions;
using BrokerIQ.Online.Server.Shared;

using MudBlazor;
using System.Runtime.InteropServices;
using System.Linq;
using System.Collections.Generic;
using BrokerIQ.Online.Services;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Server.Pages.Settings.Components
{
    public partial class ReminderOptionDialog
    {
        [Inject]
        public IDialogService DialogService { get; set; }

        [Microsoft.AspNetCore.Components.CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Microsoft.AspNetCore.Components.Parameter]
        public ReminderOptionDto Option { get; set; }

        [Microsoft.AspNetCore.Components.Parameter]
        public Models.BrokerIdentifier BrokerIdentifier  { get; set; }

        [Inject]
        public IAlertService AlertService { get; set; }

        MudForm form;

        MudSelect<int> ReminderTypeSelect;

        private string OptionLink { get; set; }

        private bool HideLink { get; set; }

        private readonly TimeSpan[] ExpiryTimeSpan = new TimeSpan[]{
            TimeSpan.FromDays(365),
            TimeSpan.FromDays(270),
            TimeSpan.FromDays(240),
            TimeSpan.FromDays(210),
            TimeSpan.FromDays(180),
            TimeSpan.FromDays(150),
            TimeSpan.FromDays(120),
            TimeSpan.FromDays(90),
            TimeSpan.FromDays(60),
            TimeSpan.FromDays(30),
            TimeSpan.FromDays(15),
            TimeSpan.FromDays(7),
            TimeSpan.FromDays(6),
            TimeSpan.FromDays(5),
            TimeSpan.FromDays(4),
            TimeSpan.FromDays(3),
            TimeSpan.FromDays(2),
            TimeSpan.FromDays(1),
        };

        private readonly TimeSpan[] AppointmentTimeSpan = new TimeSpan[]{
            TimeSpan.FromDays(3),
            TimeSpan.FromDays(2),
            TimeSpan.FromDays(1),
            TimeSpan.FromMinutes(180),
            TimeSpan.FromMinutes(120),
            TimeSpan.FromMinutes(60),
            TimeSpan.FromMinutes(45),
            TimeSpan.FromMinutes(30),
            TimeSpan.FromMinutes(15),
            TimeSpan.FromMinutes(10),
            TimeSpan.FromMinutes(5),
        };

        static string[] REMINDER_MESSAGES = new string[] {
            "",
            "The INSERT_INSURANCE_NAME insurance policy of your client INSERT_CLIENT_NAME should be reviewed on INSERT_DATE.",
            "Hi INSERT_CLIENT_NAME, your INSERT_INSURANCE_NAME insurance policy is due for renewal, and should have a review on INSERT_DATE.Contact your Broker for a new quote and prevent your policy being automatically renewed.",
            "Hi INSERT_CLIENT_NAME, your INSERT_INSURANCE_NAME insurance policy is due for renewal, and should have a review on INSERT_DATE.Contact your Broker for a new quote.",
            "The mortgage promotional period of your client INSERT_CLIENT_NAME ends on INSERT_DATE.",
            "Hi INSERT_CLIENT_NAME, your mortgage promotional period ends on INSERT_DATE. Contact your Broker to discuss your mortgage options.",
            // appointment starter templates
            "Hi INSERT_CLIENT_NAME, your appointment with INSERT_BROKER_NAME is on INSERT_DATE at INSERT_TIME in 1 day",
            "Hi INSERT_CLIENT_NAME, your appointment with INSERT_BROKER_NAME is on INSERT_DATE at INSERT_TIME in 3 hours",
            "Hi INSERT_BROKER_NAME, your appointment with INSERT_CLIENT_NAME is on INSERT_DATE at INSERT_TIME in 1 day",
            "Hi INSERT_BROKER_NAME, your appointment with INSERT_CLIENT_NAME is on INSERT_DATE at INSERT_TIME in 1 hour",
        };

        int SelectedStarterTemplate;

        Func<int, string> promptText = i => REMINDER_MESSAGES[i];

        void Submit()
        {
            form.Validate();

            //Find substring
            if (HideLink)
            {

                var found = Option.MessageContent.IndexOf(OptionLink);
                if (found > 0)
                {
                    Option.MessageContent = Option.MessageContent.Insert(found + OptionLink.Length, "-->");
                    Option.MessageContent = Option.MessageContent.Insert(found, "<--");
                }
                else
                {
                    this.AlertService.Warn("The link has been altered and may not show in the reminder correctly");
                }
            }

            if (form.IsValid) MudDialog.Close(DialogResult.Ok(Option));
        }

        void Cancel() => MudDialog.Cancel();

        protected override async Task OnInitializedAsync()
        {
            if(Option.Id > 0)
            {
                HideLink = true;
                var startLink = Option.MessageContent.IndexOf("<--") + 3;
                var endLink = Option.MessageContent.IndexOf("-->");

                if (startLink > 0 && endLink > 0)
                {
                    try
                    {
                        OptionLink = Option.MessageContent.Substring(startLink, endLink - startLink);
                        Option.MessageContent = Option.MessageContent.FormatForMobileNotification();
                    }
                    catch
                    {

                    }
                }
            }
        }

        protected static string IsValidReminderType(int i)
        {
            return Enum.IsDefined(typeof(ReminderTypeEnum), i) ? null : "Please select a reminder type";
        }

        protected static string IsValidTarget(int i)
        {
            return Enum.IsDefined(typeof(ReminderTargetEnum), i) ? null : "Please select a target for the reminder";
        }

        protected string IsValidNotificationInterval(TimeSpan ts)
        {
            return ts != TimeSpan.Zero ? null : "Please select a notification interval";
        }

        async Task FillTemplate()
        {
            Option.MessageContent = REMINDER_MESSAGES[SelectedStarterTemplate];
        }

        async void InsertLink()
        {
            if (!OptionLink.IsValidUrl())
            {
                var dialogParams = new DialogParameters
                {
                    { "Message", $"The URL supplied is not valid." }
                };

                await DialogService.Show<AlertDialog>("Validation failure", dialogParams).Result;
                return;
            }

            Option.MessageContent += ' ';
            Option.MessageContent += OptionLink;
            Option.MessageContent += ' ';

            HideLink = true;
            StateHasChanged();


            ReminderTypeSelect?.ForceRender(false);
        }

        private void ReminderTypeChanged(IEnumerable<int> selectedValue)
        {
            Option.NotificationPeriod = TimeSpan.Zero;
        }
    }
}