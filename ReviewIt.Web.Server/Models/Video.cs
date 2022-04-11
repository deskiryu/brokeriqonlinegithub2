using System;
using System.Collections.Generic;
using System.Text;

namespace ReviewIt.Web.Server.Models
{
    public class Video
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public bool Vetted { get; set; }

        public bool BirthdayVideo { get; set; }
    }
}
