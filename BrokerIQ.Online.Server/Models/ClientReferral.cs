using System;
using BrokerIQ.Dto.Enum;

namespace BrokerIQ.Online.Models
{
    public class ClientReferral
    {
        public int Id { get; set; }
        public string ReferralName { get; set; }
        public string ReferralEmailAddress { get; set; }
        public string ReferralTelephoneNumber { get; set; }
        public int CustomerId { get; set; }
        public int BrokerId { get; set; }
        public int? BrokerStaffId { get; set; }
        public string ReferralNote { get; set; }
        public bool ConvertedLoggedIn { get; set; }
        public ConvertedToProductEnum ConvertedToProduct { get; set; }
        public bool PrizeAwarded { get; set; }
        public string BrokerStaffName { get; set; }
        public string BrokerName { get; set; }
        public string CustomerName { get; set; }
        public DateTime? NoteReminderDate { get; set; }
    }
}
