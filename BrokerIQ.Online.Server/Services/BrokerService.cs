using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BrokerIQ.Dto.Models;
using BrokerIQ.Dto.Request;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services
{
    public class BrokerService : IBrokerService
    {
        private readonly string BrokerUrl = "Broker";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;

        public BrokerService(IRequestProviderService requestProviderService, IMapper mapper)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
        }

        public async Task<Broker> GetBroker(int id, bool eagerload = false)
        {
            if (id == 0) return null;

            var answer = await this.requestProviderService.Get<BrokerDto>(this.BrokerUrl, id, eagerload);
            return this.mapper.Map<Broker>(answer);
        }

        public async Task<IEnumerable<Broker>> GetBrokers()
        {
            var answer = await this.requestProviderService.Get<IEnumerable<BrokerDto>>(this.BrokerUrl);
            return this.mapper.Map<IEnumerable<Broker>>(answer);
        }

        public async Task<IEnumerable<Broker>> GetBrokersByList(IEnumerable<int> ids)
        {
            var url = this.BrokerUrl + "/list?";
            foreach (var id in ids)
            {
                url += $"ids={id}&";
            }
            url.TrimEnd('&');
            var answer = await this.requestProviderService.Get<IEnumerable<Broker>>(url);
            return this.mapper.Map<IEnumerable<Broker>>(answer);
        }

        public async Task<Broker> UpdateBroker(Broker broker)
        {
            var mapped = mapper.Map<UpdateBrokerDto>(broker);
            var answer = await this.requestProviderService.Put<UpdateBrokerDto, BrokerDto>(this.BrokerUrl, mapped);
            return this.mapper.Map<Broker>(answer);
        }

        public async Task<Broker> AddBroker(Broker ins)
        {
            var mapped = mapper.Map<CreateBrokerDto>(ins);
            var answer = await this.requestProviderService.Post<CreateBrokerDto, BrokerDto>(this.BrokerUrl, mapped);
            return this.mapper.Map<Broker>(answer);
        }

        public async Task<bool> DeleteBroker(int id)
        {
            return await this.requestProviderService.Delete(this.BrokerUrl, id);
        }

        public async Task<BoolResponseDto> VerifyBroker(int id)
        {
            var urlToGo = this.BrokerUrl + $"/verifybroker/{id}";
            return await this.requestProviderService.Post<BoolResponseDto>(urlToGo);
        }

        public async Task<bool> ToggleService(int brokerId, int serviceId)
        {
            var urlToGo = this.BrokerUrl + $"/{brokerId}/toggleservice/{serviceId}";

            return await this.requestProviderService.Post<bool>(urlToGo);
        }
    }
}
