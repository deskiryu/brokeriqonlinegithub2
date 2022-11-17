using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Server.Models
{
    public class BrokerIdentifier
    {
        public bool IdentifierFound { get; set; }

        public string BundleIdentifier { get; set; }

        public string BackgroundColour { get; set; }

        public string TextColour { get; set; }

        public string HttpLink { get; set; }

        public string HttpAddress { get; set; }

        public string AppName { get; set; }

        public bool Invert { get; set; }

        public string LinkColour { get; set; }

        public string AppStoreLink { get; set; }

        public string PlayStoreLink { get; set; }

        public string FromEmailName { get; set; }

        public string FromEmailAddress { get; set; }

        public string WelcomeVideoUrl { get; set; }
    }
}
