using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BrokerIQ.Online.Models
{
    public class Wealth : IValidatableObject
    {
        [Key]
        public int Id { get; set; }

        public int CustomerId { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "Policy number is too long.")]
        public string PolicyNumber { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Provider name is too long.")]
        public string ProviderName { get; set; }

        public DateTime? AmountInvestedOn { get; set; }

        [Range(0.1, double.MaxValue, ErrorMessage = "Growth percent has to be greater than 0")]
        public decimal? YearToDateGrowthPercent { get; set; }

        [Required]
        [Range(40, 150, ErrorMessage = "Please enter the retirement age goal")]
        public int RetirementAgeGoal { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Please enter the monthly income goal")]
        public decimal MonthlyIncomeGoal { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Please enter the customer monthly contribution")]
        public decimal CustomerMonthlyContribution { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Monthly contribution has to be greater than 0")]
        public decimal? EmployersMonthlyContribution { get; set; }

        public DateTime? NextReview { get; set; }

        public int WealthProductType { get; set; } = 1;

        public virtual ICollection<WealthDocument> SupportingDocuments { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (AmountInvestedOn.Value > DateTime.UtcNow)
            {
                yield return new ValidationResult("Amount invested date must be in the past.", new[] { "AmountInvestedOn" });
            }

            if (NextReview.Value <= DateTime.UtcNow)
            {
                yield return new ValidationResult("Next review date must be in the future.", new[] { "NextReview" });
            }
        }
    }
}

