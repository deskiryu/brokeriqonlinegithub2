using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Server;
using BrokerIQ.Online.Server.Services.Base;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services;

public class BrokerIntegrationService : BrokerIQService, IBrokerIntegrationService
{
    private readonly string API_CONTROLLER = "BrokerIntegration";

    public BrokerIntegrationService(IAccountService accountService, IRequestProviderService requestProviderService) : base(accountService, requestProviderService)
    {
    }

    public async Task<IEnumerable<BrokerIntegrationDto>> GetBrokerIntegrations()
    {
        var brokerId = await GetCurrentBrokerId();

        try
        {
            var integrations = await requestProviderService.Get<IEnumerable<BrokerIntegrationDto>>($"{API_CONTROLLER}/broker/{brokerId}");
            return integrations;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get: exception {ex.Message}");
        }

        return Array.Empty<BrokerIntegrationDto>();
    }
}
