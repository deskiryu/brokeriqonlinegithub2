using System.Collections.Generic;
using BrokerIQ.Dto;

namespace BrokerIQ.Online.Server.Models
{
    public class DocumentsRequirement
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int BrokerId { get; set; }
        public bool Satisfied { get; set; }
        public virtual ICollection<DocumentsCheckDto> DocumentChecks { get; set; }
    }
}