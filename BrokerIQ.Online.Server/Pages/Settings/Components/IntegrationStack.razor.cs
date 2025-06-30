using System;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Dto.Dto;
using BrokerIQ.Dto.Enum;
using BrokerIQ.Online.Server.AppSettings;
using BrokerIQ.Online.Server.Services.Interface;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace BrokerIQ.Online.Server.Pages.Settings.Components;

public partial class IntegrationStack
{
    [Inject]
    public IBrokerIntegrationService BrokerIntegrationService { private get; set; }

    [Inject]
    public IPipedriveService PipedriveService { private get; set; }

    [Inject]
    public IBrokerService BrokerService { private get; set; }

    [Inject]
    public IOptions<PipedriveSettings> Options { get; set; }

    [Inject]
    public NavigationManager NavigationManager { get; set; }

    [Parameter]
    public Online.Models.Broker Broker { get; set; }

    protected bool AllowCalendlyConnection { get; set; }

    protected PipedriveAccessDetailsDto PipedriveDetails { get; set; }

    public bool ConnectedToPipedrive { get; set; }

    protected string PipedriveButtonText => PipedriveDetails is null || PipedriveDetails.ExpiresAt < DateTime.Now ? "Login to Pipedrive" : "Pipedrive connected";

    protected bool PipedriveButtonDisabled => PipedriveDetails is not null && PipedriveDetails.ExpiresAt > DateTime.Now;

    protected override async Task OnInitializedAsync()
    {
        var integrations = await BrokerIntegrationService.GetBrokerIntegrations();

        AllowCalendlyConnection = integrations.Any(i => i.Integration == Dto.Enum.IntegrationEnum.Calendly);

        PipedriveDetails = await BrokerService.GetBrokerPipedriveDetails(Broker.Id);

        ConnectedToPipedrive = PipedriveDetails is not null && PipedriveDetails.ExpiresAt > DateTime.Now;
    }

    private async void ToggleCalendlyConnection()
    {
        if (AllowCalendlyConnection)
        {
            await BrokerIntegrationService.Remove(IntegrationEnum.Calendly);
        }
        else
        {
            await BrokerIntegrationService.Add(IntegrationEnum.Calendly);
        }

        var integrations = await BrokerIntegrationService.GetBrokerIntegrations();

        AllowCalendlyConnection = integrations.Any(i => i.Integration == Dto.Enum.IntegrationEnum.Calendly);

        StateHasChanged();
    }

    public void OnPipedriveToggledChanged(bool turnOn)
    {
        if(turnOn)
        {
            NavigationManager.NavigateTo(GetAuthorizationUrl());
        }
        else
        {
            PipedriveService.Disconnect();
            ConnectedToPipedrive = turnOn;
        }
    }

    protected string GetAuthorizationUrl(string state = null)
    {
        var url = $"{Options.Value.BaseAuthUri}/authorize?client_id={Options.Value.ClientId}&redirect_uri={Uri.EscapeDataString(Options.Value.BiqReturnUri)}";

        if (!string.IsNullOrWhiteSpace(state))
            url += $"&state={Uri.EscapeDataString(state)}";

        return url;
    }

    protected bool ShowAppointmentSection()
    {
        if (Broker is null) return false; // disabled while broker is not set

        if (Broker.BrokerIdentifier is null || !Broker.BrokerIdentifier.IdentifierFound) return true;

        return Broker.BrokerIdentifier.HasAppointments;
    }
}