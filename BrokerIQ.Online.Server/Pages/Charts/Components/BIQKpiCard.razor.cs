using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace BrokerIQ.Online.Server.Pages.Charts.Components;

public partial class BIQKpiCard : BIQDashboardComponent
{
    [Parameter]
    public RenderFragment ChildContent { get; set; }

    public string PaperStyle { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        PaperStyle = $"border-radius: 10px; background-color : {BackgroundColour}";
    }
}