using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Online.Models;

namespace BrokerIQ.Online.Server.Services.Interface
{
    public interface IAssignmentService
    {
        Task<IEnumerable<Customer>> GetForEmployee(int employeeId);

        Task<IEnumerable<Customer>> Assign(BrokerStaff staff, IEnumerable<int> customerIds);

        Task<IEnumerable<Customer>> Unassign(BrokerStaff staff, IEnumerable<int> customerIds);
    }
}