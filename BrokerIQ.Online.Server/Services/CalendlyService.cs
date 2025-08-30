using System.Threading.Tasks;
using BrokerIQ.Online.Server.Services.Interface;
using BrokerIQ.Online.Services.Abstract;

namespace BrokerIQ.Online.Server;

public class CalendlyService : ICalendlyService
{
    private readonly string Url = "Calendly";

    private readonly IRequestProviderService requestProviderService;

    public CalendlyService(IRequestProviderService requestProviderService)
    {
        this.requestProviderService = requestProviderService;
    }

    public async Task<bool> RegisterConnection(string code)
    {
        return await this.requestProviderService.Post($"{Url}/auth", code);
    }

    public async Task<bool> Disconnect()
    {
        return await this.requestProviderService.Delete($"{Url}");
    }
}