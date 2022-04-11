using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReviewIt.Web.Models
{
    using ReviewIt.Dto.Enum;
    using System.ComponentModel.DataAnnotations;

    public class MenuPlan
    {
        [Key]
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public int BrokerId { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Name is too long.")]
        public string Name { get; set; }

        public decimal Cost { get; set; }

        public bool Annual { get; set; }

        public DateTime? ReviewDate { get; set; }

        public bool ShowReviewDate { get; set; }

        public virtual ICollection<int> InsuranceIds { get; set; }
    }
}
