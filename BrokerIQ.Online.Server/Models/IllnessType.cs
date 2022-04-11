using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Models
{
    using System.ComponentModel.DataAnnotations;

    public class IllnessType
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
