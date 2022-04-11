using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReviewIt.Web.Services
{
    using System.Net.Http;
    using System.Text.Json;
    using Abstract;
    using AppSettings;
    using AutoMapper;
    using Dto.Models;
    using Interface;
    using Mapper;
    using Microsoft.Extensions.Options;
    using Models;
    using ReviewIt.Dto.Request;
    using ReviewIt.Web.Services.Interface;

    public class BrokerService : IBrokerService
    {
        private readonly string BrokerUrl = "Broker";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;
        private readonly IAccountService accountService;

        public BrokerService(IRequestProviderService requestProviderService, IMapper mapper, IAccountService accountService)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
            this.accountService = accountService;
        }

        public async Task<Broker> GetBroker(int id)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var answer = await this.requestProviderService.Get<BrokerDto>(this.BrokerUrl, id, false);
            return this.mapper.Map<Broker>(answer);
        }

        public async Task<IEnumerable<Broker>> GetBrokers()
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var answer = await this.requestProviderService.Get<IEnumerable<BrokerDto>>(this.BrokerUrl);
            return this.mapper.Map<IEnumerable<Broker>>(answer);
        }

        public async Task<IEnumerable<Broker>> GetBrokersByList(IEnumerable<int> ids)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.BrokerUrl + "/list?";
            foreach(var id in ids){
                url += $"ids={id}&";
            }
            url.TrimEnd('&');
            var answer = await this.requestProviderService.Get<IEnumerable<Broker>>(url);
            return this.mapper.Map<IEnumerable<Broker>>(answer);
        }

        public async Task<Broker> UpdateBroker(Broker broker)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var mapped = mapper.Map<UpdateBrokerDto>(broker);
            var answer = await this.requestProviderService.Put<UpdateBrokerDto, BrokerDto>(this.BrokerUrl, mapped);
            return this.mapper.Map<Broker>(answer);
        }

        public async Task<Broker> AddBroker(Broker ins)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var mapped = mapper.Map<CreateBrokerDto>(ins);
            var answer = await this.requestProviderService.Post<CreateBrokerDto, BrokerDto>(this.BrokerUrl, mapped);
            return this.mapper.Map<Broker>(answer);
        }

        public async Task<bool> DeleteBroker(int id)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            return await this.requestProviderService.Delete(this.BrokerUrl, id);
        }

        public async Task<bool> UpdateBrokerBirthdayVideoUrl(int id, string url)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var toSend = new UpdateBirthdayVideoUrlDto
            {
                Id = id,
                BirthdayVideoUrl = url
            };
            var urlToGo = this.BrokerUrl + $"/birthdayvideourl";
            var answer = await this.requestProviderService.Post<UpdateBirthdayVideoUrlDto,bool>(urlToGo,toSend);
            return answer;
        }

        public async Task<BoolResponseDto> VerifyBroker(int id)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var urlToGo = this.BrokerUrl + $"/verifybroker/{id}";
            return await this.requestProviderService.Post<BoolResponseDto>(urlToGo);
        }
    }
}
