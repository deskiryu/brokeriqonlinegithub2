using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Models;

namespace BrokerIQ.Online.Services.Interface
{
    public interface IBrokerDefinedMessageService
    {
        Task<IEnumerable<BrokerDefinedMessageDto>> GetAllForCurrentBroker();

        Task<IEnumerable<BrokerDefinedMessageDto>> GetForBroker(int brokerId);

        Task<bool> Create(CreateBrokerDefinedMessageDto message);

        Task<bool> Update(BrokerDefinedMessageDto message);

        Task<bool> Delete(BrokerDefinedMessageDto message);
    }
}
