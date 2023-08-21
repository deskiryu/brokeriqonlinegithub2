using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Dto;
using BrokerIQ.Dto.Models;

namespace BrokerIQ.Online.Services.Interface
{
    public interface IBrokerSubscriptionService
    {
        Task<IEnumerable<BrokerSubscriptionDto>> GetAllForBroker(int brokerId);

        Task<bool> Create(CreateBrokerSubscriptionDto subscription);

        Task<bool> Update(UpdateBrokerSubscriptionDto subscription);
    }
}