using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Models
{
    public class BrokerDefinedMessage
    {
        public int Id { get; set; }

        public int BrokerId { get; set; }

        public string Prompt { get; set; }

        public string Message { get; set; }

        public string FileName { get; set; }

        public byte[] File { get; set; }

        public bool WelcomeChat { get; set; }

        public int SortOrder { get; set; }

        public string ConvertedMessage { get; set; }
    }
}
