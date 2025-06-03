using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Charts.Components;

public abstract class BIQDashboardComponent : ComponentBase
{
    [Parameter]
    public string BackgroundColour { get; set; }

    [Parameter]
    public string Title { get; set; }

    [Parameter]
    public bool IsLoadingData { get; set; }

    protected static string[] Pallete = new string[] { "#ffde1a", "#ff7400", "#ffa700", "#ff8d00", "#ffce00" };

    protected string TitleStyle { get; set; } = $"color: {Colors.Grey.Darken1}";

    protected string ValueStyle { get; set; } = $"color: {Colors.Shades.Black}";

    public const string DAILY = "Daily";
    public const string WEEKLY = "Weekly";
    public const string MONTHLY = "Monthly";
    public const string YEARLY = "Yearly";

    protected string GraphWidth => "100%";
    protected string GraphHeight => "300px";

    protected string[] PeriodOptions =
    {
        DAILY,
        WEEKLY,
        MONTHLY,
        YEARLY
    };

    protected ChartOptions Options = new ChartOptions()
    {
        ChartPalette = Pallete
    };
}

