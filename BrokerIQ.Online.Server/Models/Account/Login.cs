using System.ComponentModel.DataAnnotations;

namespace BrokerIQ.Online.Models.Account
{
    public class Login
    {
        [Required]
        public string EmailAddress { get; set; }

        [Required]
        public string Password { get; set; }
    }
}