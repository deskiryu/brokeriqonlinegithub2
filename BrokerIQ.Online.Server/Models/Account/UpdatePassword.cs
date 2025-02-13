using System.ComponentModel.DataAnnotations;

namespace BrokerIQ.Online.Models.Account
{
    public class UpdatePassword
    {
        [Required]
        [Display(Name = "Existing Password")]
        public string ExistingPassword { get; set; }

        [Required]
        [MinLength(12, ErrorMessage = "The Password field must be a minimum of 12 characters")]
        [MaxLength(16, ErrorMessage = "The Password field must be a maximum of 16 characters")]
        [RegularExpression(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9]).{12,}$", ErrorMessage = "Passwords should contain at least one capital letter, one lowercase letter and one number")]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [Compare("NewPassword")]
        [Display(Name = "Confirm New Password")]
        public string NewConfirmPassword { get; set; }
    }

}