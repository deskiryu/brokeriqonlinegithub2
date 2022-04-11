using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;

namespace ReviewIt.Web.Models
{
    public class Note
    {
        [Key]
        public int Id { get; set; }
        public string Message { get; set; }
        public int BrokerId { get; set; }
        public int CustomerId { get; set; }
        public DateTime DateTaken { get; set; }

    }
}
