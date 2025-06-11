using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Dto.Response;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Pages.Charts.Components;
using BrokerIQ.Online.Services.Interface;

using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Pages
{
    public class DashboardBase : BIQDashboardComponent
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

        protected bool IsLoadingCustomerRiskData { get; set; }
        protected IEnumerable<AnalyticsRiskCustomerRankingItemDto> CustomerRankingList { get; set; } = Array.Empty<AnalyticsRiskCustomerRankingItemDto>();

        protected bool IsLoadingReferralData { get; set; }
        protected string ReferralTotal { get; set; }
        protected double? ReferralChange { get; set; }
        protected string ConversionTotal { get; set; }
        protected double? ConversionChange { get; set; }
        protected List<ChartSeries> ReferralSeries = new List<ChartSeries>();
        protected string[] ReferralLabels = Array.Empty<string>();

        protected bool IsLoadingReferralSplitData { get; set; }
        protected double[] ReferralSplitData = Array.Empty<double>();
        protected string[] ReferralSplitLabels = Array.Empty<string>();
        protected string ReferralConvertRate = string.Empty;

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
            HandleReferralPeriodChange(DAILY);
            HandleReferralSplitPeriodChange(DAILY);
            HandleCustomerRiskRatingPeriodChange(DAILY);
        }

        private void SetAllDataLoadingFlags()
        {
            IsLoadingDownloadData = true;
            IsLoadingClientLoginData = true;
            IsLoadingChatMessageData = true;
            IsLoadingReferralData = true;
            IsLoadingReferralSplitData = true;
            IsLoadingCustomerRiskData = true;

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
            ClientLoginAverage = result.Average.HasValue ? result.Average.Value.ToString("N2") : string.Empty;
            ClientLoginAverageChange = result.ChangeInAverageBetweenPeriodsPercent.HasValue ? result.ChangeInAverageBetweenPeriodsPercent : 0;

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

            ClientChatMessageAverage = result.Average.HasValue ? result.Average.Value.ToString("N2") : string.Empty;
            ClientChatMessageAverageChange = result.ChangeInAverageBetweenPeriodsPercent.HasValue ? result.ChangeInAverageBetweenPeriodsPercent : 0;

            IsLoadingChatMessageData = false;
            StateHasChanged();
        }

        protected async void HandleCustomerRiskRatingPeriodChange(string period)
        {
            IsLoadingCustomerRiskData = true;
            StateHasChanged();

            CustomerRankingList = await ChartDataService.GetHighRiskCustomerRanking(User.MasterBrokerId, StaffId, period);

            IsLoadingCustomerRiskData = false;
            StateHasChanged();
        }

        protected async void HandleReferralPeriodChange(string period)
        {
            IsLoadingReferralData = true;
            StateHasChanged();

            ReferralSeries = new List<ChartSeries>();
            
            var result = await ChartDataService.GetReferralData(User.MasterBrokerId, StaffId, period);

            ReferralTotal = result.Total.ToString("N0");
            ReferralChange = result.ChangeInTotalBetweenPeriodsPercent;
            ReferralSeries.Add(new ChartSeries() { Name = "Referrals", Data = result.Items.Select(i => i.Value).ToArray() });
            ReferralLabels = result.Items.Select(i => i.Label).ToArray();

            result = await ChartDataService.GetReferralConversionData(User.MasterBrokerId, StaffId, period);

            ConversionTotal = result.Total.ToString("N2");
            ConversionChange = result.ChangeInAverageBetweenPeriodsPercent.HasValue ? result.ChangeInAverageBetweenPeriodsPercent : 0;
            ReferralSeries.Add(new ChartSeries() { Name = "Conversion", Data = result.Items.Select(i => i.Value).ToArray() });

            IsLoadingReferralData = false;
            StateHasChanged();
        }

        protected async void HandleReferralSplitPeriodChange(string period)
        {
            IsLoadingReferralSplitData = true;
            StateHasChanged();

            var referrals = await ChartDataService.GetReferralData(User.MasterBrokerId, StaffId, period);
            var conversions = await ChartDataService.GetReferralConversionData(User.MasterBrokerId, StaffId, period);

            ReferralSplitData = new double[] { referrals.Total, conversions.Total };
            ReferralSplitLabels = new string[] { "Referrals", "Conversions" };
            ReferralConvertRate = (ReferralSplitData[1] / ReferralSplitData[0]).ToString("P0");

            IsLoadingReferralSplitData = false;
            StateHasChanged();
        }

    }
}
