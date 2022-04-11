using System;
using System.Collections.Generic;
using System.Text;

namespace ReviewIt.Web.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public bool BrokerSource { get; set; }
        public DateTime SentTime { get; set; }
        public byte[] Image { get; set; }
        public int ChatId { get; set; }
        public string Style { get => BrokerSource ? "chat_broker" : "chat_client"; }
    }
}
