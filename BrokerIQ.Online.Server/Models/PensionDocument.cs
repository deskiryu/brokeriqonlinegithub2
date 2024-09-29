using System;
using System.ComponentModel.DataAnnotations;
using BrokerIQ.Dto.Enum;

namespace BrokerIQ.Online.Models
{
    public class PensionDocument
    {
        [Key]
        public Guid Id { get; set; }

        public int PensionId { get; set; }

        public DocumentTypeEnum SupportingDocumentType { get; set; }

        public string FileName { get; set; }

        public byte[] File { get; set; }
    }
}
