using System.ComponentModel.DataAnnotations;

namespace BrokerIQ.Online.Models.Account
{
    public class AddStaff
    {
        [Required]
        [MaxLength(50, ErrorMessage = "The employee email address must be 50 letters or less")]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        [MaxLength(20, ErrorMessage = "The employee name must be 20 letters or less")]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(30, ErrorMessage = "The employee name must be 30 letters or less")]
        public string LastName { get; set; }

        [Required]
        [MaxLength(15, ErrorMessage = "The phone number must be 15 letters or less")]
        [Phone]
        public string TwoFactorPhoneNumber { get; set; }

        [Required]
        [MinLength(8, ErrorMessage = "The Password field must be a minimum of 8 characters")]
        [MaxLength(16, ErrorMessage = "The Password field must be a maximum of 16 characters")]
        [RegularExpression(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9]).{8,}$", ErrorMessage = "Passwords should contain at least one capital letter, one lowercase letter and one number")]
        public string Password { get; set; }

        [CompareProperty("Password")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}