using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Online.Models;

namespace BrokerIQ.Online.Services.Interface
{
    public interface IPensionService
    {
        Task<IEnumerable<Pension>> GetForCustomer(int id);

        Task<Pension> Get(int id);

        Task<Pension> Update(Pension pension);

        Task<Pension> Add(Pension pension, int brokerId);

        Task<bool> Delete(int pension);
    }
}
