using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Models
{
    using System.ComponentModel.DataAnnotations;
    using Dto.Enum;
    using Models;

    public class MortgageDocument
    {
        [Key]
        public Guid Id { get; set; }

        public int MortgageId { get; set; }

        public DocumentTypeEnum SupportingDocumentType { get; set; }

        public string FileName { get; set; }

        public byte[] File { get; set; }
    }
}
