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

public class AppDownloadsBase : BIQDashboardComponent
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

    public int? StaffId { get; set; }

    public User User { get; set; }

    public IEnumerable<Online.Models.Broker> Brokers { get; set; }

    protected bool IsLoadingDownloadData { get; set; }
    protected string DownloadTotal { get; set; }
    protected double? DownloadChange { get; set; }
    protected List<ChartSeries> DownloadData { get; set; }
    protected string[] DownloadLabels { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        IsLoadingDownloadData = true;

        User = await AccountService.GetUser();

        if (User.IsBrokerStaff)
        {
            StaffId = User.StaffBrokerId;
        }

        if (User.IsAdmin)
        {
            Brokers = await BrokerService.GetBrokers();
        }

        HandleDownloadPeriodChange(DAILY);
    }

    public override Task SetParametersAsync(ParameterView parameters)
    {
        return base.SetParametersAsync(parameters);
    }

    protected async void HandleDownloadPeriodChange(string period)
    {
        IsLoadingDownloadData = true;
        StateHasChanged();

        var result = await ChartDataService.GetDownloadData(User.MasterBrokerId, period);

        DownloadTotal = result.Total.ToString("F0");
        DownloadChange = result.ChangeBetweenPeriodsPercent;
        DownloadLabels = result.Items.Select(i => i.Label).ToArray();
        DownloadData = result.Items.ToChartSeries("Downloads") ;

        IsLoadingDownloadData = false;
        StateHasChanged();
    }
}

