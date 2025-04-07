using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Dto;
using BrokerIQ.Dto.UpdateDto;
using BrokerIQ.Online.Server.Services.Base;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services
{
    public class EmailMessageTemplateService : BIQService, IEmailMessageTemplateService
    {
        private const string API_CONTROLLER = "EmailMessageTemplate";

        public EmailMessageTemplateService(IAccountService accountService, IRequestProviderService requestProviderService)
            : base(accountService, requestProviderService)
        {
        }

        public async Task<IEnumerable<EmailMessageTemplateDto>> GetAllForBroker(int brokerId)
        {
            try
            {
                return await _requestProviderService.Get<IEnumerable<EmailMessageTemplateDto>>($"{API_CONTROLLER}?brokerid={brokerId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get: exception {ex.Message}");
            }

            return Array.Empty<EmailMessageTemplateDto>();
        }

        public async Task<bool> Update(UpdateEmailMessageTemplateDto EmailMessageTemplate)
        {
            try
            {
                 await _requestProviderService.Put<UpdateEmailMessageTemplateDto, EmailMessageTemplateDto>(API_CONTROLLER, EmailMessageTemplate);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update: exception {ex.Message}");
            }

            return false;
        }

        public async Task<string> GetContentPreviewFor(EmailMessageTemplateDto emailMessageTemplate)
        {
            try
            {
                return await _requestProviderService.Post<EmailMessageTemplateDto, string>($"{API_CONTROLLER}/preview", emailMessageTemplate);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update: exception {ex.Message}");
            }

            return "Unable to get preview for email at the moment. Please try again in a bit.";
        }
    }
}