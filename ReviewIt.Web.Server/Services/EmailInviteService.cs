using System;
namespace ReviewIt.Web.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoMapper;
    using ReviewIt.Dto.Models;
    using ReviewIt.Web.Models;
    using ReviewIt.Web.Services.Abstract;
    using ReviewIt.Web.Services.Interface;


    public class EmailInviteService : IEmailInviteService
    {
        private readonly string emailUrl = "EmailInvite";
        private readonly IRequestProviderService requestProviderService;
        private readonly IAccountService accountService;
        private readonly IMapper mapper;

        public EmailInviteService(IRequestProviderService requestProviderService, IMapper mapper, IAccountService accountService)
        {
            this.requestProviderService = requestProviderService;
            this.mapper = mapper;
            this.accountService = accountService;
        }

        public async Task<IEnumerable<EmailInvite>> GetEmailInvites()
        {
            var answer = await this.requestProviderService.Get<IEnumerable<EmailInviteDto>>(this.emailUrl + $"/invites");
            return this.mapper.Map<IEnumerable<EmailInvite>>(answer);
        }

        public async Task<IEnumerable<EmailInvite>> GetEmailInvitesByBrokerId(int brokerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;

            var answer = await this.requestProviderService.Get<IEnumerable<EmailInviteDto>>(this.emailUrl + $"/invites/{brokerId}");
            return this.mapper.Map<IEnumerable<EmailInvite>>(answer);
        }

        public async Task<bool> AddEmailInvites(CreateEmailInviteDto createEmailInvite)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var answer = await this.requestProviderService.Post<CreateEmailInviteDto, bool>(this.emailUrl + $"/directinvites", createEmailInvite);
            return answer;
        }

        public async Task<bool> DeleteEmailInvite(int id)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var answer = await this.requestProviderService.Delete(this.emailUrl, id);
            return answer;
        }

        public async Task<bool> UpdateEmailInvites(UpdateEmailInviteDto updateEmailInviteDto)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var answer = await this.requestProviderService.Post<UpdateEmailInviteDto, bool>(this.emailUrl + $"/update", updateEmailInviteDto);
            return answer;
        }
    }
}