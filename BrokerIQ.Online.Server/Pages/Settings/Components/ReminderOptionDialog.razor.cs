using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using BrokerIQ.Dto.Dto;
using BrokerIQ.Dto.Enum;
using BrokerIQ.Online.Server.Extensions;
using BrokerIQ.Online.Server.Shared;

using MudBlazor;

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

        MudForm form;

        MudSelect<int> ReminderTypeId;
        MudSelect<TimeSpan> Notification;
        MudSelect<int> ReminderTargetId;

        private string OptionLink { get; set; }

        public int LinkStart;

        public int LinkEnd;

        private bool HideLink { get; set; }

        private TimeSpan FromDays270 = TimeSpan.FromDays(270);
        private TimeSpan FromDays240 = TimeSpan.FromDays(240);
        private TimeSpan FromDays210 = TimeSpan.FromDays(210);
        private TimeSpan FromDays180 = TimeSpan.FromDays(180);
        private TimeSpan FromDays150 = TimeSpan.FromDays(150);
        private TimeSpan FromDays120 = TimeSpan.FromDays(120);
        private TimeSpan FromDays90 = TimeSpan.FromDays(90);
        private TimeSpan FromDays60 = TimeSpan.FromDays(60);
        private TimeSpan FromDays30 = TimeSpan.FromDays(30);
        private TimeSpan FromDays15 = TimeSpan.FromDays(15);
        private TimeSpan FromDays7 = TimeSpan.FromDays(7);
        private TimeSpan FromDays6 = TimeSpan.FromDays(6);
        private TimeSpan FromDays5 = TimeSpan.FromDays(5);
        private TimeSpan FromDays4 = TimeSpan.FromDays(4);
        private TimeSpan FromDays3 = TimeSpan.FromDays(3);
        private TimeSpan FromDays2 = TimeSpan.FromDays(2);
        private TimeSpan FromDays1 = TimeSpan.FromDays(1);

        static string[] REMINDER_MESSAGES = new string[] {
            "",
            "The INSERT_INSURANCE_NAME insurance policy of your client INSERT_CLIENT_NAME ends on INSERT_DATE.",
            "Hi INSERT_CLIENT_NAME, your INSERT_INSURANCE_NAME insurance policy is due for renewal on INSERT_DATE.Contact your Broker for a new quote and prevent your policy being automatically renewed.",
            "Hi INSERT_CLIENT_NAME, your INSERT_INSURANCE_NAME insurance policy is due for renewal on INSERT_DATE.Contact your Broker for a new quote.",
            "The mortgage promotional period of your client INSERT_CLIENT_NAME ends on INSERT_DATE.",
            "Hi INSERT_CLIENT_NAME, your mortgage promotional period ends on INSERT_DATE. Contact your Broker to discuss your mortgage options.",
        };

        int SelectedStarterTemplate;

        Func<int, string> promptText = i => REMINDER_MESSAGES[i];

        void Submit()
        {
            form.Validate();

            if (LinkStart > 0)
            {
                Option.MessageContent = Option.MessageContent.Insert(LinkEnd, "-->");
                Option.MessageContent = Option.MessageContent.Insert(LinkStart, "<--");
            }

            if (form.IsValid) MudDialog.Close(DialogResult.Ok(Option));
        }

        void Cancel() => MudDialog.Cancel();

        protected override async Task OnInitializedAsync()
        {
            HideLink = Option.MessageContent.Contains("<--") && Option.MessageContent.Contains("-->");

            if (HideLink)
            {
                var linkStart = Option.MessageContent.IndexOf("<--") + 3;
                var linkEnd = Option.MessageContent.IndexOf("-->");
                OptionLink = Option.MessageContent[linkStart..linkEnd];
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

        protected static string IsValidNotificationInterval(TimeSpan ts)
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
            LinkStart = Option.MessageContent.Length;
            Option.MessageContent += OptionLink;
            LinkEnd = Option.MessageContent.Length;
            Option.MessageContent += ' ';

            HideLink = true;
            StateHasChanged();
        }
    }
}