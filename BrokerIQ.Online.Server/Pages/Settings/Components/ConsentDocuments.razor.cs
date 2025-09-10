using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Online.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using MudBlazor;
using BrokerIQ.Online.Server.AppSettings;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Server.Pages.Settings.Components
{
    public partial class ConsentDocuments
    {
        [Parameter]
        public User User { get; set; }

        [Parameter]
        public Online.Models.Broker Broker { get; set; }

        [Inject]
        private ISnackbar Snackbar { get; set; }

        [Inject]
        public IBrokerConsentDocumentService BrokerConsentDocumentService { get; set; }

        [Inject]
        public IOptions<FileUploadSettings> FileUploadSettingsOption { get; set; }

        protected FileUploadSettings FileUploadSettings { get; set; }

        protected MudForm form;

        protected class ConsentDocumentModel
        {
            public Dto.Enum.ConsentDocumentsEnum ConsentType { get; set; }
            public string Description { get; set; }
            public bool IsUrlConsent { get; set; }
            public string Url { get; set; }
            public int MajorVersion { get; set; } = 1;
            public int MinorVersion { get; set; } = 1;
        }

        protected ConsentDocumentModel Model { get; set; } = new ConsentDocumentModel();

        protected List<IBrowserFile> SelectedFiles = new();
        protected string HoverClass;
        protected string DropZoneClass => SelectedFiles.Any() ? "card" : "card";

        protected override void OnInitialized()
        {
            FileUploadSettings = FileUploadSettingsOption.Value;
        }

        protected void OnDragEnter(DragEventArgs e) => HoverClass = "drag-file-hover";
        protected void OnDragLeave(DragEventArgs e) => HoverClass = string.Empty;

        protected void LoadFiles(InputFileChangeEventArgs e)
        {
            SelectedFiles.Clear();

            try
            {
                var file = e.File;
                var ext = System.IO.Path.GetExtension(file.Name);
                if (!ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Only PDF files are allowed");
                }

                SelectedFiles.Add(file);
            }
            catch (Exception)
            {
                // swallow and keep empty selection
            }
        }

        protected void DeleteSelectedFile()
        {
            SelectedFiles.Clear();
        }

        protected async Task Submit()
        {
            await form.Validate();

            // Basic validation: consent type, version numbers, and either URL or file
            if (Model.IsUrlConsent)
            {
                if (string.IsNullOrWhiteSpace(Model.Url))
                {
                    Snackbar.Add("Please provide a valid URL", Severity.Warning);
                    return;
                }
                if (!Uri.TryCreate(Model.Url, UriKind.Absolute, out var _))
                {
                    Snackbar.Add("URL format is invalid", Severity.Warning);
                    return;
                }
            }
            else
            {
                if (!SelectedFiles.Any())
                {
                    Snackbar.Add("Please upload a PDF file", Severity.Warning);
                    return;
                }
            }

            // At this point we have a valid model; construct a preview summary.
            var summary = $"Created consent: {Model.ConsentType} v{Model.MajorVersion}.{Model.MinorVersion}" +
                          (Model.IsUrlConsent ? $" (URL: {Model.Url})" : $" (PDF: {SelectedFiles.FirstOrDefault()?.Name})");
            Snackbar.Add(summary, Severity.Success);

            // TODO: integrate with backend service to persist BrokerConsentDocumentDto
        }
    }
}
