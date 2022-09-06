using System.Collections.Generic;
using BrokerIQ.Dto.Models;

namespace BrokerIQ.Online.Server.Models
{
    public class BrokerDefinedMessage
    {
        public int BrokerId { get; set; }

        public List<DefinedMessagesDto> BrokerDefinedMessages { get; set; }
    }
}