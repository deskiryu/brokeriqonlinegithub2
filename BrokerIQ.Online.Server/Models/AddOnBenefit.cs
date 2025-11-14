using System.ComponentModel.DataAnnotations;

namespace BrokerIQ.Online.Models
{
    public class AddOnBenefit
    {
        public int Id { get; set; }

        public int InsuranceId { get; set; }

        [StringLength(100, ErrorMessage = "Title is too long.")]
        public string Title { get; set; }

        [StringLength(1000, ErrorMessage = "Description is too long.")]
        public string Description { get; set; }
    }
}
