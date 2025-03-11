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
            bool response = false;
            try
            {
                response = await _requestProviderService.Put<UpdateEmailMessageTemplateDto, bool>(API_CONTROLLER, EmailMessageTemplate);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update: exception {ex.Message}");
            }
            return response;
        }
    }
}