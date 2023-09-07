using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Server.Models
{
    public class CustomerFilter
    {
        public int BrokerId { get; set; }
        public int Recent { get; set; }
        public int Period { get; set; }
        public int Category { get; set; }
        public int AgeRange { get; set; }
        public bool ProfilePictures { get; set; }
        public bool HasNeeds { get; set; }
        public bool WithoutIncomeProtection { get; set; }
        public bool WithoutLifeInsurance { get; set; }
        public bool WithoutLifeCritical { get; set; }
    }
}