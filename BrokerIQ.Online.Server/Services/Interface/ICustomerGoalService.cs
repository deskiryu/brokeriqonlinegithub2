using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Entities;
using BrokerIQ.Dto.Models;

namespace BrokerIQ.Online.Services.Interface
{
    public interface ICustomerGoalService
    {
        Task<IEnumerable<CustomerGoalDto>> Get(int customerId);

        Task<bool> Create(CreateCustomerGoalDto dto);

        Task<bool> Update(UpdateCustomerGoalDto dto);

        Task<bool> Delete(int id);
    }
}
