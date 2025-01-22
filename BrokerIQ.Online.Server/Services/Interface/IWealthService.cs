using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Online.Models;

namespace BrokerIQ.Online.Services.Interface
{
    public interface IWealthService
    {
        Task<IEnumerable<Wealth>> GetForCustomer(int id, int brokerId);

        Task<Wealth> Get(int id);

        Task<Wealth> Update(Wealth wealth);

        Task<Wealth> Add(Wealth wealth, int brokerId, List<(string, byte[])> documents);

        Task<bool> Delete(int wealth);
    }
}
