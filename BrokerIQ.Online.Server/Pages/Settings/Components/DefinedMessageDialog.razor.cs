using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;

using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Server.Shared;
using BrokerIQ.Online.Server.AppSettings;

using MudBlazor;
using BrokerIQ.Online.Server.Extensions;

namespace BrokerIQ.Online.Server.Pages.Settings.Components
{
    public partial class DefinedMessageDialog
    {
        [Inject]
        public IDialogService DialogService { get; set; }

        [Microsoft.AspNetCore.Components.CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Microsoft.AspNetCore.Components.Parameter]
        public BrokerDefinedMessageDto Template { get; set; }

        [Inject]
        public IOptions<FileUploadSettings> FileUploadSettingsOption { get; set; }

        private const string LINK_START_INDICATOR = "<--";
        private const string LINK_END_INDICATOR = "-->";

        static string[] MESSAGE_PROMPTS = new string[] {
            "Empty",
            "Thank you for registering",
            "Please upload documents",
            "Decision in principle",
            "Mortgage offer",
            "Survey instructed",
            "Survey booked",
            "Survey came back",
            "Completion list date"
        };

        readonly string[] MESSAGE_TEMPLATES = new string[] {
            "",
            "Hi INSERT_CLIENT_NAME, Thank you for downloading our new App. All communications will be carried out on here going forward. Please keep an eye out for communications from us.",
            "Please can you now upload all of the requested documents in the uploads section of the App.",
            "Great News! Your Decision in Principle has been accepted. Please see a copy of your Decision in Principle attached. The next stage is to find the right property.Happy House Hunting!",
            "Hi INSERT_CLIENT_NAME, Congratulations! Your mortgage Offer has now been issued. I have attached my copy. A copy will be sent to you and also to your solicitor. You will find a copy of your mortgage offer in the Mortgage section of the app. Any questions please let me know.",
            "Hi INSERT_CLIENT_NAME, Just a quick update to let you know your survey has been instructed today. We will keep you updated on the progress.",
            "Hi INSERT_CLIENT_NAME, Your survey has been booked for INSERT_DATE we will keep you updated on the progress.",
            "Hi INSERT_CLIENT_NAME, Just a quick note to let you know your survey has come back and you application is now with the underwriters. We will keep you updated on the progress.",
            "Hi INSERT_CLIENT_NAME, Your completion list has been set for INSERT_DATE."
        };

        int SelectedStarterTemplate;

        Func<int, string> promptText = i => MESSAGE_PROMPTS[i];

        protected FileUploadSettings fileUploadSettings { get; set; }

        MudForm form;

        protected List<IBrowserFile> SelectedFiles = new();

        protected string HoverClass;

        protected void OnDragEnter(DragEventArgs e) => HoverClass = "drag-file-hover";

        protected void OnDragLeave(DragEventArgs e) => HoverClass = string.Empty;

        protected bool IsCurrentFileToBeRemoved = false;

        private string TemplateLink { get; set; }

        public int LinkStart;

        public int LinkEnd;

        private bool HideLink { get; set; }

        protected override async Task OnInitializedAsync()
        {
            fileUploadSettings = this.FileUploadSettingsOption.Value;

            HideLink = Template.Message.Contains(LINK_START_INDICATOR) && Template.Message.Contains(LINK_END_INDICATOR);

            if (HideLink)
            {
                var linkStart = Template.Message.IndexOf(LINK_START_INDICATOR) + 3;
                var linkEnd = Template.Message.IndexOf(LINK_END_INDICATOR);
                TemplateLink = Template.Message[linkStart..linkEnd];
            }
        }

        async Task Submit()
        {
            await form.Validate();

            if (form.IsValid)
            {
                if (IsCurrentFileToBeRemoved)
                {
                    Template.FileName = null;
                    Template.File = null;
                }

                var uploadedFile = SelectedFiles.FirstOrDefault();

                if (uploadedFile != null)
                {
                    var contents = new MemoryStream(); ;
                    await uploadedFile.OpenReadStream(fileUploadSettings.MaxFileSize).CopyToAsync(contents);

                    if (contents.Length > 0)
                    {
                        Template.FileName = uploadedFile.Name;
                        Template.File = contents.ToArray();
                    }
                }

                if (LinkStart > 0)
                {
                    Template.Message = Template.Message.Insert(LinkEnd, LINK_END_INDICATOR);
                    Template.Message = Template.Message.Insert(LinkStart, LINK_START_INDICATOR);
                }

                MudDialog.Close(DialogResult.Ok(Template));
            }
        }

        void Cancel() => MudDialog.Cancel();

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

        protected void DeleteDefinedDocument()
        {
            SelectedFiles.Clear();
        }

        protected void RemoveCurrentFile()
        {
            IsCurrentFileToBeRemoved = true;
        }

        async Task FillTemplate()
        {
            Template.Prompt = MESSAGE_PROMPTS[SelectedStarterTemplate];
            Template.Message = MESSAGE_TEMPLATES[SelectedStarterTemplate];
        }

        async void InsertLink()
        {
            if (!TemplateLink.ToLower().StartsWith("http"))
            {
                TemplateLink = "http://" + TemplateLink;
            }

            if (!TemplateLink.IsValidUrl())
            {
                var dialogParams = new DialogParameters
                {
                    { "Message", $"The URL supplied is not valid." }
                };

                await DialogService.Show<AlertDialog>("Validation failure", dialogParams).Result;
                return;
            }


            Template.Message += ' ';
            LinkStart = Template.Message.Length;
            Template.Message += TemplateLink;
            LinkEnd = Template.Message.Length;
            Template.Message += ' ';

            HideLink = true;
            StateHasChanged();
        }
    }
}