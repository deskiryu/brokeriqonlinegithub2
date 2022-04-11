using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReviewIt.Web.Models
{
    using System.ComponentModel.DataAnnotations;

    public class InsuranceType
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
