using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Online.Models;
using BrokerIQ.Dto.Request;

namespace BrokerIQ.Online.Services.Interface
{
    public interface IBrokerService
    {
        Task<Broker> GetBroker(int id, bool eagerload = false);

        Task<IEnumerable<Broker>> GetBrokers();

        Task<IEnumerable<Broker>> GetBrokersByList(IEnumerable<int> ids);

        Task<Broker> UpdateBroker(Broker ins);

        Task<Broker> AddBroker(Broker ins);

        Task<bool> DeleteBroker(int id);

        Task<BoolResponseDto> VerifyBroker(int id);
    }
}
