using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Charts.Components;

public partial class BIQKpiCell : BIQDashboardComponent
{
    [Parameter]
    public string Value { get; set; }

    [Parameter]
    public RenderFragment ChildContent { get; set; }
}

