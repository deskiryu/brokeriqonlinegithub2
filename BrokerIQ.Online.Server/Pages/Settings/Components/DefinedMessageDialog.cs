using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Server.AppSettings;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Settings.Components
{
    public partial class DefinedMessageDialog
    {
        [Microsoft.AspNetCore.Components.CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Microsoft.AspNetCore.Components.Parameter]
        public BrokerDefinedMessageDto Template { get; set; }

        [Inject]
        public IOptions<FileUploadSettings> FileUploadSettingsOption { get; set; }

        protected FileUploadSettings fileUploadSettings { get; set; }

        MudForm form;

        protected List<IBrowserFile> SelectedFiles = new();

        protected string HoverClass;

        protected void OnDragEnter(DragEventArgs e) => HoverClass = "drag-file-hover";

        protected void OnDragLeave(DragEventArgs e) => HoverClass = string.Empty;

        protected bool IsCurrentFileToBeRemoved = false;

        protected override async Task OnInitializedAsync()
        {
            fileUploadSettings = this.FileUploadSettingsOption.Value;
        }

        async Task Submit()
        {
            await form.Validate();

            if (form.IsValid)
            {
                Template.FileName = null;
                Template.File = null;

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
    }
}