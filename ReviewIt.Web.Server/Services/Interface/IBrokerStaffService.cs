using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReviewIt.Web.Services.Interface
{
    using Dto.Models;
    using Models;
    using ReviewIt.Web.Server.Models;

    public interface IBrokerStaffService
    {
        Task<IEnumerable<BrokerStaff>> GetBrokerStaffbyBrokerId(int id);

        Task<IEnumerable<BrokerStaff>> GetBrokerStaff();

        Task<bool> DeleteBrokerStaff(int id);

        Task<BrokerStaff> GetBrokerStaff(int id);

        Task<BrokerStaff> UpdateBrokerStaff(BrokerStaff brokerStaff);
    }
}
