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

        public string RowStyle
        {
            get
            {
                return Read ? "" : "font-weight:bold";
            }
        }
    }
}
