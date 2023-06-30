using System;

namespace BrokerIQ.Online.Server.Models
{
    public class TrainingVideo
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public DateTimeOffset? UploadDate { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
    }
}
