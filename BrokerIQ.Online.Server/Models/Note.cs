using System;
using System.ComponentModel.DataAnnotations;

namespace BrokerIQ.Online.Models
{
    public class Note
    {
        [Key]
        public int Id { get; set; }

        public string Message { get; set; }

        public int BrokerId { get; set; }

        public int CustomerId { get; set; }

        public DateTime DateTaken { get; set; }

        public string CreatedByName { get; set; }
    }
}
