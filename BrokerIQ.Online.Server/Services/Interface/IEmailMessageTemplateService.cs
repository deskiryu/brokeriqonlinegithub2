using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Dto;
using BrokerIQ.Dto.UpdateDto;

namespace BrokerIQ.Online.Services.Interface
{
    public interface IEmailMessageTemplateService
    {
        Task<IEnumerable<EmailMessageTemplateDto>> GetAllForBroker(int brokerId);

        Task<bool> Update(UpdateEmailMessageTemplateDto EmailMessageTemplate);
    }
}