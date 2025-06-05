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

        public async Task<AnalyticsDataResponse> GetReferralData(int brokerId, int? staffId, string period)
        {
            var url = $"{baseUrl}/analytics/referrals?brokerId={brokerId}&period={period}";
            if (staffId.HasValue) url += $"brokerstaffid={staffId}";

            var answer = await this.requestProviderService.Get<AnalyticsDataResponse>(url);
            return answer;
        }

        public async Task<AnalyticsDataResponse> GetReferralConversionData(int brokerId, int? staffId, string period)
        {
            var url = $"{baseUrl}/analytics/referralconversion?brokerId={brokerId}&period={period}";
            if (staffId.HasValue) url += $"brokerstaffid={staffId}";

            var answer = await this.requestProviderService.Get<AnalyticsDataResponse>(url);
            return answer;
        }

    }
}
