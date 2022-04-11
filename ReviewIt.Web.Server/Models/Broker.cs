using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReviewIt.Web.Models
{
    using ReviewIt.Web.Server.Models;
    using System.ComponentModel.DataAnnotations;

    public class Broker
    {
        [Key]
        public int Id { get; set; }

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

        public byte[] LogoImage { get; set; }

        public string Identifier { get; set; }

        public bool Selected { get; set; }

        public virtual ICollection<BrokerStaff> BrokerStaff { get; set; }
    }
}
