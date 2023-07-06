using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Models
{
    using BrokerIQ.Dto.Enum;
    using BrokerIQ.Online.Attributes;
    using System.ComponentModel.DataAnnotations;

    public class Insurance
    {
        [Key]
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public int BrokerId { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Name is too long.")]
        public string Name { get; set; }

        [Required]
        public InsuranceEnum InsType { get; set; }

        [Required]
        [StringLength(15, ErrorMessage = "Number is too long.")]
        public string ContactNumber { get; set; }

        [StringLength(15, ErrorMessage = "Number is too long.")]
        public string PolNumber { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        public DateTime LastNotificationCheck { get; set; }

        public bool NotificationMinus3Sent { get; set; }

        public bool NotificationMinus2Sent { get; set; }

        public bool NotificationMinus1Sent { get; set; }

        [Required]
        [Range(0.01, float.MaxValue, ErrorMessage = "Please enter a cost")]
        public decimal Cost { get; set; }

        public bool Annual { get; set; }

        [RequiredIf(nameof(ShowReviewDate), true, ErrorMessage = "Please enter a review date")]
        public DateTime? ReviewDate { get; set; }

        public bool ShowReviewDate { get; set; }

        public int? MenuPlanId { get; set; }

        public TermTypeEnum TermType { get; set; }

        [RequiredIf(nameof(ShowTermYears), true, ErrorMessage = "Please enter a term in years")]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter a term in years")]
        public int? TermYears { get; set; }

        public bool ShowTermYears { get; set; }

        [RequiredIf(nameof(ShowTermAmount), true, ErrorMessage = "Please enter a benefit amount")]
        [Range(0.01, float.MaxValue, ErrorMessage = "Please enter a benefit amount")]
        public decimal? TermAmount { get; set; }

        public bool ShowTermAmount { get; set; }

        [RequiredIf(nameof(ShowDeferredPeriodWeeks), true, ErrorMessage = "Please enter a deferred period in weeks")]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter a deferred period in weeks")]
        public int? DeferredPeriodWeeks { get; set; }

        public bool ShowDeferredPeriodWeeks { get; set; }

        public string BrokerNotes { get; set; }

        public decimal SecondTermAmount { get; set; }

        public bool ShowSecondTermAmount { get; set; }

        public bool FractureCover { get; set; }

        public bool ShowFractureCover { get; set; }

        public virtual ICollection<InsuranceDocument> SupportingDocuments { get; set; }
    }
}
