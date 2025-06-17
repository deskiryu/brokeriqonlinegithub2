using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Response;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services
{
    public class ChartDataService : IChartDataService
    {
        private readonly string baseUrl = "Statistics";

        private readonly IRequestProviderService requestProviderService;

        public ChartDataService(IRequestProviderService requestProviderService)
        {
            this.requestProviderService = requestProviderService;
        }

        public async Task<AnalyticsDataResponse> GetDownloadData(int brokerId, string period)
        {
            var url = $"{baseUrl}/analytics/downloads?brokerId={brokerId}&period={period}";
            var answer = await this.requestProviderService.Get<AnalyticsDataResponse>(url);
            return answer;
        }

        public async Task<AnalyticsDataResponse> GetDownloadWithoutLoginData(int brokerId, string period)
        {
            var url = $"{baseUrl}/analytics/downloadswithoutlogin?brokerId={brokerId}&period={period}";
            var answer = await this.requestProviderService.Get<AnalyticsDataResponse>(url);
            return answer;
        }

        public async Task<AnalyticsDataResponse> GetClientLoginData(int brokerId, int? staffId, string period)
        {
            var url = $"{baseUrl}/analytics/customerlogins?brokerId={brokerId}&period={period}";
            if (staffId.HasValue) url += $"brokerstaffid={staffId}";

            var answer = await this.requestProviderService.Get<AnalyticsDataResponse>(url);
            return answer;
        }

        public async Task<AnalyticsDataResponse> GetClientLoginAverageData(int brokerId, int? staffId, string period)
        {
            var url = $"{baseUrl}/analytics/customerloginaverage?brokerId={brokerId}&period={period}";
            if (staffId.HasValue) url += $"brokerstaffid={staffId}";

            var answer = await this.requestProviderService.Get<AnalyticsDataResponse>(url);
            return answer;
        }

        public async Task<AnalyticsDataResponse> GetChatMessageData(int brokerId, int? staffId, string period)
        {
            var url = $"{baseUrl}/analytics/chatmessagevolume?brokerId={brokerId}&period={period}";
            if (staffId.HasValue) url += $"brokerstaffid={staffId}";

            var answer = await this.requestProviderService.Get<AnalyticsDataResponse>(url);
            return answer;
        }

        public async Task<AnalyticsDataResponse> GetCustomerChatMessageAverageData(int brokerId, int? staffId, string period)
        {
            var url = $"{baseUrl}/analytics/customerchatmessageaverage?brokerId={brokerId}&period={period}";
            if (staffId.HasValue) url += $"brokerstaffid={staffId}";

            var answer = await this.requestProviderService.Get<AnalyticsDataResponse>(url);
            return answer;
        }

        public async Task<IEnumerable<AnalyticsRiskCustomerRankingItemDto>> GetHighRiskCustomerRanking(int brokerId, int? staffId, string period)
        {
            var url = $"{baseUrl}/analytics/highriskcustomerranking?brokerId={brokerId}&period={period}";
            if (staffId.HasValue) url += $"brokerstaffid={staffId}";

            return await this.requestProviderService.Get<IEnumerable<AnalyticsRiskCustomerRankingItemDto>>(url);
        }

        public async Task<AnalyticsDataResponse> GetReferralData(int brokerId, int? staffId, string period)
        {
            var url = $"{baseUrl}/analytics/customerreferrals?brokerId={brokerId}&period={period}";
            if (staffId.HasValue) url += $"brokerstaffid={staffId}";

            var answer = await this.requestProviderService.Get<AnalyticsDataResponse>(url);
            return answer;
        }

        public async Task<AnalyticsDataResponse> GetReferralConversionData(int brokerId, int? staffId, string period)
        {
            var url = $"{baseUrl}/analytics/convertedreferrals?brokerId={brokerId}&period={period}";
            if (staffId.HasValue) url += $"brokerstaffid={staffId}";

            var answer = await this.requestProviderService.Get<AnalyticsDataResponse>(url);
            return answer;
        }

        public async Task<AnalyticsDataResponse> GetInsuranceCustomersData(int brokerId, int? staffId, string period)
        {
            var url = $"{baseUrl}/analytics/insurancecustomers?brokerId={brokerId}&period={period}";
            if (staffId.HasValue) url += $"brokerstaffid={staffId}";

            var answer = await this.requestProviderService.Get<AnalyticsDataResponse>(url);
            return answer;
        }

        public async Task<AnalyticsDataResponse> GetMortgageCustomersData(int brokerId, int? staffId, string period)
        {
            var url = $"{baseUrl}/analytics/mortgagecustomers?brokerId={brokerId}&period={period}";
            if (staffId.HasValue) url += $"brokerstaffid={staffId}";

            var answer = await this.requestProviderService.Get<AnalyticsDataResponse>(url);
            return answer;
        }

        public async Task<AnalyticsDataResponse> GetWealthCustomersData(int brokerId, int? staffId, string period)
        {
            var url = $"{baseUrl}/analytics/wealthcustomers?brokerId={brokerId}&period={period}";
            if (staffId.HasValue) url += $"brokerstaffid={staffId}";

            var answer = await this.requestProviderService.Get<AnalyticsDataResponse>(url);
            return answer;
        }

        public async Task<AnalyticsDataResponse> GetNoProductCustomersData(int brokerId, int? staffId, string period)
        {
            var url = $"{baseUrl}/analytics/noproductcustomers?brokerId={brokerId}&period={period}";
            if (staffId.HasValue) url += $"brokerstaffid={staffId}";

            var answer = await this.requestProviderService.Get<AnalyticsDataResponse>(url);
            return answer;
        }
    }
}
