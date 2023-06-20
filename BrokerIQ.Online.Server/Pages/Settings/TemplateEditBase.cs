using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Pages
{
    using System;
    using System.IO;
    using System.Linq;
    using BrokerIQ.Dto.Enum;
    using BrokerIQ.Dto.Models;
    using BrokerIQ.Online.Server.AppSettings;
    using BrokerIQ.Online.Server.Extensions;
    using BrokerIQ.Online.Server.Models;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;
    using Microsoft.Extensions.Options;
    using Services.Interface;

    public class MessageElement
    {
        public int Index { get; set; }
        public string Message { get; set; }
        public string Prompt { get; set; }
        public string FileName { get; set; }

        public MessageElement GetCopy()
        {
            return new MessageElement()
            {
                Index = this.Index,
                Message = this.Message,
                Prompt = this.Prompt,
                FileName = this.FileName
            };
        }
    }

    public class TemplateEditBase : ComponentBase
    {
        [Inject]
        public IBrokerDefinedMessageService BrokerDefinedMessageService { get; set; }

        [Inject]
        public IOptions<FileUploadSettings> FileUploadSettingsOption { get; set; }

        protected FileUploadSettings fileUploadSettings { get; set; }

        public List<MessageElement> MessageElements = new List<MessageElement>();

        protected List<IBrowserFile> SelectedFiles = new();

        protected bool IsCurrentFileToBeRemoved = false;

        protected override async Task OnInitializedAsync()
        {
            await PopulateBrokerDefinedMessages();

            fileUploadSettings = this.FileUploadSettingsOption.Value;
        }

        protected async Task PopulateBrokerDefinedMessages()
        {
            BrokerDefinedMessage definedMessages = await BrokerDefinedMessageService.Get();
            MessageElements = new List<MessageElement>(Enum.GetValues(typeof(BrokerDefinedMessageEnum)).GetLength(0));

            foreach (BrokerDefinedMessageEnum enumVal in Enum.GetValues(typeof(BrokerDefinedMessageEnum)))
            {
                var message = definedMessages.BrokerDefinedMessages.FirstOrDefault(m => m.BrokerDefinedMessageEnumValue == enumVal);

                var element = new MessageElement()
                {
                    Index = (int)enumVal,
                    Message = message == null ? enumVal.GetDisplayName() : message.BrokerDefinedMessage,
                    Prompt = enumVal.GetDisplayPrompt(),
                    FileName = message?.FileName
                };

                MessageElements.Add(element);
            }

            StateHasChanged();
        }

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

        protected async void CommitMessage(object element)
        {
            // update defined messages in database
            List<DefinedMessagesDto> definedMessages = new List<DefinedMessagesDto>();

            var message = new DefinedMessagesDto
            {
                BrokerDefinedMessageEnumValue = (BrokerDefinedMessageEnum)((MessageElement)element).Index,
                BrokerDefinedMessage = ((MessageElement)element).Message,
            };

            var uploadedFile = SelectedFiles.FirstOrDefault();

            if (uploadedFile != null)
            {
                message.FileName = uploadedFile.Name;

                var contents = new MemoryStream(); ;
                await uploadedFile.OpenReadStream(fileUploadSettings.MaxFileSize).CopyToAsync(contents);
                message.File = contents.ToArray();
            }

            definedMessages.Add(message);
            await BrokerDefinedMessageService.UpdateOrCreate(definedMessages);
            await PopulateBrokerDefinedMessages();

            SelectedFiles.Clear();
            IsCurrentFileToBeRemoved = false;
        }
    }
}
