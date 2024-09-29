using System;
using System.Linq;
using BrokerIQ.Dto.Enum;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;

namespace BrokerIQ.Online.Server.Pages.Broker.Components;

public partial class ServicesStack
{
    [Inject]
    public IBrokerService brokerService { private get; set; }

    [Parameter]
    public Online.Models.Broker Broker { get; set; }

    protected Tuple<ServicesEnum, bool>[] ServicesProvided;

    protected override void OnInitialized()
    {
        FillServicesProvided();
    }

    private void FillServicesProvided()
    {
        ServicesProvided = Array.Empty<Tuple<ServicesEnum, bool>>();

        foreach (var service in (ServicesEnum[])Enum.GetValues(typeof(ServicesEnum)))
        {
            ServicesProvided = ServicesProvided.Append(Tuple.Create(service, Broker.BrokerServices.Any(bs => bs.ServiceId == (int)service))).OrderBy(o => o.Item1).ToArray();
        }
    }

    async void ToggleService(int serviceIndex)
    {
        await brokerService.ToggleService(Broker.Id, (int)ServicesProvided[serviceIndex].Item1);

        Broker = await brokerService.GetBroker(Broker.Id, true);

        FillServicesProvided();

        StateHasChanged();
    }
}