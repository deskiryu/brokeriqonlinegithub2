using System.Threading.Tasks;
using BrokerIQ.Dto.Dto;
using BrokerIQ.Online.Server.Services.Interface;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Customer.Components
{
    public partial class IncomeProtectionQuoteDialog
    {
        [Inject]
        private IInsuranceQuoteService InsuranceQuoteService { get; set; }

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

        protected string SelectedPremiumType { get; set; } = "Guaranteed";

        public int SelectedToAge { get; set; } = 60;

        protected string SelectedBenefitBasis { get; set; } = "Monthly";

        public decimal SelectedBenefitAmount { get; set; } = 1000;

        public string SelectedDeferredPeriod { get; set; } = "Zero";

        public string SelectedIndexation { get; set; } = "Level";

        public bool IncludeLimitedPaymentPlans { get; set; } = false;

        protected bool isProcessing = false;

        protected string QuoteText { get; set; } = string.Empty;

        protected async Task GetQuote()
        {
            isProcessing = true;

            var quoteData = new IncomeProtectionQuoteDataDto()
            {
                CustomerId = Customer.Id,
                LivesAssured = "First",
                ToAge = SelectedToAge,
                DeferredPeriod = SelectedDeferredPeriod,
                Indexation = SelectedIndexation,
                PremiumType = SelectedPremiumType,
                IncludeLimitedPaymentPlans = true
            };

            var quoteResult = await InsuranceQuoteService.GetIncomeProtectionQuoteFor(quoteData);

            QuoteText = quoteResult.PremiumAmount <= 0 ?
                "No insurance quotes available for this customer with the provided options. Is there some client information missing ?" :
                $"Customer might be able to get insurance with a premium of £{quoteResult.PremiumAmount} for a benefit of £{quoteResult.BenefitAmount}.";

            // await Task.Delay(2000);

            isProcessing = false;
        }

        void Cancel() => MudDialog.Cancel();
    }
}