using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.CreateDto;
using BrokerIQ.Dto.Dto;
using BrokerIQ.Dto.UpdateDto;

namespace BrokerIQ.Online.Services.Interface
{
    public interface IBrokerSubscriptionService
    {
        Task<IEnumerable<BrokerSubscriptionDto>> GetAllForBroker(int brokerId);

        Task<bool> Create(CreateBrokerSubscriptionDto subscription);

        Task<bool> Update(UpdateBrokerSubscriptionDto subscription);
    }
}