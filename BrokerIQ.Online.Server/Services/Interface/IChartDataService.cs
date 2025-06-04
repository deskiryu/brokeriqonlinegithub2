using System.Threading.Tasks;
using BrokerIQ.Dto.Response;

namespace BrokerIQ.Online.Services.Interface
{
    public interface IChartDataService
    {
        Task<AnalyticsDataResponse> GetDownloadData(int brokerId, string period);

        Task<AnalyticsDataResponse> GetClientLoginData(int brokerId, int? brokerStaffId, string period);

        Task<AnalyticsDataResponse> GetClientLoginAverageData(int brokerId, int? brokerStaffId, string period);

        Task<AnalyticsDataResponse> GetReferralData(int brokerId, int? staffId, string period);

        Task<AnalyticsDataResponse> GetReferralConversionData(int brokerId, int? staffId, string period);

        Task<AnalyticsDataResponse> GetChatMessageData(int brokerId, int? staffId, string period);
    }
}
