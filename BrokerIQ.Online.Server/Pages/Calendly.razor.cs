using BrokerIQ.Online.Server.AppSettings;
using BrokerIQ.Online.Server.Services.Interface;

using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;


namespace BrokerIQ.Online.Server.Pages;

public partial class Calendly
{
    [Inject]
    public ICalendlyService CalendlyService { get; set; }

    [Inject]
    public IOptions<CalendlySettings> Options { get; set; }

    [Inject]
    public NavigationManager NavigationManager { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);

        var success = false;

        if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("code", out var code))
        {
            success = await CalendlyService.RegisterCalendlyConnection(code);
        }

        NavigationManager.NavigateTo($"/clientlist?sucess={success}");
    }
}
