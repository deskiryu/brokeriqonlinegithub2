using System;
using BrokerIQ.Dto.Enum;

namespace BrokerIQ.Online.Server.Models
{
    public class BrokerSubscription
    {
        public int BrokerId { get; set; }

        public SubscriptionServiceEnum SubscriptionServiceId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}