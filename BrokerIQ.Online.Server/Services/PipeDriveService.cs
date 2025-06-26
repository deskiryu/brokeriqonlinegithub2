using System.Threading.Tasks;
using BrokerIQ.Online.Server.Services.Interface;
using BrokerIQ.Online.Services.Abstract;

namespace BrokerIQ.Online.Server;

public class PipedriveService : IPipedriveService
{
    private readonly string Url = "Pipedrive";

    private readonly IRequestProviderService requestProviderService;

    public PipedriveService(IRequestProviderService requestProviderService)
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