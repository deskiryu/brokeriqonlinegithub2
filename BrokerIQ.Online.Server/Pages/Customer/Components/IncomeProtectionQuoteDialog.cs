using System.Threading.Tasks;
using BrokerIQ.Dto.Dto;
using BrokerIQ.Online.Server.Services.Interface;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Customer.Components
{
    public partial class IncomeProtectionQuoteDialog
    {
        [Inject]
        private IInsuranceQuoteService InsuranceQuoteService { get; set; }

        [Inject]
        public IChatService ChatService { get; set; }

        [Inject]
        private ISnackbar Snackbar { get; set; }

        [Microsoft.AspNetCore.Components.CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Microsoft.AspNetCore.Components.Parameter]
        public Online.Models.Customer Customer { get; set; }

        MudForm form;

        private IncomeProtectionQuoteDataDto LastQuoteData { get; set; }

        private decimal LastBenefitAmount { get; set; }

        protected decimal LastPremiumAmount { get; set; }

        protected string[] LivesAssured { get; private set; } = new string[] { "First", "Second", "Both" };

        protected string[] PremiumTypes { get; private set; } = new string[] { "Guaranteed", "Reviewable" };

        protected string[] BenefitBasis { get; private set; } = new string[] { "Maximum", "Monthly" };

        protected string[] DeferredPeriods { get; private set; } = new string[] { "One", "Two", "Three", "Six", "Twelve", "TwentyFour" };

        protected string[] Indexations { get; private set; } = new string[] { "Level", "RPI", "Three", "Five" };

        protected string SelectedLife { get; set; } = "First";

        protected string SelectedPremiumType { get; set; } = "Guaranteed";

        public int SelectedToAge { get; set; } = 60;

        protected string SelectedBenefitBasis { get; set; } = "Monthly";

        public decimal SelectedBenefitAmount { get; set; } = 1000;

        public string SelectedDeferredPeriod { get; set; } = "One";

        public string SelectedIndexation { get; set; } = "Level";

        public bool IncludeLimitedPaymentPlans { get; set; } = false;

        protected bool isProcessing = false;

        protected string QuoteText { get; set; } = string.Empty;

        protected async Task GetQuote()
        {
            isProcessing = true;

            LastQuoteData = new IncomeProtectionQuoteDataDto()
            {
                CustomerId = Customer.Id,
                LivesAssured = "First",
                ToAge = SelectedToAge,
                DeferredPeriod = SelectedDeferredPeriod,
                Indexation = SelectedIndexation,
                PremiumType = SelectedPremiumType,
                IncludeLimitedPaymentPlans = true
            };

            var quoteResult = await InsuranceQuoteService.GetIncomeProtectionQuoteFor(LastQuoteData);

            LastBenefitAmount = quoteResult.BenefitAmount;
            LastPremiumAmount = quoteResult.PremiumAmount;

            QuoteText = LastPremiumAmount > 0 ?
                $"Customer might be able to get insurance with a premium of £{LastPremiumAmount} for a benefit of £{LastBenefitAmount}." :
                "No insurance quotes available for this customer with the provided options. Is there some client information missing ?";

            isProcessing = false;
        }

        void Cancel() => MudDialog.Cancel();

        protected async Task SendChatToCustomer()
        {
            if (await ChatService.Send("Your broker is currently studying your case file. If they do not contact you in the meantime, please get in touch.", Customer.Id))
            {
                Snackbar.Add("Customer chat message was sent.", Severity.Success);
            }
            else
            {
                Snackbar.Add("Unable to send chat message to customer. Please try again", Severity.Error);
            }

            MudDialog.Cancel();
        }
    }
}