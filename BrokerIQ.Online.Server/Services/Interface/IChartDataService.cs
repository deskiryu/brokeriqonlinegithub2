using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Response;

namespace BrokerIQ.Online.Services.Interface
{
    public interface IChartDataService
    {
        Task<AnalyticsDataResponse> GetDownloadData(int brokerId, string period);

        Task<AnalyticsDataResponse> GetDownloadWithoutLoginData(int brokerId, string period);

        Task<AnalyticsDataResponse> GetClientLoginData(int brokerId, int? brokerStaffId, string period);

        Task<AnalyticsDataResponse> GetClientLoginAverageData(int brokerId, int? brokerStaffId, string period);

        Task<AnalyticsDataResponse> GetChatMessageData(int brokerId, int? staffId, string period);

        Task<AnalyticsDataResponse> GetCustomerChatMessageAverageData(int brokerId, int? staffId, string period);

        Task<IEnumerable<AnalyticsRiskCustomerRankingItemDto>> GetHighRiskCustomerRanking(int masterBrokerId, int? staffId, string period);

        Task<AnalyticsDataResponse> GetReferralData(int brokerId, int? staffId, string period);

        Task<AnalyticsDataResponse> GetReferralConversionData(int brokerId, int? staffId, string period);

        Task<AnalyticsDataResponse> GetInsuranceCustomersData(int brokerId, int? staffId, string period);

        Task<AnalyticsDataResponse> GetMortgageCustomersData(int brokerId, int? staffId, string period);

        Task<AnalyticsDataResponse> GetWealthCustomersData(int brokerId, int? staffId, string period);

        Task<AnalyticsDataResponse> GetNoProductCustomersData(int brokerId, int? staffId, string period);

        Task<AnalyticsDataResponse> GetVideoEngagementData(int brokerId, int? staffId, string period);

        Task<AnalyticsDataResponse> GetAudioEngagementData(int brokerId, int? staffId, string period);
    }
}
