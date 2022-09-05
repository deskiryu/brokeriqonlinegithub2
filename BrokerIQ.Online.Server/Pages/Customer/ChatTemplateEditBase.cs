using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Pages
{
    using BrokerIQ.Dto.Enum;
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

    public class ChatTemplateEditBase : ComponentBase
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
            BrokerDefinedMessage brokerDefinedMessages = await BrokerDefinedMessageService.Get();
            BrokerDefinedMessages = new List<MessageElement>();
            int index = (int) BrokerDefinedMessageEnum.FirstLoginMessage;
            foreach (var message in brokerDefinedMessages.BrokerDefinedMessages)
            {
                BrokerDefinedMessages.Add(new MessageElement(index, message.BrokerDefinedMessage));
                index++;
            }
            StateHasChanged();
        }
    }
}
