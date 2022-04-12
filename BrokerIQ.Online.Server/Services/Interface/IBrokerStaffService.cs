using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Services.Interface
{
    using Dto.Models;
    using Models;
    using BrokerIQ.Online.Server.Models;

    public interface IBrokerStaffService
    {
        Task<IEnumerable<BrokerStaff>> GetBrokerStaffbyBrokerId(int id);

        Task<IEnumerable<BrokerStaff>> GetBrokerStaff();

        Task<bool> DeleteBrokerStaff(int id);

        Task<BrokerStaff> GetBrokerStaff(int id);

        Task<BrokerStaff> UpdateBrokerStaff(BrokerStaff brokerStaff);
    }
}
