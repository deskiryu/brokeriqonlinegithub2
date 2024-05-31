using System;
using System.ComponentModel.DataAnnotations;

using BrokerIQ.Dto.Enum;

namespace BrokerIQ.Online.Models
{
    public class CustomerDocument
    {
        [Key]
        public Guid Id { get; set; }

        public int CustomerId { get; set; }

        public DocumentTypeEnum SupportingDocumentType { get; set; }

        public string FileName { get; set; }

        public byte[] File { get; set; }

        public int DocuVaultType { get; set; }

        public string Description { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool Selected { get; set; }

        public string TypeDesc { get => (SupportingDocumentType == DocumentTypeEnum.JPEG || SupportingDocumentType == DocumentTypeEnum.PNG) ? "Photo" : "Pdf"; }
    }
}
