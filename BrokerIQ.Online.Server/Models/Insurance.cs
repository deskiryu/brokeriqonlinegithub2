using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using BrokerIQ.Dto.Enum;
using BrokerIQ.Online.Attributes;

namespace BrokerIQ.Online.Models
{
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
        public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;

        public bool ShowStartDate { get; set; }

        [Required]
        public DateTime ReviewDate { get; set; } = DateTime.UtcNow.AddMonths(6).Date;

        public bool ShowReviewDate { get; set; }

        [RequiredIf(nameof(ShowExpiryDate), true, ErrorMessage = "Please enter a expiry date")]
        public DateTime? ExpiryDate { get; set; }

        public bool ShowExpiryDate { get; set; }

        public DateTime LastNotificationCheck { get; set; }

        public bool NotificationMinus3Sent { get; set; }

        public bool NotificationMinus2Sent { get; set; }

        public bool NotificationMinus1Sent { get; set; }

        public decimal? Cost { get; set; }

        public bool Annual { get; set; }

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

        [RequiredIf(nameof(ShowSecondTermAmount), true, ErrorMessage = "Please enter a second benefit amount")]
        [Range(0.01, float.MaxValue, ErrorMessage = "Please enter a second benefit amount")]
        public decimal? SecondTermAmount { get; set; }

        public bool ShowSecondTermAmount { get; set; }

        public bool FractureCover { get; set; }

        public bool ShowFractureCover { get; set; }

        public bool ShowPolNumber { get; set; }

        public PaymentMethodEnum PaymentMethod { get; set; }

        public bool ShowPaymentMethod { get; set; }

        public DateTime? RetroactiveDate { get; set; }

        public bool ShowRetroactiveDate { get; set; }

        [StringLength(200, ErrorMessage = "Jurisdiction is too long.")]
        public string Jurisdiction { get; set; }

        public bool ShowJurisdiction { get; set; }

        [StringLength(200, ErrorMessage = "Territorial Limit is too long.")]
        public string TerritorialLimit { get; set; }

        public bool ShowTerritorialLimit { get; set; }

        public bool? IsInTrust { get; set; }

        public bool? HasWill { get; set; }

        public bool AvailableToClient { get; set; }

        public virtual ICollection<InsuranceDocument> SupportingDocuments { get; set; }
    }
}
