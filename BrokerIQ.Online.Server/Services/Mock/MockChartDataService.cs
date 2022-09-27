using BrokerIQ.Dto.Model.Statistics;
using BrokerIQ.Online.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Services.Mock
{
    public class MockChartDataService : IChartDataService
    {
 
        public List<(DateTime, int, int)> GetNotificationsSent()
        {
            //Date, ios, android
            var response = new List<(DateTime, int, int)>
            {
                (DateTime.Now.AddDays(-10),50,60),
                (DateTime.Now.AddDays(-9),95,27),
                (DateTime.Now.AddDays(-8),25,37),
                (DateTime.Now.AddDays(-7),15,40),
                (DateTime.Now.AddDays(-6),50,50),
                (DateTime.Now.AddDays(-5),15,34),
                (DateTime.Now.AddDays(-4),50,45),
                (DateTime.Now.AddDays(-3),30,50),
                (DateTime.Now.AddDays(-2),15,34),
                (DateTime.Now.AddDays(-1),34,20),
                (DateTime.Now.AddDays(0),50,60)
            };
            return response;
        }

        public List<(string, int)> GetBrokersPerYear()
        {
            var response = new List<(string, int)>
            {
                ("Year 1",500),
                ("Year 2",1000),
                ("Year 3",1500),
                ("Year 4",3000),
                ("Year 5",4500),
            };
            return response;
        }

        public List<(string, int)> GetAppUsersPerYear()
        {
            var response = new List<(string, int)>
            {
                ("Year 1",500000),
                ("Year 2",1000000),
                ("Year 3",1500000),
                ("Year 4",3000000),
                ("Year 5",4500000),
            };
            return response;
        }

        public List<(string, int)> GetPremiumsManagedPerYear()
        {
            var response = new List<(string, int)>
            {
                ("Year 1",750),
                ("Year 2",1500),
                ("Year 3",2250),
                ("Year 4",4500),
                ("Year 5",6750),
            };
            return response;
        }

        Task<AppConversionDto> IChartDataService.GetInvitesSentAndConverted(int brokerId)
        {
            throw new NotImplementedException();
        }


        public Task<List<(string, int)>> GetTotalLogins(int brokerId)
        {
            throw new NotImplementedException();
        }

        public Task<List<(DateTime, int, int, int, int, int)>> GetInvitesSentAndConvertedSequence(int brokerId)
        {
            throw new NotImplementedException();
        }
    }
}
