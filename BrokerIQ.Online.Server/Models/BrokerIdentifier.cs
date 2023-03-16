using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Server.Models
{
    public class BrokerIdentifier
    {
        public int Id { get; set; }
        public int BrokerId { get; set; }
        public bool IdentifierFound { get; set; }
        [Required]
        public string BundleIdentifier { get; set; }
        [Required]
        public string BackgroundColour { get; set; }
        [Required]
        public string TextColour { get; set; }
        [Required]
        [Url]
        public string HttpLink { get; set; }
        [Required]
        public string HttpAddress { get; set; }
        [Required]
        public string AppName { get; set; }

        public bool Invert { get; set; }
        [Required]
        public string LinkColour { get; set; }
        [Required]
        [Url]
        public string AppStoreLink { get; set; }
        [Required]
        [Url]
        public string PlayStoreLink { get; set; }
        [Required]
        public string FromEmailName { get; set; }
        [Required]
        [EmailAddress]
        public string FromEmailAddress { get; set; }
        [Required]
        [Url]
        public string WelcomeVideoUrl { get; set; }
        [Required]
        public string HubClientConnectString { get; set; }
        [Required]
        public string HubClientName { get; set; }
        [Required]
        public string FirebaseKey { get; set; }
        [Required]
        public string FirebaseClient { get; set; }
    }
}
