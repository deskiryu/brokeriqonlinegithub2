using System.Collections.Generic;
using BrokerIQ.Dto.Dto;

namespace BrokerIQ.Online.Server.Models
{
    public class BrokerReminderOption
    {
        public int BrokerId { get; set; }

        public List<ReminderOptionDto> BrokerReminderOptions { get; set; }
    }
}