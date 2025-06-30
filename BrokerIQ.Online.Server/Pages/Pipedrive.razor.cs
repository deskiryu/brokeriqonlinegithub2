using System.Threading.Tasks;
using BrokerIQ.Online.Server.Services.Interface;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace BrokerIQ.Online.Server.Pages;

public partial class Pipedrive
{
    [Inject]
    public IPipedriveService PipedriveService { get; set; }

    [Inject]
    public NavigationManager NavigationManager { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);

        var success = false;

        if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("code", out var code))
        {
            success = await PipedriveService.RegisterConnection(code);
        }

        NavigationManager.NavigateTo($"/settingsedit?sucess={success}");
    }
}
