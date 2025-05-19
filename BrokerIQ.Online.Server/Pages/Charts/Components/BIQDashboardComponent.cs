using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Charts.Components;

public abstract class BIQDashboardComponent : ComponentBase
{
    [Parameter]
    public string BackgroundColour { get; set; }

    protected const string DAILY = "Daily";
    protected const string WEEKLY = "Weekly";
    protected const string MONTHLY = "Monthly";
    protected const string YEARLY = "Yearly";

    protected string[] PeriodOptions;

    protected ChartOptions Options = new ChartOptions()
    {
        ChartPalette = GetChartPallete()
    };

    protected static string[] GetChartPallete()
    {
        return new string[] { "#EDBF33", "#EA5545", "#F46A9B", "#EF9B20", "#BDCF32", "#27AEEF", "#B33DC6", "#87BC45", "#EDE15B", "#B30000", "#7C1158", "#4421AF", "#1A53FF", "#0D88E6", "#00B7C7", "#5AD45A", "#8BE04E", "#EBDC78" };
    }
}

