using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrokerIQ.Online.Models
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
        public bool IsRead { get; set; }
        public bool IsVideo { get; set; }
        public string VideoUrl { get; set; }
        public bool HasEmbeddedUrl { get; set; }
        public int ChatDocumentId { get; set; }
        public virtual ChatDocument ChatDocument { get; set; }
        public List<string> FormattedLinkMessage
        {
            get
            {
                if (!string.IsNullOrEmpty(Message))
                {
                    string[] separatingStrings = { "<--", "-->" };
                    var myStrings = Message.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (myStrings != null && myStrings.Any())
                    {
                        if (myStrings.Count==2)
                        {
                            myStrings.Add("");
                        }
                        if(myStrings.Count==3)
                        {
                            return myStrings;
                        }
                    }
                }

                return null;
            }
        }
    }
}
