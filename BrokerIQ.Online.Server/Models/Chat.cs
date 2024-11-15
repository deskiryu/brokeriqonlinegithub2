using System.Collections.Generic;

namespace BrokerIQ.Online.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public int BrokerId { get; set; }
        public int CustomerId { get; set; }
        public virtual ICollection<ChatMessage> Messages { get; set; }
        public bool MoreMessagesAvailable { get; set; }
        public virtual ICollection<ChatDraftMessage> DraftMessages { get; set; }
    }
}
