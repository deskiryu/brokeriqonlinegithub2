using System.Collections.Generic;
using System.Threading.Tasks;

using BrokerIQ.Dto.Models;

namespace BrokerIQ.Online.Server;

public interface IBrokerIntegrationService
{
    Task<IEnumerable<BrokerIntegrationDto>> GetBrokerIntegrations();
}
