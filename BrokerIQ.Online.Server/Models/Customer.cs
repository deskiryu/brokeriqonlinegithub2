using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Models
{
    using BrokerIQ.Dto.Enum;
    using System.ComponentModel.DataAnnotations;
    using BrokerIQ.Online.Attributes;
    using BrokerIQ.Dto.Dto;

    public class Customer
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(35)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(35)]
        public string LastName { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [MinLength(10)]
        [MaxLength(15)]
        [Phone]
        public string TelephoneNumber { get; set; }

        public string EmailAddress { get; set; }

        public string Salutation { get; set; }

        [Required]
        [MaxLength(200)]
        public string Address { get; set; }

        public EmploymentEnum Employment { get; set; }

        public NationalityEnum Nationality { get; set; }

        public ResidentialStatusEnum ResidentialStatus { get; set; }

        public OperatingSystemEnum OperatingSystem { get; set; }

        public string LatestVideoUrl { get; set; }

        public int ChosenBrokerId { get; set; }

        public string PotentialBroker1 { get; set; }

        public string PotentialBroker2 { get; set; }

        public string PotentialBroker3 { get; set; }

        public bool BrokerConfirmed { get; set; }

        public bool EmailConfirmed { get; set; }

        [MaxLength(200)]
        public string BusinessName { get; set; }

        public string AppVersion { get; set; }

        public bool IsWhiteLabel { get; set; }

        public bool VideoNotificationsAllowed { get; set; }

        public bool HasNeeds { get; set; }

        public virtual ICollection<Insurance> Insurances { get; set; }

        public virtual ICollection<Mortgage> Mortgages { get; set; }

        public virtual ICollection<MenuPlan> MenuPlans { get; set; }

        public virtual ICollection<int> ConnectedBrokers { get; set; }

        public virtual ICollection<CustomerDocument> CustomerDocuments { get; set; }

        public virtual CustomerDocument ProfilePicture { get; set; }

        public CustomerCategoryEnum CustomerCategory { get; set; }

        public bool Selected { get; set; }

        public string Name => Salutation + ' ' + FirstName + ' ' + LastName;

        public string MultiSelect => FirstName + LastName + EmailAddress + TelephoneNumber + DateOfBirth.ToString();

        public string PPAsImage
        {
            get
            {
                if (ProfilePicture != null && ProfilePicture.File.Any())
                {
                    var base64 = Convert.ToBase64String(ProfilePicture.File);
                    var imgSrc = String.Format("data:image/gif;base64,{0}", base64);
                    return imgSrc;
                }
                return null;
            }
        }

        public bool? IsSmokerOrVaper { get; set; }

        public int? AnnualIncome { get; set; }

        public GenderEnum Gender { get; set; }

        public int OccupationId { get; set; }
    }
}
