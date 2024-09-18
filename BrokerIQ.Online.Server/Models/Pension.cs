using System;
using System.ComponentModel.DataAnnotations;

namespace BrokerIQ.Online.Models
{
    public class Pension
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

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Please enter the current pension amount")]
        public decimal CurrentPensionAmount { get; set; }

        [Range(0.1, double.MaxValue, ErrorMessage = "Growth percent has to be greater than 0")]
        public decimal? YearToDateGrowthPercent { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Please enter the estimated anount at retirement")]
        public decimal EstimatedAmountAtRetirement { get; set; }

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
    }
}

