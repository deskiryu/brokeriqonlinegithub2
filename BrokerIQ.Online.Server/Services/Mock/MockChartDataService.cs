using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Response;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services.Mock
{
    public class MockChartDataService : IChartDataService
    {
        public Task<AnalyticsDataResponse> GetDownloadData(int brokerId, string period)
        {
            throw new NotImplementedException();
        }

        public Task<AnalyticsDataResponse> GetDownloadWithoutLoginData(int brokerId, string period)
        {
            throw new NotImplementedException();
        }

        public Task<AnalyticsDataResponse> GetClientLoginData(int brokerId, int? brokerStaffId, string period)
        {
            throw new NotImplementedException();
        }

        public Task<AnalyticsDataResponse> GetClientLoginAverageData(int brokerId, int? brokerStaffId, string period)
        {
            throw new NotImplementedException();
        }

        public Task<AnalyticsDataResponse> GetChatMessageData(int brokerId, int? staffId, string period)
        {
            throw new NotImplementedException();
        }

        public Task<AnalyticsDataResponse> GetReferralData(int brokerId, int? staffId, string period)
        {
            throw new NotImplementedException();
        }

        public Task<AnalyticsDataResponse> GetReferralConversionData(int brokerId, int? staffId, string period)
        {
            throw new NotImplementedException();
        }

        public Task<AnalyticsDataResponse> GetCustomerChatMessageAverageData(int brokerId, int? staffId, string period)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<AnalyticsRiskCustomerRankingItemDto>> GetHighRiskCustomerRanking(int masterBrokerId, int? staffId, string period)
        {
            throw new NotImplementedException();
        }
    }
}
