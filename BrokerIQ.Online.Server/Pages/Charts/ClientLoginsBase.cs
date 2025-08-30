using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Extensions;
using BrokerIQ.Online.Server.Pages.Charts.Components;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Pages;

public class ClientLoginsBase : BIQDashboardComponent
{
    [Inject]
    public NavigationManager NavigationManager { get; set; }

    [Inject]
    public IAccountService AccountService { get; set; }

    [Inject]
    public IBrokerService BrokerService { get; set; }

    [Inject]
    public IBrokerStaffService BrokerStaffService { get; set; }

    [Inject]
    public IChartDataService ChartDataService { get; set; }

    [Parameter]
    public string BrokerId { get; set; }

    public int _BrokerId { get; set; }

    public int? StaffId { get; set; }

    public User User { get; set; }

    public string Href { get; set; } 

    protected bool IsLoadingClientLoginData { get; set; }
    protected string ClientLoginTotal { get; set; }
    protected double? ClientLoginChange { get; set; }
    protected string ClientLoginAverage { get; set; }
    protected double? ClientLoginAverageChange { get; set; }
    protected List<ChartSeries> CustomerLoginData { get; set; }
    protected string[] CustomerLoginLabels { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        IsLoadingClientLoginData = true;

        User = await AccountService.GetUser();

        if (User.IsBrokerStaff)
        {
            StaffId = User.StaffBrokerId;
        }

        var brokerId = 0;
        if (User.IsAdmin || User.IsMinorAdmin)
        {
            System.Int32.TryParse(BrokerId, out brokerId);
            _BrokerId = brokerId;
            Href = $"charts/dashboard/{_BrokerId}";
        }
        else
        {
            _BrokerId = User.MasterBrokerId;
            Href = $"charts/dashboard/0";
        }

        HandleClientLoginPeriodChange(DAILY);
    }

    protected async void HandleClientLoginPeriodChange(string period)
    {
        IsLoadingClientLoginData = true;
        StateHasChanged();

        var result = await ChartDataService.GetClientLoginData(User.MasterBrokerId, StaffId, period);

        ClientLoginTotal = result.Total[DEFAULT_SERIES_KEY].Value.ToString("N0");
        ClientLoginChange = result.ChangeInTotalBetweenPeriodsPercent[DEFAULT_SERIES_KEY];
        ClientLoginAverage = result.Average[DEFAULT_SERIES_KEY].HasValue ? result.Average[DEFAULT_SERIES_KEY].Value.ToString("N2") : string.Empty;
        ClientLoginAverageChange = result.ChangeInAverageBetweenPeriodsPercent[DEFAULT_SERIES_KEY].HasValue ?
            result.ChangeInAverageBetweenPeriodsPercent[DEFAULT_SERIES_KEY] : 0;

        CustomerLoginLabels = result.Items.Select(i => i.PeriodLabel).ToArray();
        CustomerLoginData = result.Items.ToChartSeries("Total");

        IsLoadingClientLoginData = false;
        StateHasChanged();
    }

}

