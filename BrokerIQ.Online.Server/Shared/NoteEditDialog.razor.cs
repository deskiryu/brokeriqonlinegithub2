using System;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Shared
{
    public partial class NoteEditDialog
    {
        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public string Text { get; set; }

        [Parameter]
        public bool ShowReminderControls { get; set; } = true;

        [Parameter]
        public bool HasNoteReminder { get; set; }

        [Parameter]
        public DateTime? ReminderDate { get; set; }

        [Parameter]
        public TimeSpan? ReminderTime { get; set; }

        [Inject]
        protected IAlertService AlertService { get; set; }


        public bool DisableReminderDate => !HasNoteReminder;

        public bool TextIsEmpty => string.IsNullOrWhiteSpace(Text);

        private void Confirm()
        {
            DateTime combined = new DateTime();
            if (HasNoteReminder)
            {
                if(ReminderDate.HasValue && ReminderTime.HasValue)
                {
                    combined = ReminderDate.Value + ReminderTime.Value;
                }
                else
                {
                    this.AlertService.Error("You must choose a time and date");
                    return;
                }
            }
       

            MudDialog.Close(DialogResult.Ok(new NoteDetail()
            {
                Text = Text,
                ReminderDate = HasNoteReminder ? combined : null
            }));

            MudDialog.Close();
        }

        void Cancel() => MudDialog.Cancel();

        public class NoteDetail
        {
            public string Text { get; set; }

            public DateTime? ReminderDate { get; set; }
        }
    }
}

