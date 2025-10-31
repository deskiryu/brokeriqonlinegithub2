using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Online.Models;

namespace BrokerIQ.Online.Server.Services.Interface
{
    public interface IAddOnBenefitSampleService
    {
        Task<IReadOnlyList<AddOnBenefit>> GetSamplesAsync(int insuranceType, int count);
    }
}
