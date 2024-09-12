using System;
using System.Collections.Generic;
using System.Text;

namespace BrokerIQ.Online.Server.Models
{
    public class Audio
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string GuidId { get; set; }

        public string Url { get; set; }

        public DateTime UploadDate { get; set; }

        public int BrokerId { get; set; }

        public bool Vetted { get; set; }
        
        public string BrokerName { get; set; }
    }
}
