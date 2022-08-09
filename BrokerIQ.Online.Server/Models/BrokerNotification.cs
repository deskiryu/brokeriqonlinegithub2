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
                var position = Message.IndexOf("Your", StringComparison.OrdinalIgnoreCase);
                var position2 = Message.IndexOf("client", StringComparison.OrdinalIgnoreCase);

                if(position>=0 && position2 == position + 5)
                {
                    try
                    {
                        myStrings.Add(Message.Substring(0, position+12));
                        var customerNameStart = Message.Substring(position + 12, Message.Length - position - 12);
                        string[] words = customerNameStart.Split(' ');
                        if(words.Length>=2)
                        {
                            myStrings.Add(words[0] + ' ' + words[1] + ' ');
                        }

                        var bigEnd = string.Empty;
                        for (int i = 2; i < words.Length; i++)
                        {
                            bigEnd += words[i] + ' ';
                        }
                        myStrings.Add(bigEnd);
                    }
                    catch
                    {
                        myStrings.Clear();
                        myStrings.Add(Message);
                    }


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
