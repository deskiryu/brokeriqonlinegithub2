using System.Collections.Generic;
using System.Threading.Tasks;

using BrokerIQ.Dto.Models;
using BrokerIQ.Dto.UpdateDto;

namespace BrokerIQ.Online.Services.Interface
{
    public interface IGoalService
    {
        Task<IEnumerable<GoalDto>> GetAllForBroker(int brokerId);

        Task<bool> Create(CreateGoalDto Goal);

        Task<bool> Update(UpdateGoalDto Goal);

        Task<bool> Delete(GoalDto toDelete);
    }
}