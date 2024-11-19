using System.Collections.Generic;
using System.Threading.Tasks;

using BrokerIQ.Dto.Models;
using BrokerIQ.Dto.UpdateDto;

namespace BrokerIQ.Online.Services.Interface
{
    public interface IWealthTypeService
    {
        Task<IEnumerable<WealthTypeDto>> GetAllForBroker(int brokerId);

        Task<bool> Create(CreateWealthTypeDto wealthType);

        Task<bool> Update(UpdateWealthTypeDto wealthType);

        Task<bool> Delete(WealthTypeDto toDelete);
    }
}