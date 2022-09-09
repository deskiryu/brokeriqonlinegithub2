using System.Collections.Generic;

namespace BrokerIQ.Online.Services.Interface
{
    using System.Threading.Tasks;
    using BrokerIQ.Dto.Models;
    using BrokerIQ.Online.Server.Models;

    public interface IBrokerDefinedMessageService
    {
        Task<BrokerDefinedMessage> Get();
        
        Task<bool> UpdateOrCreate(List<DefinedMessagesDto> definedMessages);
    }
}
