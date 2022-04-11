using ReviewIt.Dto.Model.Statistics;
using ReviewIt.Web.Services.Abstract;
using ReviewIt.Web.Services.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReviewIt.Web.Services
{
    public class ChartDataService : IChartDataService
    {
        private readonly string statsUrl = "Statistics";
        private readonly IRequestProviderService requestProviderService;
        private readonly IAccountService accountService;

        public ChartDataService(IRequestProviderService requestProviderService, IAccountService accountService)
        {
            this.requestProviderService = requestProviderService;
            this.accountService = accountService;
        }

        public async Task<AppConversionDto> GetInvitesSentAndConverted(int brokerId)
        {
#if FALSE
            var response = new List<(DateTime, int, int)>
            {
                (DateTime.Now.AddDays(-10),50,12),
                (DateTime.Now.AddDays(-9),100,22),
                (DateTime.Now.AddDays(-8),75,3),
                (DateTime.Now.AddDays(-7),25,14),
                (DateTime.Now.AddDays(-6),50,11),
                (DateTime.Now.AddDays(-5),100,34),
                (DateTime.Now.AddDays(-4),50,12),
                (DateTime.Now.AddDays(-3),20,9),
                (DateTime.Now.AddDays(-2),10,4),
                (DateTime.Now.AddDays(-1),30,15),
                (DateTime.Now.AddDays(0),50,12)
            };
            return response;
#else
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.statsUrl + $"/{brokerId}";
            var answer = await this.requestProviderService.Get<AppConversionDto>(url);
            return (answer);
#endif
        }
        public async Task<List<(DateTime, int, int, int)>> GetInvitesSentAndConvertedSequence(int brokerId)
        {
#if FALSE
            var response = new List<(DateTime, int, int)>
            {
                (DateTime.Now.AddDays(-10),50,12),
                (DateTime.Now.AddDays(-9),100,22),
                (DateTime.Now.AddDays(-8),75,3),
                (DateTime.Now.AddDays(-7),25,14),
                (DateTime.Now.AddDays(-6),50,11),
                (DateTime.Now.AddDays(-5),100,34),
                (DateTime.Now.AddDays(-4),50,12),
                (DateTime.Now.AddDays(-3),20,9),
                (DateTime.Now.AddDays(-2),10,4),
                (DateTime.Now.AddDays(-1),30,15),
                (DateTime.Now.AddDays(0),50,12)
            };
            return response;
#else
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.statsUrl + $"/sequence/{brokerId}?sequenceType=1&noelements=10";
            var answer = await this.requestProviderService.Get<AppConversionSequenceDto>(url);
            var response = new List<(DateTime, int, int, int)>
            {
                (DateTime.Now.AddDays( -9),answer.Sequence[9].TotalEmailInvites,answer.Sequence[9].TotalConvertedLogins,answer.Sequence[9].TotalUnConvertedLogins),
                (DateTime.Now.AddDays( -8),answer.Sequence[8].TotalEmailInvites,answer.Sequence[8].TotalConvertedLogins,answer.Sequence[8].TotalUnConvertedLogins),
                (DateTime.Now.AddDays( -7),answer.Sequence[7].TotalEmailInvites,answer.Sequence[7].TotalConvertedLogins,answer.Sequence[7].TotalUnConvertedLogins),
                (DateTime.Now.AddDays( -6),answer.Sequence[6].TotalEmailInvites,answer.Sequence[6].TotalConvertedLogins,answer.Sequence[6].TotalUnConvertedLogins),
                (DateTime.Now.AddDays( -5),answer.Sequence[5].TotalEmailInvites,answer.Sequence[5].TotalConvertedLogins,answer.Sequence[5].TotalUnConvertedLogins),
                (DateTime.Now.AddDays( -4),answer.Sequence[4].TotalEmailInvites,answer.Sequence[4].TotalConvertedLogins,answer.Sequence[4].TotalUnConvertedLogins),
                (DateTime.Now.AddDays( -3),answer.Sequence[3].TotalEmailInvites,answer.Sequence[3].TotalConvertedLogins,answer.Sequence[3].TotalUnConvertedLogins),
                (DateTime.Now.AddDays( -2),answer.Sequence[2].TotalEmailInvites,answer.Sequence[2].TotalConvertedLogins,answer.Sequence[2].TotalUnConvertedLogins),
                (DateTime.Now.AddDays( -1),answer.Sequence[1].TotalEmailInvites,answer.Sequence[1].TotalConvertedLogins,answer.Sequence[1].TotalUnConvertedLogins),
                (DateTime.Now.AddDays(  0),answer.Sequence[0].TotalEmailInvites,answer.Sequence[0].TotalConvertedLogins,answer.Sequence[0].TotalUnConvertedLogins),
            };
            return (response);
#endif
        }

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

        public async Task<List<(string, int)>> GetTotalLogins(int brokerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.statsUrl + $"/logins/{brokerId}?noelements=10";
            var answer = await this.requestProviderService.Get<AppLoginsDto>(url);
            var response = new List<(string, int)>
            {
                ("Once",answer.NoLogins[0]),
                ("Twice",answer.NoLogins[1]),
                ("Three times",answer.NoLogins[2]),
                ("Four times",answer.NoLogins[3]),
                ("Five+ times",answer.NoLogins[4]),
            };
            return (response);
        }
    }
}
