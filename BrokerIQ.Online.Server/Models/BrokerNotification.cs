using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Models
{
    public class BrokerNotification
    {
        public int Id { get; set; }

        public string Message { get; set; }

        public int BrokerId { get; set; }

        public bool Read { get; set; }

        public DateTime SentDate { get; set; }

        public bool IsChat { get; set; }
        public int RelevantCustomerId { get; set; }
        public bool SendBrokerNotificationToPhone { get; set; }
        public bool SendBrokerNotificationToStaffPhone { get; set; }

        public string RowStyle
        {
            get
            {
                return Read ? "" : "font-weight:bold";
            }
        }

        public List<string> FormattedLinkMessage
        {
            get
            {
                var myStrings = new List<string>();
                string[] words = Message.Split(' ');
                if(words.Length > 4 && words[0]=="Your" && words[1]=="client")
                {
                    myStrings.Add(words[0]+' '+words[1] + ' ');
                    myStrings.Add(words[2] + ' ' + words[3] + ' ');
                    var bigEnd = string.Empty;
                    for(int i = 4; i < words.Length; i++)
                    {
                        bigEnd += words[i] + ' ';
                    }
                    myStrings.Add(bigEnd);
                }
                else
                {
                    myStrings.Add(Message);
                }

                return myStrings;
            }
        }
    }
}
