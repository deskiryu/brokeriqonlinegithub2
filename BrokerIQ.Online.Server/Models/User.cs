namespace BrokerIQ.Online.Models
{
    public class User
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
        public bool IsDeleting { get; set; }
        public bool IsBroker { get; set; }
        public bool IsCustomer { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsBrokerStaff { get; set; }
        public int MasterBrokerId { get; set; }
        public int? StaffBrokerId { get; set; }
        public bool RequiresTwoFactor { get; set; }
    }
}