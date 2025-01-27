using System;
using System.ComponentModel.DataAnnotations;
using BrokerIQ.Dto.Enum;

namespace BrokerIQ.Online.Models
{
    public class WealthDocument
    {
        [Key]
        public Guid Id { get; set; }

        public int WealthId { get; set; }

        public DocumentTypeEnum SupportingDocumentType { get; set; }

        public string FileName { get; set; }

        public byte[] File { get; set; }
    }
}
