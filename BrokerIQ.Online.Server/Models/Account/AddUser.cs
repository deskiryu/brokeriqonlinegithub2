using BrokerIQ.Dto.Enum;
using System.ComponentModel.DataAnnotations;

namespace BrokerIQ.Online.Models.Account
{
    public class AddUser
    {
        [Required]
        [MaxLength(50, ErrorMessage = "The broker name must be 50 letters or less")]
        public string Name { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "The broker email must be 50 letters or less")]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        [MaxLength(20, ErrorMessage = "The broker name must be 20 letters or less")]
        public string BrokerFirstName { get; set; }

        [Required]
        [MaxLength(30, ErrorMessage = "The broker name must be 30 letters or less")]
        public string BrokerLastName { get; set; }

        [Required]
        [MaxLength(15, ErrorMessage = "The phone number must be 15 letters or less")]
        [Phone]
        public string TelephoneNumber { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "The address must be 50 letters or less")]
        public string AddressLine1 { get; set; }


        [MaxLength(50, ErrorMessage = "The address must be 50 letters or less")]
        public string AddressLine2 { get; set; }

        [MaxLength(50, ErrorMessage = "The address must be 50 letters or less")]
        public string AddressLine3 { get; set; }

        [Required]
        [MaxLength(10, ErrorMessage = "The postcode must be 10 letters or less")]
        [RegularExpression(@"([Gg][Ii][Rr] 0[Aa]{2})|((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9][A-Za-z]?))))\s?[0-9][A-Za-z]{2})", 
            ErrorMessage = "Please enter a valid uk postcode")]
        public string Postcode { get; set; }

        [Required]
        [MinLength(8, ErrorMessage = "The Password field must be a minimum of 8 characters")]
        [MaxLength(16, ErrorMessage = "The Password field must be a maximum of 16 characters")]
        [RegularExpression(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9]).{8,}$", ErrorMessage = "Passwords should contain at least one capital letter, one lowercase letter and one number")]
        public string Password { get; set; }

        public byte[] LogoImage { get; set; }

        [MaxLength(15, ErrorMessage = "The phone number must be 15 letters or less")]
        [Required(ErrorMessage = "Mobile no. is required in format +4407xxxxxxxxx")]
        [RegularExpression("^((\\+447)) ?\\d{9}$", ErrorMessage = "Please enter valid phone no in format +447xxxxxxxx")]
        public string TwoFactorPhoneNumber { get; set; }

        public bool TwoFactorUseBrokerPhoneNumber { get; set; }

        public TwoFactorEnum TwoFactorType { get; set; }

        [Compare("Password")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}