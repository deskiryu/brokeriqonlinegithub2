using System;
using BrokerIQ.Dto.Dto;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Settings.Components
{
    public partial class ReminderOptionDialog
    {
        [Microsoft.AspNetCore.Components.CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Microsoft.AspNetCore.Components.Parameter]
        public ReminderOptionDto Option { get; set; }

        MudForm form;

        MudSelect<int> ReminderTypeId;
        MudSelect<TimeSpan> Notification;
        MudSelect<int> ReminderTargetId;

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

        void Submit()
        {
            if (form.IsValid) MudDialog.Close(DialogResult.Ok(Option));
        }

        void Cancel() => MudDialog.Cancel();
    }
}