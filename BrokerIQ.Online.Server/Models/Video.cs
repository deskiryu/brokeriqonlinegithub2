using System;
using System.Collections.Generic;
using System.Text;

namespace BrokerIQ.Online.Server.Models
{
    public class Video
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public DateTimeOffset? UploadDate { get; set; }

        public DateTime? SendDate { get; set; }

        public bool Vetted { get; set; }

        public bool BirthdayVideo { get; set; }

        public bool SendDateTick { get; set; }

        public int BrokerId { get; set; }

        public string Broker { get; set; }
    }
}