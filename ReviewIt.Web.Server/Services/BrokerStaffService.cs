using AutoMapper;
using BrokerIQ.Dto.Models;
using ReviewIt.Web.Models;
using ReviewIt.Web.Services.Abstract;
using ReviewIt.Web.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewIt.Web.Server.Services
{
    public class BrokerStaffService : IBrokerStaffService
    {
        private readonly string BrokerUrl = "BrokerStaff";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;
        private readonly IAccountService accountService;

        public BrokerStaffService(IRequestProviderService requestProviderService, IMapper mapper, IAccountService accountService)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
            this.accountService = accountService;
        }
        public async Task<bool> DeleteBrokerStaff(int id)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.BrokerUrl + $"?id={id}";
            return await this.requestProviderService.Delete(url);
        }

        public async Task<BrokerStaff> GetBrokerStaff(int id)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.BrokerUrl + $"/id?id={id}";
            var answer = await this.requestProviderService.Get<BrokerStaffDto>(url);
            return this.mapper.Map<BrokerStaff>(answer);
        }

        public async Task<BrokerStaff> UpdateBrokerStaff(BrokerStaff brokerStaff)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var mapped = mapper.Map<UpdateBrokerStaffDto>(brokerStaff);
            var answer = await this.requestProviderService.Put<UpdateBrokerStaffDto, BrokerStaffDto>(this.BrokerUrl, mapped);
            return this.mapper.Map<BrokerStaff>(answer);
        }

        public async Task<IEnumerable<BrokerStaff>> GetBrokerStaff()
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var answer = await this.requestProviderService.Get<IEnumerable<BrokerStaffDto>>(this.BrokerUrl);
            return this.mapper.Map<IEnumerable<BrokerStaff>>(answer);
        }

        public async Task<IEnumerable<BrokerStaff>> GetBrokerStaffbyBrokerId(int id)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var answer = await this.requestProviderService.Get<IEnumerable<BrokerStaffDto>>(this.BrokerUrl+"/broker" + $"?brokerid={id}");
            var mapped = this.mapper.Map<IEnumerable<BrokerStaff>>(answer);
            return mapped;
        }
    }
}
