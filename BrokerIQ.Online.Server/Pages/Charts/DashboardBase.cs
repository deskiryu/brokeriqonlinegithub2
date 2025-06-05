using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Pages.Charts.Components;
using BrokerIQ.Online.Services.Interface;

using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Pages
{
    public class DashboardBase : BIQDashboardComponent
    {
        protected List<ChartSeries> _series = new List<ChartSeries>()
        {
            new ChartSeries() { Name = "Mortgage & Insurance", Data = new double[] { 400, 200, 250, 270, 460, 600, 480} },
            new ChartSeries() { Name = "Mortgage Only", Data = new double[] { 190, 240, 350, 130, 280, 150, 130 } },
            new ChartSeries() { Name = "No Products", Data = new double[] { 80, 60, 110, 130, 40, 160, 100 } },
        };
        protected string[] _xAxisLabels = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };


        protected string[] advisorHeadings = { "Advisor Name", "Mortgage", "Insurance", "Referral", "Conversion", "Latest Engagement" };
        protected string[] rows = {
            @"Advisor Name 1;1;5;23;20;Just Now",
            @"Advisor Name 5;3;3;20;20;A minute ago",
            @"Advisor Name 7;1;5;12;20;30 minutes ago",
            @"Advisor Name 2;1;8;9;20;45 minutes ago",
            @"Advisor Name 9;5;3;16;20;1 hour ago",
        };

        public double[] data = { 100, 64 };
        public string[] labels = { "Total Sent", "Total Opened" };

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

        public IEnumerable<Broker> Brokers { get; set; }

        protected bool IsLoadingDownloadData { get; set; }
        protected string DownloadTotal { get; set; }
        protected double? DownloadChange { get; set; }
        protected string DownloadWithoutLoginTotal { get; set; }
        protected double? DownloadWithoutLoginChange { get; set; }

        protected bool IsLoadingClientLoginData { get; set; }
        protected string ClientLoginTotal { get; set; }
        protected double? ClientLoginChange { get; set; }
        protected string ClientLoginAverage { get; set; }
        protected double? ClientLoginAverageChange { get; set; }

        protected bool IsLoadingChatMessageData { get; set; }
        protected string ChatMessageTotal { get; set; }
        protected double? ChatMessageChange { get; set; }
        protected string ClientChatMessageAverage { get; set; }
        protected double? ClientChatMessageAverageChange { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            SetAllDataLoadingFlags();

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
            HandleClientLoginPeriodChange(DAILY);
            HandleChatMessagePeriodChange(DAILY);
        }

        private void SetAllDataLoadingFlags()
        {
            IsLoadingDownloadData = true;
            IsLoadingClientLoginData = true;
            IsLoadingChatMessageData = true;

            StateHasChanged();
        }

        protected async void HandleDownloadPeriodChange(string period)
        {
            IsLoadingDownloadData = true;
            StateHasChanged();

            var result = await ChartDataService.GetDownloadData(User.MasterBrokerId, period);

            DownloadTotal = result.Total.ToString("N0");
            DownloadChange = result.ChangeInTotalBetweenPeriodsPercent;

            result = await ChartDataService.GetDownloadWithoutLoginData(User.MasterBrokerId, period);
            DownloadWithoutLoginTotal = result.Total.ToString("N0");
            DownloadWithoutLoginChange = result.ChangeInTotalBetweenPeriodsPercent;

            IsLoadingDownloadData = false;
            StateHasChanged();
        }

        protected async void HandleDownloadOnClick()
        {
            NavigationManager.NavigateTo($"/charts/AppDownloads");
        }

        protected async void HandleClientLoginPeriodChange(string period)
        {
            IsLoadingClientLoginData = true;
            StateHasChanged();

            var result = await ChartDataService.GetClientLoginData(User.MasterBrokerId, StaffId, period);

            ClientLoginTotal = result.Total.ToString("N0");
            ClientLoginChange = result.ChangeInTotalBetweenPeriodsPercent;
            ClientLoginAverage = result.Average.ToString("N2");
            ClientLoginAverageChange = result.ChangeInAverageBetweenPeriodsPercent;

            IsLoadingClientLoginData = false;
            StateHasChanged();
        }

        protected async void HandleClientLoginOnClick()
        {
            NavigationManager.NavigateTo($"/charts/ClientLogins");
        }

        protected async void HandleChatMessagePeriodChange(string period)
        {
            IsLoadingChatMessageData = true;
            StateHasChanged();

            var result = await ChartDataService.GetChatMessageData(User.MasterBrokerId, StaffId, period);

            ChatMessageTotal = result.Total.ToString("N0");
            ChatMessageChange = result.ChangeInTotalBetweenPeriodsPercent;

            result = await ChartDataService.GetCustomerChatMessageAverageData(User.MasterBrokerId, StaffId, period);

            ClientChatMessageAverage = result.Average.ToString("N2");
            ClientChatMessageAverageChange = result.ChangeInAverageBetweenPeriodsPercent;

            IsLoadingChatMessageData = false;
            StateHasChanged();
        }
    }
}
