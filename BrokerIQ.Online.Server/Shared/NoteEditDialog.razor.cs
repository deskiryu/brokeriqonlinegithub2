using System;
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

        public bool DisableReminderDate => !HasNoteReminder;

        public bool TextIsEmpty => string.IsNullOrWhiteSpace(Text);

        private void Confirm()
        {
            MudDialog.Close(DialogResult.Ok(new NoteDetail()
            {
                Text = Text,
                ReminderDate = HasNoteReminder ? ReminderDate : null
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

