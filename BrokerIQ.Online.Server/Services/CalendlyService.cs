using System.Threading.Tasks;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Server.Services.Interface;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Server;

public class CalendlyService : ICalendlyService
{
    private readonly string Url = "Calendly";

    private readonly IRequestProviderService requestProviderService;
    private readonly IAccountService accountService;

    public CalendlyService(IRequestProviderService requestProviderService, IAccountService accountService)
    {
        this.requestProviderService = requestProviderService;
        this.accountService = accountService;
    }

    public async Task<CalendlyUserDto> GetUser()
    {
        var user = await this.accountService.GetUser();
        this.requestProviderService.Token = user?.Token;

        return await this.requestProviderService.Get<CalendlyUserDto>($"{Url}/user");
    }

    public async Task<bool> RegisterCalendlyConnection(string code)
    {
        var user = await this.accountService.GetUser();
        this.requestProviderService.Token = user?.Token;

        return await this.requestProviderService.Post($"{Url}/auth", code);
    }
}