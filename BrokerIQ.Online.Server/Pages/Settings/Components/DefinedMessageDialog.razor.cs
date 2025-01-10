using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Server.AppSettings;
using BrokerIQ.Online.Server.Extensions;
using BrokerIQ.Online.Server.Shared;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Settings.Components
{
    public partial class DefinedMessageDialog
    {
        [Inject]
        public IDialogService DialogService { get; set; }

        [Inject]
        private ISnackbar Snackbar { get; set; }

        [Inject]
        public IBrokerDefinedMessageService BrokerDefinedMessageService { get; set; }

        [Inject]
        public IAlertService AlertService { get; set; }

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

        private bool HideLink { get; set; }

        protected override void OnInitialized()
        {
            fileUploadSettings = this.FileUploadSettingsOption.Value;

            HideLink = Template.Message.Contains(LINK_START_INDICATOR) && Template.Message.Contains(LINK_END_INDICATOR);

            if (HideLink)
            {
                HideLink = true;
                var startLink = Template.Message.IndexOf(LINK_START_INDICATOR) + 3;
                var endLink = Template.Message.IndexOf(LINK_END_INDICATOR);

                if (startLink > 0 && endLink > 0)
                {
                    try
                    {
                        TemplateLink = Template.Message.Substring(startLink, endLink - startLink);
                        Template.Message = Template.Message.FormatForMobileNotification();
                    }
                    catch
                    {

                    }
                }
            }
        }

        async Task Submit()
        {
            await form.Validate();

            //Find substring
            if (HideLink)
            {

                var found = Template.Message.IndexOf(TemplateLink);
                if (found > 0)
                {
                    Template.Message = Template.Message.Insert(found + TemplateLink.Length, LINK_END_INDICATOR);
                    Template.Message = Template.Message.Insert(found, LINK_START_INDICATOR);
                }
                else
                {
                    this.AlertService.Warn("The link has been altered and may not show in the reminder correctly");
                }
            }

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

                if (await SaveDefinedMessage())
                {
                    MudDialog.Close(DialogResult.Ok(true));
                }
            }
        }

        private async Task<bool> SaveDefinedMessage()
        {
            bool wasSuccessfull;

            if (Template.Id == 0)
            {
                var newTemplate = new CreateBrokerDefinedMessageDto()
                {
                    BrokerId = Template.BrokerId,
                    Prompt = Template.Prompt,
                    Message = Template.Message,
                    FileName = Template.FileName,
                    File = Template.File,
                    WelcomeChat = Template.WelcomeChat
                };

                wasSuccessfull = await BrokerDefinedMessageService.Create(newTemplate);
            }
            else
            {
                wasSuccessfull = await BrokerDefinedMessageService.Update(Template);
            }

            if (wasSuccessfull)
            {
                Snackbar.Add("Defined message saved successfully", Severity.Success);
            }
            else
            {
                Snackbar.Add("Unable to save defined message. Please try again.", Severity.Error);
            }

            return wasSuccessfull;
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

        void FillTemplate()
        {
            Template.Prompt = MESSAGE_PROMPTS[SelectedStarterTemplate];
            Template.Message = MESSAGE_TEMPLATES[SelectedStarterTemplate];
        }

        async void InsertLink()
        {

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
            Template.Message += TemplateLink;
            Template.Message += ' ';

            HideLink = true;
            StateHasChanged();
        }
    }
}