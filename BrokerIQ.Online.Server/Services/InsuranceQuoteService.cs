using System.Threading.Tasks;
using BrokerIQ.Dto.Dto;
using BrokerIQ.Online.Server.Services.Interface;
using BrokerIQ.Online.Services.Abstract;

namespace BrokerIQ.Online.Server.Services
{
    public class InsuranceQuoteService : IInsuranceQuoteService
    {
        private readonly string URL = "InsuranceQuote";

        public IRequestProviderService RequestProviderService { get; }

        public InsuranceQuoteService(IRequestProviderService requestProviderService)
        {
            RequestProviderService = requestProviderService;
        }

        public async Task<IncomeProtectionQuoteDto> GetIncomeProtectionQuoteFor(IncomeProtectionQuoteDataDto quoteData)
        {
            try
            {
                return await RequestProviderService.Post<IncomeProtectionQuoteDataDto, IncomeProtectionQuoteDto>(URL, quoteData);
            }
            catch (System.Exception)
            {
                return new IncomeProtectionQuoteDto();
            }
        }
    }
}