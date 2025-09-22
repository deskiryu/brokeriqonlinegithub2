using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace BrokerIQ.Online.Server.Pages.Charts.Components;

public partial class BIQKpiCard : BIQDashboardComponent
{
    [Parameter]
    public RenderFragment Summary { get; set; }

    [Parameter]
    public RenderFragment Body { get; set; }

    [Parameter]
    public EventCallback<string> OnPeriodChange { get; set; }

    [Parameter]
    public EventCallback OnClick { get; set; }

    [Parameter]
    public string SkeletonHeight { get; set; } = "89px";

    protected string SelectedPeriodOption { get; set; } = TODAY;

    protected string PaperStyle { get; set; }

    protected string BarMargin => Body is not null ? "mb-0": "mb-2";

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        PaperStyle = $"border-radius: 10px; background-color : {BackgroundColour}";
    }

    protected async Task HandleOnPeriodChange(string period)
    {
        SelectedPeriodOption = period;

        await OnPeriodChange.InvokeAsync(period);
    }

    protected async Task HandleOnClick() {
        await OnClick.InvokeAsync();
    }
}