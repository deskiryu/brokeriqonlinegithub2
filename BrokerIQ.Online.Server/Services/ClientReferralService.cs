using System;
namespace BrokerIQ.Online.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using BrokerIQ.Dto.Models;
    using BrokerIQ.Online.Models;
    using BrokerIQ.Online.Services.Abstract;
    using BrokerIQ.Online.Services.Interface;


    public class ClientReferralService : IClientReferralService
    {
        private readonly string Url = "ClientReferral";
        private readonly IRequestProviderService requestProviderService;
        private readonly IAccountService accountService;
        private readonly IMapper mapper;

        public ClientReferralService(IRequestProviderService requestProviderService, IMapper mapper, IAccountService accountService)
        {
            this.requestProviderService = requestProviderService;
            this.mapper = mapper;
            this.accountService = accountService;
        }

        public async Task<bool> Delete(int id, int brokerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;

            var answer = await this.requestProviderService.Delete(this.Url+$"/{id}?brokerId={brokerId}");
            return answer;
        }

        public async Task<List<ClientReferral>> GetReferralsByBrokerId(int brokerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;

            var answer = await this.requestProviderService.Get<IEnumerable<ClientReferralDto>>(this.Url + $"/broker?brokerId={brokerId}");
            return (this.mapper.Map<IEnumerable<ClientReferral>>(answer)).ToList();
        }

        public async Task<ClientReferral> Update(ClientReferral updateClientReferral)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var updateDto = this.mapper.Map<UpdateClientReferralDto>(updateClientReferral);

            var answer = await this.requestProviderService.Post<UpdateClientReferralDto,ClientReferralDto>(this.Url, updateDto);
            return this.mapper.Map<ClientReferral>(answer);
        }
    }
}