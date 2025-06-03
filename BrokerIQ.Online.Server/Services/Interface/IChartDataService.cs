using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Model.Statistics;
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




        Task<AppConversionDto> GetInvitesSentAndConverted(int brokerId);

        Task<List<(DateTime, int, int, int, int, int)>> GetInvitesSentAndConvertedSequence(int brokerId);

        List<(DateTime, int, int)> GetNotificationsSent();

        public List<(string, int)> GetBrokersPerYear();

        public List<(string, int)> GetAppUsersPerYear();

        public List<(string, int)> GetPremiumsManagedPerYear();

        Task<List<(string, int)>> GetTotalLogins(int brokerId);
    }
}
