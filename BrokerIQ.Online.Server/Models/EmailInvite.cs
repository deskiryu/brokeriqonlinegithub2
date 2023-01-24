using System;

namespace BrokerIQ.Online.Models
{
    public class EmailInvite
    {
        public int Id { get; set; }

        public int BrokerId { get; set; }

        public string EmailAddress { get; set; }

        public int? BrokerStaffId { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool Converted { get; set; }

        public int InvitationCount { get; set; }

        public string BrokerStaffName { get; set; }

        public string BrokerName { get; set; }

    }
}
