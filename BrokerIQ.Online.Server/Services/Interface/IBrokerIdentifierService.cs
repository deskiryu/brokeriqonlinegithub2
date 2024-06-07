using System.Threading.Tasks;

using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Server.Models;

namespace BrokerIQ.Online.Services.Interface
{
    public interface IBrokerIdentifierService
    {
        Task<BrokerIdentifier> UpdateBrokerIdentifier(BrokerIdentifier brokerIdentifier);

        Task<BrokerIdentifier> AddBrokerIdentifier(CreateBrokerIdentifierDto brokerIdentifier);

        Task<bool> DeleteBrokerIdentifier(int id);

        Task<BrokerIdentifier> GetDefaultBrokerIdentifier();
    }
}
