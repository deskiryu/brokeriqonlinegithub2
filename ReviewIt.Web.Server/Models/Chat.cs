using System;
using System.Collections.Generic;
using System.Text;

namespace ReviewIt.Web.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public int BrokerId { get; set; }
        public int CustomerId { get; set; }
        public virtual ICollection<ChatMessage> Messages { get; set; }
    }
}
