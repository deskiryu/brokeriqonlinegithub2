using System;
namespace ReviewIt.Web.Services
{
    using System.Threading.Tasks;
    using BrokerIQ.Dto.Models;
    using ReviewIt.Web.Models;
    using ReviewIt.Web.Services.Abstract;
    using ReviewIt.Web.Services.Interface;


    public class EmailService : IEmailService
    {
        private readonly string emailUrl = "Email";
        private readonly IRequestProviderService requestProviderService;

        public EmailService(IRequestProviderService requestProviderService)
        {
            this.requestProviderService = requestProviderService;
        }
        public async Task<bool> SendEmail(CreateEmailDto email)
        {
            var answer = await this.requestProviderService.Post<CreateEmailDto, bool>(this.emailUrl, email);
            return answer;
        }

        public async Task<bool> SendInviteEmails(CreateEmailDto email)
        {
            var answer = await this.requestProviderService.Post<CreateEmailDto, bool>(this.emailUrl + "/invitetemplate", email);
            return answer;
        }
    }
}