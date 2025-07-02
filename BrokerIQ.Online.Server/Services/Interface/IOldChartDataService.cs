using BrokerIQ.Dto.Model.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Services.Interface
{
    public interface IOldChartDataService
    {
        Task<AppConversionDto> GetInvitesSentAndConverted(int brokerId);

        Task<List<(DateTime, int, int, int, int, int)>> GetInvitesSentAndConvertedSequence(int brokerId);

        List<(DateTime, int, int)> GetNotificationsSent();

        public List<(string, int)> GetBrokersPerYear();

        public List<(string, int)> GetAppUsersPerYear();

        public List<(string, int)> GetPremiumsManagedPerYear();

        Task<List<(string, int)>> GetTotalLogins(int brokerId);
    }
}
