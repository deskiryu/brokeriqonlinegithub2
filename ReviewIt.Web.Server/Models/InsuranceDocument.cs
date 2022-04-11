using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReviewIt.Web.Models
{
    using System.ComponentModel.DataAnnotations;
    using Dto.Enum;
    using Models;

    public class InsuranceDocument
    {
        [Key]
        public Guid Id { get; set; }

        public int InsuranceId { get; set; }

        public DocumentTypeEnum SupportingDocumentType { get; set; }

        public string FileName { get; set; }

        public byte[] File { get; set; }
    }
}
