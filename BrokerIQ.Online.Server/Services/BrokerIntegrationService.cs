using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using BrokerIQ.Dto.Enum;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Server;
using BrokerIQ.Online.Server.Services.Base;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services;

public class BrokerIntegrationService : BIQService, IBrokerIntegrationService
{
    private readonly string API_CONTROLLER = "BrokerIntegration";

    public BrokerIntegrationService(IAccountService accountService, IRequestProviderService requestProviderService, CookieService cookieService)
            : base(accountService, requestProviderService, cookieService)
    {
    }

    public async Task<IEnumerable<BrokerIntegrationDto>> GetBrokerIntegrations()
    {
        var brokerId = await GetCurrentBrokerId();

        try
        {
            var integrations = await _requestProviderService.Get<IEnumerable<BrokerIntegrationDto>>($"{API_CONTROLLER}/broker/{brokerId}");
            return integrations;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get: exception {ex.Message}");
        }

        return Array.Empty<BrokerIntegrationDto>();
    }

    public async Task<bool> Add(IntegrationEnum integration)
    {
        var brokerId = await GetCurrentBrokerId();

        try
        {
            var addedIntegration = await _requestProviderService.Post<int, BrokerIntegrationDto>($"{API_CONTROLLER}/broker/{brokerId}", (int)integration);

            if (addedIntegration != null) return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Post: exception {ex.Message}");
        }

        return false;
    }

    public async Task<bool> Remove(IntegrationEnum integration)
    {
        var brokerId = await GetCurrentBrokerId();

        try
        {
            return await _requestProviderService.Delete($"{API_CONTROLLER}/broker/{brokerId}/integration/{(int)integration}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Delete: exception {ex.Message}");
        }

        return false;
    }
}
