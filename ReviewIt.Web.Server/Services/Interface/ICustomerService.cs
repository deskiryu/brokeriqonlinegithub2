using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReviewIt.Web.Services.Interface
{
    using Dto.Models;
    using Models;

    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetAllCustomers(int brokerId = 0, int filterRecent = 0, int filterPeriod = 0);

        Task<Customer> GetCustomer(int id);

        Task<IEnumerable<Customer>> GetCustomersByList(IEnumerable<int> ids);

        Task<Customer> UpdateCustomer(Customer customer);

        Task<bool> DeleteCustomer(int id);

        Task<int> GetCustomerCount(int brokerId = 0);
    }
}
