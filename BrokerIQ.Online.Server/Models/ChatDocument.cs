using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Models
{
    using System.ComponentModel.DataAnnotations;
    using Dto.Enum;
    using Models;

    public class ChatDocument
    {
        [Key]
        public Guid Id { get; set; }

        public int ChatId { get; set; }

        public int ChatMessageId { get; set; }

        public string FileName { get; set; }

        public byte[] File { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
