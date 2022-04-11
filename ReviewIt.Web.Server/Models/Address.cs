using System;
using System.Collections.Generic;
using System.Text;

namespace ReviewIt.Web.Models
{
    public class Address
    {
        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string City { get; set; }

        public string Postcode { get; set; }

        public string SummaryLine { get; set; }
    }
}
