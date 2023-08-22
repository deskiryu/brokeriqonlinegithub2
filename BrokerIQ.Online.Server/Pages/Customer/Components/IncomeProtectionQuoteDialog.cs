using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Customer.Components
{
    public partial class IncomeProtectionQuoteDialog
    {
        [Microsoft.AspNetCore.Components.CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Microsoft.AspNetCore.Components.Parameter]
        public Online.Models.Customer Customer { get; set; }

        MudForm form;

        protected string[] LivesAssured { get; private set; } = new string[] { "First", "Second", "Both" };

        protected string[] PremiumTypes { get; private set; } = new string[] { "Guaranteed", "Reviewable" };

        protected string[] BenefitBasis { get; private set; } = new string[] { "Maximum", "Monthly" };

        protected string[] DeferredPeriods { get; private set; } = new string[] { "Zero", "OneWeek", "TwoWeeks", "One", "Two", "Three", "Six", "Twelve", "TwentyFour" };

        protected string[] Indexations { get; private set; } = new string[] { "Level", "RPI", "Three", "Five" };

        protected string SelectedLife { get; set; } = "First";

        protected string SelectedPremium { get; set; } = "Guaranteed";

        public int SelectedToAge { get; set; } = 60;

        protected string SelectedBenefitBasis { get; set; } = "Monthly";

        public decimal SelectedBenefitAmount { get; set; } = 1000;

        public string SelectedDeferredPeriod { get; set; } = "Zero";

        public string SelectedIndexation { get; set; } = "Level";

        public bool IncludeLimitedPaymentPlans { get; set; } = false;

        void Submit()
        {
            form.Validate();

            if (form.IsValid)
            {

            };
        }

        void Cancel() => MudDialog.Cancel();
    }
}