using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Pages
{
    using System;
    using System.Linq;
    using BrokerIQ.Dto.Enum;
    using BrokerIQ.Online.Server.Extensions;
    using BrokerIQ.Online.Server.Models;
    using Microsoft.AspNetCore.Components;
    using Services.Interface;

    public class MessageElement
    {
        public MessageElement(int index, string message)
        {
            Index = index;
            Message = message;
        }

        public int Index { get; set; }
        public string Message { get; set; }
    }

    public class TemplateEditBase : ComponentBase
    {
        [Inject]
        public IBrokerDefinedMessageService BrokerDefinedMessageService { get; set; }

        public List<MessageElement> BrokerDefinedMessages = new List<MessageElement>();

        protected override async Task OnInitializedAsync()
        {
            await PopulateBrokerDefinedMessages();
        }

        protected async Task PopulateBrokerDefinedMessages()
        {
            BrokerDefinedMessage brokerDefinedMessage = await BrokerDefinedMessageService.Get();
            BrokerDefinedMessages = new List<MessageElement>();

            foreach (BrokerDefinedMessageEnum enumVal in Enum.GetValues(typeof(BrokerDefinedMessageEnum)))
            {
                string message = String.Empty;
                foreach (var item in brokerDefinedMessage.BrokerDefinedMessages.Where(
                    b => b.BrokerDefinedMessageEnumValue == enumVal && !b.BrokerDefinedMessage.Equals(enumVal.GetDisplayName())))
                {
                    message = item.BrokerDefinedMessage;
                    break;
                }

                if (message == String.Empty)
                {
                    // display default
                    BrokerDefinedMessages.Add(new MessageElement((int)enumVal, enumVal.GetDisplayName()));
                }
                else
                {
                    // display broker defined message
                    BrokerDefinedMessages.Add(new MessageElement((int)enumVal, message));
                }
            }

            StateHasChanged();
        }
    }
}
