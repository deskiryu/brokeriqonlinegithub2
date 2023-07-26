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
        public int Id { get; set; }

        public int ChatId { get; set; }

        public int ChatMessageId { get; set; }

        public string FileName { get; set; }

        public byte[] File { get; set; }

        public DateTime CreatedDate { get; set; }

        public DocumentTypeEnum SupportingDocumentType { get; set; }

        public string ChatDocAsImage
        {
            get
            {
                if (SupportingDocumentType == DocumentTypeEnum.JPEG || SupportingDocumentType == DocumentTypeEnum.PNG)
                {
                    var base64 = Convert.ToBase64String(File);
                    var imgSrc = String.Format("data:image/gif;base64,{0}", base64);
                    return imgSrc;
                }
                return null;
            }
        }
    }
}
