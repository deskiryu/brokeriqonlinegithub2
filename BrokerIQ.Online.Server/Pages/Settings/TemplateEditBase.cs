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
        public MessageElement(int index, string message, string prompt)
        {
            Index = index;
            Message = message;
            Prompt = prompt;
        }

        public int Index { get; set; }
        public string Message { get; set; }
        public string Prompt { get; set; }
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

                //Only 10 for now
                if (enumVal != BrokerDefinedMessageEnum.TickBoxMessage1 &&
                    enumVal != BrokerDefinedMessageEnum.TickBoxMessage2 &&
                    enumVal != BrokerDefinedMessageEnum.TickBoxMessage3 &&
                    enumVal != BrokerDefinedMessageEnum.TickBoxMessage4 &&
                    enumVal != BrokerDefinedMessageEnum.TickBoxMessage5 &&
                    enumVal != BrokerDefinedMessageEnum.TickBoxMessage6 &&
                    enumVal != BrokerDefinedMessageEnum.TickBoxMessage7 &&
                    enumVal != BrokerDefinedMessageEnum.TickBoxMessage8 &&
                    enumVal != BrokerDefinedMessageEnum.TickBoxMessage9 &&
                    enumVal != BrokerDefinedMessageEnum.TickBoxMessage10)
                {
                    continue;
                }

                foreach (var item in brokerDefinedMessage.BrokerDefinedMessages.Where(
                    b => b.BrokerDefinedMessageEnumValue == enumVal && !b.BrokerDefinedMessage.Equals(enumVal.GetDisplayName())))
                {
                    message = item.BrokerDefinedMessage;
                    
                    break;
                }

                if (message == String.Empty)
                {
                    // display default
                    BrokerDefinedMessages.Add(new MessageElement((int)enumVal, enumVal.GetDisplayName(), enumVal.GetDisplayPrompt()));
                }
                else
                {
                    // display broker defined message
                    BrokerDefinedMessages.Add(new MessageElement((int)enumVal, message, enumVal.GetDisplayPrompt()));
                }
            }

            StateHasChanged();
        }
    }
}
