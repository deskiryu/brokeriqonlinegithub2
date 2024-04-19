using System.Collections.Generic;
using System.Threading.Tasks;

using BrokerIQ.Dto.Enum;
using BrokerIQ.Dto.Models;

namespace BrokerIQ.Online.Server;

public interface IBrokerIntegrationService
{
    Task<IEnumerable<BrokerIntegrationDto>> GetBrokerIntegrations();

    Task<bool> Add(IntegrationEnum integration);

    Task<bool> Remove(IntegrationEnum integration);
}
