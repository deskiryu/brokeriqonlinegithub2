using BrokerIQ.Dto.Enum;
using System.ComponentModel.DataAnnotations;

namespace BrokerIQ.Online.Models
{
    public class BrokerStaff
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "The broker staff email must be 50 letters or less")]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        [MaxLength(20, ErrorMessage = "The broker staff name must be 20 letters or less")]
        [RegularExpression("^[-a-zA-Z0-9(&)' - .-.]*$", ErrorMessage = "Name contains disallowed characters")]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(30, ErrorMessage = "The broker staff name must be 30 letters or less")]
        [RegularExpression("^[-a-zA-Z0-9(&)' - .-.]*$", ErrorMessage = "Name contains disallowed characters")]
        public string LastName { get; set; }

        [MaxLength(15, ErrorMessage = "The phone number must be 15 letters or less")]
        [Required(ErrorMessage = "Mobile no. is required in format +4407xxxxxxxxx")]
        [RegularExpression("^((\\+447)) ?\\d{9}$", ErrorMessage = "Please enter valid phone no in format +447xxxxxxxx")]
        public string TwoFactorPhoneNumber { get; set; }

        public TwoFactorEnum TwoFactorType { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public StaffTypeEnum StaffTypeId { get; set; }

        public EmailNotificationPreferencesEnum EmailNotificationPreferences { get; set; }

        public MobileNotificationPreferencesEnum MobileNotificationPreferences { get; set; }

        public int BrokerId { get; set; }

        public string FullName { get => FirstName + ' ' + LastName; }
    }
}
