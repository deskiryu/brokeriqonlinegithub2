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

        protected int _BrokerId;

        protected Broker Broker { get; set; }

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

        protected bool IsLoadingVideoData { get; set; }
        protected string VideoSentTotal { get; set; }
        protected double? VideoSentChange { get; set; }
        protected string VideoViewedTotal { get; set; }
        protected double? VideoViewedChange { get; set; }
        protected string AverageVideoTimeTotal { get; set; }
        protected double? AverageVideoTimeChange { get; set; }

        protected bool IsLoadingAudioData { get; set; }
        protected string AudioSentTotal { get; set; }
        protected double? AudioSentChange { get; set; }
        protected string AudioViewedTotal { get; set; }
        protected double? AudioViewedChange { get; set; }
        protected string AverageAudioTimeTotal { get; set; }
        protected double? AverageAudioTimeChange { get; set; }

        public bool IsAdmin { get; set; }

        public bool IsMinorAdmin { get; set; }

        protected bool IsLoadingProductData { get; set; }
        protected string ProductsTotal { get; set; }
        protected double? ProductsChange { get; set; }
        protected List<ChartSeries> ProductsSeries = new List<ChartSeries>();
        protected string[] ProductsLabels = Array.Empty<string>();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            SetAllDataLoadingFlags();

            User = await AccountService.GetUser();

            if (User.IsAdmin || User.IsMinorAdmin)
            {
                Int32.TryParse(BrokerId, out _BrokerId);
            }
            else
            {
                _BrokerId = User.MasterBrokerId;
            }

            Broker = await BrokerService.GetBroker(_BrokerId, true);

            if (User.IsBrokerStaff)
            {
                StaffId = User.StaffBrokerId;
            }

            IsAdmin = User.IsAdmin;
            IsMinorAdmin = User.IsMinorAdmin;

            HandleDownloadPeriodChange(DAILY);
            HandleClientLoginPeriodChange(DAILY);
            HandleChatMessagePeriodChange(DAILY);
            HandleReferralPeriodChange(DAILY);
            HandleReferralSplitPeriodChange(DAILY);
            HandleCustomerRiskRatingPeriodChange(DAILY);
            HandleProductsPeriodChange(DAILY);
            HandleVideoPeriodChange(DAILY);
            HandleAudioPeriodChange(DAILY);
        }

        private void SetAllDataLoadingFlags()
        {
            IsLoadingDownloadData = true;
            IsLoadingClientLoginData = true;
            IsLoadingChatMessageData = true;
            IsLoadingReferralData = true;
            IsLoadingReferralSplitData = true;
            IsLoadingCustomerRiskData = true;
            IsLoadingProductData = true;
            IsLoadingVideoData = true;
            IsLoadingAudioData = true;

            StateHasChanged();
        }

        protected async void HandleDownloadPeriodChange(string period)
        {
            IsLoadingDownloadData = true;
            StateHasChanged();

            var result = await ChartDataService.GetDownloadData(_BrokerId, period);

            DownloadTotal = result.Total[DEFAULT_SERIES_KEY].Value.ToString("N0");
            DownloadChange = result.ChangeInTotalBetweenPeriodsPercent[DEFAULT_SERIES_KEY].Value;

            result = await ChartDataService.GetDownloadWithoutLoginData(_BrokerId, period);
            DownloadWithoutLoginTotal = result.Total[DEFAULT_SERIES_KEY].Value.ToString("N0");
            DownloadWithoutLoginChange = result.ChangeInTotalBetweenPeriodsPercent[DEFAULT_SERIES_KEY].Value;

            IsLoadingDownloadData = false;
            StateHasChanged();
        }

        protected async void HandleDownloadOnClick()
        {
            if (IsAdmin || IsMinorAdmin)
            {
                NavigationManager.NavigateTo($"/charts/AppDownloads/{_BrokerId}");
            }
            else
            {
                NavigationManager.NavigateTo($"/charts/AppDownloads/0");
            }

        }

        protected async void HandleClientLoginPeriodChange(string period)
        {
            IsLoadingClientLoginData = true;
            StateHasChanged();

            var result = await ChartDataService.GetClientLoginData(_BrokerId, StaffId, period);

            ClientLoginTotal = result.Total[DEFAULT_SERIES_KEY].Value.ToString("N0");
            ClientLoginChange = result.ChangeInTotalBetweenPeriodsPercent[DEFAULT_SERIES_KEY].Value;
            ClientLoginAverage = result.Average[DEFAULT_SERIES_KEY].HasValue ? result.Average[DEFAULT_SERIES_KEY].Value.ToString("N2") : string.Empty;
            ClientLoginAverageChange = result.ChangeInAverageBetweenPeriodsPercent[DEFAULT_SERIES_KEY].HasValue ?
                result.ChangeInAverageBetweenPeriodsPercent[DEFAULT_SERIES_KEY].Value : 0;

            IsLoadingClientLoginData = false;
            StateHasChanged();
        }

        protected async void HandleClientLoginOnClick()
        {
            if (IsAdmin || IsMinorAdmin)
            {
                NavigationManager.NavigateTo($"/charts/ClientLogins/{_BrokerId}");
            }
            else
            {
                NavigationManager.NavigateTo($"/charts/ClientLogins/0");
            }
        }

        protected async void HandleChatMessagePeriodChange(string period)
        {
            IsLoadingChatMessageData = true;
            StateHasChanged();

            var result = await ChartDataService.GetChatMessageData(_BrokerId, StaffId, period);

            ChatMessageTotal = result.Total[DEFAULT_SERIES_KEY].Value.ToString("N0");
            ChatMessageChange = result.ChangeInTotalBetweenPeriodsPercent[DEFAULT_SERIES_KEY].Value;

            result = await ChartDataService.GetCustomerChatMessageAverageData(_BrokerId, StaffId, period);

            ClientChatMessageAverage = result.Average[DEFAULT_SERIES_KEY].HasValue ? result.Average[DEFAULT_SERIES_KEY].Value.ToString("N2") : string.Empty;
            ClientChatMessageAverageChange = result.ChangeInAverageBetweenPeriodsPercent[DEFAULT_SERIES_KEY].HasValue ?
                result.ChangeInAverageBetweenPeriodsPercent[DEFAULT_SERIES_KEY].Value : 0;

            IsLoadingChatMessageData = false;
            StateHasChanged();
        }

        protected async void HandleCustomerRiskRatingPeriodChange(string period)
        {
            IsLoadingCustomerRiskData = true;
            StateHasChanged();

            CustomerRankingList = await ChartDataService.GetHighRiskCustomerRanking(_BrokerId, StaffId, period);

            IsLoadingCustomerRiskData = false;
            StateHasChanged();
        }

        protected async void HandleReferralPeriodChange(string period)
        {
            IsLoadingReferralData = true;
            StateHasChanged();

            ReferralSeries = new List<ChartSeries>();

            var result = await ChartDataService.GetReferralData(_BrokerId, StaffId, period);

            ReferralTotal = result.Total[DEFAULT_SERIES_KEY].Value.ToString("N0");
            ReferralChange = result.ChangeInTotalBetweenPeriodsPercent[DEFAULT_SERIES_KEY].Value;
            ReferralSeries.Add(GetChartSeriesFrom(result, "Referrals"));
            ReferralLabels = GetLabelsFrom(result);

            result = await ChartDataService.GetReferralConversionData(_BrokerId, StaffId, period);

            ConversionTotal = result.Total[DEFAULT_SERIES_KEY].Value.ToString("N2");
            ConversionChange = result.ChangeInAverageBetweenPeriodsPercent[DEFAULT_SERIES_KEY].HasValue ?
                result.ChangeInAverageBetweenPeriodsPercent[DEFAULT_SERIES_KEY].Value : 0;
            ReferralSeries.Add(GetChartSeriesFrom(result, "Conversion"));

            IsLoadingReferralData = false;
            StateHasChanged();
        }

        private static ChartSeries GetChartSeriesFrom(AnalyticsDataResponse result, string seriesLabel)
        {
            var series = result.Items.GroupBy(i => i.PeriodLabel).Select(g => g.Sum(i => i.Value)).ToArray();

            return new ChartSeries() { Name = seriesLabel, Data = series };
        }

        private static string[] GetLabelsFrom(AnalyticsDataResponse result)
        {
            var categories = result.Items.Select(i => i.Category).Distinct().ToArray();

            // get the labels from just one of the categories
            return result.Items.Where(i => i.Category == categories[0]).Select(i => i.PeriodLabel).ToArray();
        }

        protected async void HandleReferralSplitPeriodChange(string period)
        {
            IsLoadingReferralSplitData = true;
            StateHasChanged();

            var referrals = await ChartDataService.GetReferralData(_BrokerId, StaffId, period);
            var conversions = await ChartDataService.GetReferralConversionData(_BrokerId, StaffId, period);

            ReferralSplitData = new double[] { referrals.Total[DEFAULT_SERIES_KEY].Value, conversions.Total[DEFAULT_SERIES_KEY].Value };
            ReferralSplitLabels = new string[] { "Referrals", "Conversions" };
            ReferralConvertRate = (ReferralSplitData[1] / ReferralSplitData[0]).ToString("P0");

            IsLoadingReferralSplitData = false;
            StateHasChanged();
        }

        protected async void HandleProductsPeriodChange(string period)
        {
            IsLoadingProductData = true;
            StateHasChanged();

            ProductsSeries = new List<ChartSeries>();
            ProductsLabels = Array.Empty<string>();
            var allProductsTotal = 0.0;
            var allPreviousProductsTotal = 0.0;

            if (Broker.ProvidesBusinessInsuranceServices || Broker.ProvidesPersonalInsuranceServices)
            {
                var result = await ChartDataService.GetInsuranceCustomersData(User.MasterBrokerId, StaffId, period);

                allProductsTotal += result.Total.Sum(t=> t.Value).Value;
                allPreviousProductsTotal += result.PreviousPeriodTotal.Sum(t => t.Value).Value;

                if (!ProductsLabels.Any())
                {
                    ProductsLabels = GetLabelsFrom(result);
                }

                ProductsSeries.Add(GetChartSeriesFrom(result, "Insurance"));
            }

            if (Broker.ProvidesMortgageServices)
            {
                var result = await ChartDataService.GetMortgageCustomersData(User.MasterBrokerId, StaffId, period);

                allProductsTotal += result.Total.Sum(t => t.Value).Value;
                allPreviousProductsTotal += result.PreviousPeriodTotal.Sum(t => t.Value).Value;

                if (!ProductsLabels.Any())
                {
                    ProductsLabels = GetLabelsFrom(result);
                }

                ProductsSeries.Add(GetChartSeriesFrom(result, "Mortgage"));
            }

            if (Broker.ProvidesWealthServices)
            {
                var result = await ChartDataService.GetWealthCustomersData(User.MasterBrokerId, StaffId, period);

                allProductsTotal += result.Total.Sum(t => t.Value).Value;
                allPreviousProductsTotal += result.PreviousPeriodTotal.Sum(t => t.Value).Value;

                if (!ProductsLabels.Any())
                {
                    ProductsLabels = GetLabelsFrom(result);
                }

                ProductsSeries.Add(GetChartSeriesFrom(result, "Wealth"));
            }

            ProductsTotal = allProductsTotal.ToString("N0");
            if (allProductsTotal > 0)
            {
                ProductsChange = (allProductsTotal - allPreviousProductsTotal) / allProductsTotal;
            }

            var noProducts = await ChartDataService.GetNoProductCustomersData(User.MasterBrokerId, StaffId, period);

            ProductsSeries.Add(GetChartSeriesFrom(noProducts, "No Products"));

            IsLoadingProductData = false;
            StateHasChanged();
        }

        protected async void HandleVideoPeriodChange(string period)
        {
            IsLoadingVideoData = true;
            StateHasChanged();

            var result = await ChartDataService.GetVideoEngagementData(_BrokerId, StaffId, period);

            VideoSentTotal = result.Total["Sent"].Value.ToString("N0");
            VideoSentChange = result.ChangeInTotalBetweenPeriodsPercent["Sent"].Value;

            VideoViewedTotal = result.Total["Viewed"].Value.ToString("N0");
            VideoViewedChange = result.ChangeInTotalBetweenPeriodsPercent["Viewed"].Value;

            AverageVideoTimeTotal = result.Total["AvgTimePlayed"].Value.ToString("N0");
            AverageVideoTimeChange = result.ChangeInTotalBetweenPeriodsPercent["AvgTimePlayed"].Value;

            IsLoadingVideoData = false;
            StateHasChanged();
        }

        public static double? ChangeInTotalBetweenPeriodsPercent(double total, double previousTotal)
        {
            if (previousTotal == 0) return null;

            return (total - previousTotal) / previousTotal;
        }

        protected async void HandleAudioPeriodChange(string period)
        {
            IsLoadingAudioData = true;
            StateHasChanged();

            var result = await ChartDataService.GetAudioEngagementData(_BrokerId, StaffId, period);

            AudioSentTotal = result.Total["Sent"].Value.ToString("N0");
            AudioSentChange = result.ChangeInTotalBetweenPeriodsPercent["Sent"].Value;

            AudioViewedTotal = result.Total["Viewed"].Value.ToString("N0");
            AudioViewedChange = result.ChangeInTotalBetweenPeriodsPercent["Viewed"].Value;

            AverageAudioTimeTotal = result.Total["AvgTimePlayed"].Value.ToString("N0");
            AverageAudioTimeChange = result.ChangeInTotalBetweenPeriodsPercent["AvgTimePlayed"].Value;

            IsLoadingAudioData = false;
            StateHasChanged();
        }
    }
}
