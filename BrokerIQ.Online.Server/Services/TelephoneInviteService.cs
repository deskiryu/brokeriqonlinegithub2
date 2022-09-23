using System;
namespace BrokerIQ.Online.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoMapper;
    using BrokerIQ.Dto.Models;
    using BrokerIQ.Online.Models;
    using BrokerIQ.Online.Services.Abstract;
    using BrokerIQ.Online.Services.Interface;


    public class TelephoneInviteService : ITelephoneInviteService
    {
        private readonly string TelephoneUrl = "TelephoneInvite";
        private readonly IRequestProviderService requestProviderService;
        private readonly IAccountService accountService;
        private readonly IMapper mapper;

        public TelephoneInviteService(IRequestProviderService requestProviderService, IMapper mapper, IAccountService accountService)
        {
            this.requestProviderService = requestProviderService;
            this.mapper = mapper;
            this.accountService = accountService;
        }

        public async Task<IEnumerable<TelephoneInvite>> GetTelephoneInvites()
        {
            var answer = await this.requestProviderService.Get<IEnumerable<TelephoneInviteDto>>(this.TelephoneUrl + $"/invites");
            return this.mapper.Map<IEnumerable<TelephoneInvite>>(answer);
        }

        public async Task<IEnumerable<TelephoneInvite>> GetTelephoneInvitesByBrokerId(int brokerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;

            var answer = await this.requestProviderService.Get<IEnumerable<TelephoneInviteDto>>(this.TelephoneUrl + $"/invites/{brokerId}");
            return this.mapper.Map<IEnumerable<TelephoneInvite>>(answer);
        }

        public async Task<bool> AddTelephoneInvites(CreateTelephoneInviteDto createTelephoneInvite)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var answer = await this.requestProviderService.Post<CreateTelephoneInviteDto, TelephoneInviteDto>(this.TelephoneUrl, createTelephoneInvite);
            return answer != null;
        }

        public async Task<bool> DeleteTelephoneInvite(int id)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var answer = await this.requestProviderService.Delete(this.TelephoneUrl, id);
            return answer;
        }

        public async Task<bool> UpdateTelephoneInvites(UpdateEmailInviteDto updateTelephoneInviteDto)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var answer = await this.requestProviderService.Post<UpdateEmailInviteDto, bool>(this.TelephoneUrl + $"/update", updateTelephoneInviteDto);
            return answer;
        }
    }
}