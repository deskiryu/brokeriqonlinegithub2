using BrokerIQ.Dto.Enum;

namespace BrokerIQ.Online.Server.Models
{
    public class CustomerFilter
    {
        public int BrokerId { get; set; }
        public int AssignedToId { get; set; }
        public int Recent { get; set; }
        public int Period { get; set; }
        public int Category { get; set; }
        public int AgeRange { get; set; }
        public bool ProfilePictures { get; set; }
        public ProfilingOptionEnum? ProfilingOption { get; set; }
    }
}