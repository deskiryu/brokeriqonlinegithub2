using System;
using System.Collections.Generic;
using System.Linq;

namespace BrokerIQ.Online.Models
{
    public class ChatDraftMessage
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public bool YahTheme { get; set; }
        public DateTime? ToBeSentOn { get; set; }
        public byte[] Image { get; set; }
        public int ChatId { get; set; }
        public string Style { get => YahTheme ? "chat_broker chat_broker_yah" : "chat_broker"; }
        public bool IsVideo { get; set; }
        public string VideoUrl { get; set; }
        public bool IsAudio { get; set; }
        public string AudioUrl { get; set; }
        public bool HasEmbeddedUrl { get; set; }
        public virtual IEnumerable<ChatDocument> ChatDocuments { get; set; }
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
                        if (myStrings.Count == 2)
                        {
                            myStrings.Add("");
                        }
                        if (myStrings.Count == 3)
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
