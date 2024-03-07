using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

using BrokerIQ.Online.Server.AppSettings;
using BrokerIQ.Online.Server.Services.Interface;
using System.Linq;
using System.Threading;
using BrokerIQ.Dto.Enum;

namespace BrokerIQ.Online.Server.Pages.Settings.Components;

public partial class IntegrationStack
{
    [Inject]
    public IBrokerIntegrationService brokerIntegrationService { private get; set; }

    [Parameter]
    public Online.Models.Broker Broker { get; set; }

    protected bool AllowCalendlyConnection { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var integrations = await brokerIntegrationService.GetBrokerIntegrations();

        AllowCalendlyConnection = integrations.Any(i => i.Integration == Dto.Enum.IntegrationEnum.Calendly);
    }

    async void ToggleCalendlyConnection()
    {
        if (AllowCalendlyConnection)
        {
            await brokerIntegrationService.Remove(IntegrationEnum.Calendly);
        }
        else
        {
            await brokerIntegrationService.Add(IntegrationEnum.Calendly);
        }

        var integrations = await brokerIntegrationService.GetBrokerIntegrations();

        AllowCalendlyConnection = integrations.Any(i => i.Integration == Dto.Enum.IntegrationEnum.Calendly);

        StateHasChanged();
    }
}