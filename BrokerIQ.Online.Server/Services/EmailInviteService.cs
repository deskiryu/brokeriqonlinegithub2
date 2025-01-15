using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services
{
    public class EmailInviteService : IEmailInviteService
    {
        private readonly string emailUrl = "EmailInvite";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;

        public EmailInviteService(IRequestProviderService requestProviderService, IMapper mapper)
        {
            this.requestProviderService = requestProviderService;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<EmailInvite>> GetEmailInvites()
        {
            var answer = await this.requestProviderService.Get<IEnumerable<EmailInviteDto>>(this.emailUrl + $"/invites");
            return this.mapper.Map<IEnumerable<EmailInvite>>(answer);
        }

        public async Task<IEnumerable<EmailInvite>> GetEmailInvitesByBrokerId(int brokerId)
        {
            var answer = await this.requestProviderService.Get<IEnumerable<EmailInviteDto>>(this.emailUrl + $"/invites/{brokerId}");
            return this.mapper.Map<IEnumerable<EmailInvite>>(answer);
        }

        public async Task<bool> AddEmailInvites(CreateEmailInviteDto createEmailInvite)
        {
            var answer = await this.requestProviderService.Post<CreateEmailInviteDto, bool>(this.emailUrl + $"/directinvites", createEmailInvite);
            return answer;
        }

        public async Task<bool> DeleteEmailInvite(int id)
        {
            var answer = await this.requestProviderService.Delete(this.emailUrl, id);
            return answer;
        }

        public async Task<bool> UpdateEmailInvites(UpdateEmailInviteDto updateEmailInviteDto)
        {
            var answer = await this.requestProviderService.Post<UpdateEmailInviteDto, bool>(this.emailUrl + $"/update", updateEmailInviteDto);
            return answer;
        }
    }
}