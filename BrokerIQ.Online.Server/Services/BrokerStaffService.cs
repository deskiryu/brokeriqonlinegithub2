using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Server.Services
{
    public class BrokerStaffService : IBrokerStaffService
    {
        private readonly string BrokerUrl = "BrokerStaff";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;

        public BrokerStaffService(IRequestProviderService requestProviderService, IMapper mapper)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
        }
        public async Task<bool> DeleteBrokerStaff(int id)
        {
            var url = this.BrokerUrl + $"?id={id}";
            return await this.requestProviderService.Delete(url);
        }

        public async Task<BrokerStaff> GetBrokerStaff(int id)
        {
            var url = this.BrokerUrl + $"/id?id={id}";
            var answer = await this.requestProviderService.Get<BrokerStaffDto>(url);
            return this.mapper.Map<BrokerStaff>(answer);
        }

        public async Task<BrokerStaff> UpdateBrokerStaff(BrokerStaff brokerStaff)
        {
            var mapped = mapper.Map<UpdateBrokerStaffDto>(brokerStaff);
            var answer = await this.requestProviderService.Put<UpdateBrokerStaffDto, BrokerStaffDto>(this.BrokerUrl, mapped);
            return this.mapper.Map<BrokerStaff>(answer);
        }

        public async Task<IEnumerable<BrokerStaff>> GetBrokerStaff()
        {
            var answer = await this.requestProviderService.Get<IEnumerable<BrokerStaffDto>>(this.BrokerUrl);
            return this.mapper.Map<IEnumerable<BrokerStaff>>(answer);
        }

        public async Task<IEnumerable<BrokerStaff>> GetBrokerStaffbyBrokerId(int id)
        {
            var answer = await this.requestProviderService.Get<IEnumerable<BrokerStaffDto>>(this.BrokerUrl+"/broker" + $"?brokerid={id}");
            var mapped = this.mapper.Map<IEnumerable<BrokerStaff>>(answer);
            return mapped;
        }
    }
}
