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
        public string PartialName { get; set; }
        public bool NonAppUsersOnly { get; set; }
        public SortOrderEnum SortOrder { get; set; }
        public SortByEnum SortBy { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int LastMaxId { get; set; }
        public int LastMinId { get; set; }
        public PagingDirectionEnum PagingDirectionEnum { get; set; }

    }
}