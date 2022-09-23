using System;

namespace BrokerIQ.Online.Models
{
    public class TelephoneInvite
    {
        public int Id { get; set; }

        public int BrokerId { get; set; }

        public string TelephoneNumber { get; set; }

        public int InvitationCountTelephone { get; set; }

        public string CustomerName { get; set; }

        public int? BrokerStaffId { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool Converted { get; set; }

        public string BrokerStaffName { get; set; }

        public string BrokerName { get; set; }

        public bool Selected { get; set; }
    }
}
