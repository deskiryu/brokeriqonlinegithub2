using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Dto;

namespace BrokerIQ.Online.Services.Interface
{
    public interface ICustomerWarningService
    {
        Task<IEnumerable<CustomerWarningDto>> GetForCustomer(int id);
    }
}
