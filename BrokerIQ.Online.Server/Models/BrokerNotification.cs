using System;
using System.Collections.Generic;

namespace BrokerIQ.Online.Models
{
    public class BrokerNotification
    {
        private const string CLIENT_MARKER = "your client ";
        private const string LINK_START_INDICATOR = "<--";
        private const string LINK_END_INDICATOR = "-->";

        public int Id { get; set; }

        public string Message { get; set; }

        public int BrokerId { get; set; }

        public bool Read { get; set; }

        public DateTime SentDate { get; set; }

        public bool IsChat { get; set; }

        public int RelevantCustomerId { get; set; }

        public bool SendBrokerNotificationToPhone { get; set; }

        public bool SendBrokerNotificationToStaffPhone { get; set; }

        public bool IsReferral { get; set; }

        public int RelevantReferralId { get; set; }

        public string RowStyle
        {
            get
            {
                return Read ? "" : "font-weight:bold";
            }
        }

        public bool HasLink => IsChat || IsReferral || Message.Contains(CLIENT_MARKER, StringComparison.OrdinalIgnoreCase) || HasEmbeddedLink;

        public bool HasEmbeddedLink => Message.Contains(LINK_START_INDICATOR) && Message.Contains(LINK_END_INDICATOR);


        public List<string> FormattedLinkMessage
        {
            get
            {
                if (IsChat || Message.Contains(CLIENT_MARKER, StringComparison.OrdinalIgnoreCase)) return FormatCustomerMessage();

                if (IsReferral) return FormatReferralMessage();

                if (HasEmbeddedLink) return FormatEmbeddedLink();

                return new List<string>() { Message };
            }
        }

        private List<string> FormatCustomerMessage()
        {
            var markerTextIndex = Message.IndexOf(CLIENT_MARKER, StringComparison.OrdinalIgnoreCase);

            if (markerTextIndex < 0) return new List<string>() { Message };

            var nameStartIndex = markerTextIndex + CLIENT_MARKER.Length;
            var nameEndIndex = Message.IndexOf(" ", nameStartIndex);
            nameEndIndex = Message.IndexOf(" ", nameEndIndex + 1);

            return new List<string>() {
                Message[..nameStartIndex],
                Message[nameStartIndex .. nameEndIndex],
                Message[nameEndIndex..]
            };
        }

        private List<string> FormatReferralMessage()
        {
            var linkText = "customer referral";
            var linkTextIndex = Message.IndexOf(linkText, StringComparison.OrdinalIgnoreCase);

            if (linkTextIndex < 0) return new List<string>() { Message };

            return new List<string>() {
                Message[..linkTextIndex],
                Message[linkTextIndex .. (linkTextIndex + linkText.Length)],
                Message[(linkTextIndex + linkText.Length)..]
            };
        }

        private List<string> FormatEmbeddedLink()
        {
            var linkStart = Message.IndexOf(LINK_START_INDICATOR);
            var linkEnd = Message.IndexOf(LINK_END_INDICATOR);
            var ulrAddress = Message[(linkStart + LINK_START_INDICATOR.Length)..linkEnd];

            return new List<string>() {
                Message[..linkStart],
                ulrAddress,
                Message[(linkEnd + LINK_END_INDICATOR.Length)..]
            };
        }
    }
}
