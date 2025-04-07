using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Models
{
    using BrokerIQ.Dto.Enum;
    using BrokerIQ.Online.Attributes;
    using System.ComponentModel.DataAnnotations;

    public class Mortgage
    {
        [Key]
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public int BrokerId { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Provider Name is too long.")]
        [RegularExpression("^[-a-zA-Z0-9(&)' - .-.]*$", ErrorMessage = "Name contains disallowed characters")]
        public string ProviderName { get; set; }

        [Required]
        public MortgageEnum MortgageType { get; set; }

        [Required]
        public MortgageRateEnum MortgageRateType { get; set; }

        public DateTime? StartDate { get; set; }

        [RequiredIf(nameof(EndDate), true, ErrorMessage = "Please enter a mortgage end date")]
        public DateTime? EndDate { get; set; }

        public DateTime LastNotificationCheck { get; set; }

        [RequiredIf(nameof(ShowMonthlyPayment), true, ErrorMessage = "Please enter a monthly payment")]
        [Range(0.01, float.MaxValue, ErrorMessage = "Please enter a monthly payment")]
        public decimal? MonthlyPayment { get; set; }

        [RequiredIf(nameof(ShowPotentialMonthlyPayment), true, ErrorMessage = "Please enter a potential monthly payment")]
        [Range(0.01, float.MaxValue, ErrorMessage = "Please enter a potential monthly payment")]
        public decimal? PotentialMonthlyPayment { get; set; }

        [RequiredIf(nameof(ShowInterestRate), true, ErrorMessage = "Please enter an interest rate")]
        [Range(0.01, float.MaxValue, ErrorMessage = "Please enter an interest rate")]
        public decimal? InterestRate { get; set; }

        [RequiredIf(nameof(ShowPotentialInterestRate), true, ErrorMessage = "Please enter a potential interest rate")]
        [Range(0.01, float.MaxValue, ErrorMessage = "Please enter a potential interest rate")]
        public decimal? PotentialInterestRate { get; set; }

        [RequiredIf(nameof(ShowPotentialEndDate), true, ErrorMessage = "Please enter a potential end date")]
        //Mortgage end date if overpaying
        public DateTime? PotentialEndDate { get; set; }

        [RequiredIf(nameof(ShowPromotionalEndDate), true, ErrorMessage = "Please enter a promotional end date")]
        //Promotional Period End Date
        public DateTime? PromotionalEndDate { get; set; }

        public bool ShowEndDate { get; set; }

        public bool ShowInsight { get; set; }

        public bool ShowMonthlyPayment { get; set; }

        public bool ShowPotentialMonthlyPayment { get; set; }

        public bool ShowInterestRate { get; set; }

        public bool ShowPotentialInterestRate { get; set; }

        public bool ShowPotentialEndDate { get; set; }

        public bool ShowPromotionalEndDate { get; set; }

        [RequiredIf(nameof(ShowMortgageNumber), true, ErrorMessage = "Please enter a mortgage number")]
        [RegularExpression("^[-a-zA-Z0-9(&)' - .-.]*$", ErrorMessage = "Name contains disallowed characters")]
        public string MortgageNumber { get; set; }

        public bool ShowMortgageNumber { get; set; }

        public string BrokerNotes { get; set; }

        public decimal? LoanAmount { get; set; }

        public int? TermInYears { get; set; }

        public bool ShowStartDate { get; set; }

        public bool ShowLoanAmount { get; set; }

        public bool ShowTermInYears { get; set; }

        public virtual ICollection<MortgageDocument> SupportingDocuments { get; set; }
    }
}
