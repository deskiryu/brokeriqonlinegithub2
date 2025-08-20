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

    protected string DEFAULT_SERIES_KEY = string.Empty;

    protected static string[] Pallete = new string[] { "#ebe5fd", "#cebffa", "#d8ccfb", "#3f2c58", "#f5f2fe", "#cdd0f8", "#ffcae9" };

    protected string TitleStyle { get; set; } = $"color: {Colors.Grey.Darken1}";

    protected string ValueStyle { get; set; } = $"color: {Colors.Shades.Black}";

    public const string TODAY = "Today";
    public const string THIS_WEEK = "This Week";
    public const string LAST_7_DAYS = "Last 7 Days";
    public const string THIS_MONTH = "This Month";
    public const string LAST_3_MONTHS = "Last 3 Months";
    public const string LAST_12_WEEKS = "Last 12 Weeks";

    protected string GraphWidth => "100%";
    protected string GraphHeight => "300px";

    protected string[] PeriodOptions =
    {
        TODAY,
        THIS_WEEK,
        LAST_7_DAYS,
        THIS_MONTH,
        LAST_3_MONTHS,
        LAST_12_WEEKS
    };

    protected ChartOptions Options = new ChartOptions()
    {
        ChartPalette = Pallete,
        MaxNumYAxisTicks = 10
    };
}
