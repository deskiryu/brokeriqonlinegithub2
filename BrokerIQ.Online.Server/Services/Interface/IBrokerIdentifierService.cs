using System.Threading.Tasks;

namespace BrokerIQ.Online.Services.Interface
{
    using BrokerIQ.Online.Server.Models;

    public interface IBrokerIdentifierService
    {
        Task<BrokerIdentifier> UpdateBrokerIdentifier(BrokerIdentifier brokerIdentifier);

        Task<BrokerIdentifier> AddBrokerIdentifier(int brokerID);

        Task<bool> DeleteBrokerIdentifier(int id);
    }
}
