using System.Threading.Tasks;
using BrokerIQ.Dto.Dto;

namespace BrokerIQ.Online.Server.Services.Interface
{
    public interface IInsuranceQuoteService
    {
        Task<IncomeProtectionQuoteDto> GetIncomeProtectionQuoteFor(IncomeProtectionQuoteDataDto quoteData);
    }
}