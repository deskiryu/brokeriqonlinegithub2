using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Models;

namespace BrokerIQ.Online.Services.Interface
{
    public interface IBrokerConsentDocumentService
    {
        Task<IEnumerable<BrokerConsentDocumentDto>> GetAllForCurrentBroker();

        Task<IEnumerable<BrokerConsentDocumentDto>> GetForBroker(int brokerId);

        Task<bool> Create(CreateBrokerConsentDocumentDto document);

        Task<bool> Update(BrokerConsentDocumentDto document);

        Task<bool> Delete(BrokerConsentDocumentDto document);
    }
}

