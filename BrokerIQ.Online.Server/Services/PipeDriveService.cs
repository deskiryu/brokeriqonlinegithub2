using System.Threading.Tasks;
using AutoMapper;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Services.Interface;
using BrokerIQ.Online.Services.Abstract;

namespace BrokerIQ.Online.Server;

public class PipedriveService : IPipedriveService
{
    private readonly string Url = "Pipedrive";

    private readonly IRequestProviderService requestProviderService;
    private readonly IMapper mapper;

    public PipedriveService(IRequestProviderService requestProviderService,
        IMapper mapper)
    {
        this.requestProviderService = requestProviderService;
        this.mapper = mapper;
    }

    public async Task<bool> RegisterConnection(string code)
    {
        return await this.requestProviderService.Post($"{Url}/auth", code);
    }

    public async Task<bool> Disconnect()
    {
        return await this.requestProviderService.Delete($"{Url}");
    }

    public async Task<Customer> SyncCustomer(int id)
    {
        var dto = await this.requestProviderService.Post<int, CustomerDto>($"{this.Url}/customersync", id);

        return mapper.Map<Customer>(dto);
    }

    public async Task SyncChatMessages(int id)
    {
        await this.requestProviderService.Post<int, CustomerDto>($"{this.Url}/chatsync", id);
    }
}