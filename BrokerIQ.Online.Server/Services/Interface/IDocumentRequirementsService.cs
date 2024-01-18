using System.Collections.Generic;
using System.Threading.Tasks;

using BrokerIQ.Dto;
using BrokerIQ.Dto.CreateDto;
using BrokerIQ.Online.Server.Models;

namespace BrokerIQ.Online.Services.Interface
{
    public interface IDocumentsRequirementService
    {
        Task<DocumentsRequirement> Get(int customerId);

        Task<bool> Delete(int id);

        Task<DocumentsRequirement> Create(int customerId, ICollection<CreateDocumentsCheckDto> documentChecks);

        Task<DocumentsRequirement> Update(int documentRequirementId, IEnumerable<DocumentsCheckDto> documentChecks);
    }
}
