using System.Collections.Generic;
using System.Threading.Tasks;

using BrokerIQ.Dto.Models;
using BrokerIQ.Dto.UpdateDto;

namespace BrokerIQ.Online.Services.Interface
{
    public interface IDocumentVaultTypeService
    {
        Task<IEnumerable<DocumentVaultTypeDto>> GetAllForBroker(int brokerId);

        Task<bool> Create(CreateDocumentVaultTypeDto vaultType);

        Task<bool> Update(UpdateDocumentVaultTypeDto vaultType);

        Task<bool> Delete(DocumentVaultTypeDto toDelete);
    }
}