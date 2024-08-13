using BrokerIQ.Dto.Enum;
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
        [RegularExpression("^[-a-zA-Z0-9(&)' - .-.]*$", ErrorMessage = "Name contains disallowed characters")]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(30, ErrorMessage = "The employee name must be 30 letters or less")]
        [RegularExpression("^[-a-zA-Z0-9(&)' - .-.]*$", ErrorMessage = "Name contains disallowed characters")]
        public string LastName { get; set; }

        [MaxLength(15, ErrorMessage = "The phone number must be 15 letters or less")]
        [RegularExpression("^((\\+447)) ?\\d{9}$", ErrorMessage = "Please enter valid phone no in format +447xxxxxxxx")]
        public string TwoFactorPhoneNumber { get; set; }

        public TwoFactorEnum TwoFactorType { get; set; }

        [Required]
        [MinLength(12, ErrorMessage = "The Password field must be a minimum of 12 characters")]
        [MaxLength(16, ErrorMessage = "The Password field must be a maximum of 16 characters")]
        [RegularExpression(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9]).{8,}$", ErrorMessage = "Passwords should contain at least one capital letter, one lowercase letter and one number")]
        public string Password { get; set; }

        [Compare("Password")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        public StaffTypeEnum StaffTypeId { get; set; }

        public bool IsAdminStaff { get; set; }
    }
}