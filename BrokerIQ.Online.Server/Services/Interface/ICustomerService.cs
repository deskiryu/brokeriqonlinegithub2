using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Services.Interface
{
    using BrokerIQ.Dto.Enum;
    using BrokerIQ.Online.Server.Models;
    using Dto.Models;
    using Models;

    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetAllCustomers(int brokerId = 0, int filterRecent = 0, int filterPeriod = 0, int filterCategory = 0, int filterAgeRange = 0, bool profilePictures = false);

        Task<IEnumerable<Customer>> GetFilteredCustomers(CustomerFilter filter);

        Task<Customer> GetCustomer(int id);

        Task<IEnumerable<Customer>> GetCustomersByList(IEnumerable<int> ids);

        Task<Customer> UpdateCustomer(Customer customer);

        Task<bool> DeleteCustomer(int id);

        Task<int> GetCustomerCount(int brokerId = 0);

        Task<CustomerCategoryEnum> SetCustomerCategory(int customerid, CustomerCategoryEnum customerCategory);

        Task<bool> SetCustomerNeeds(int customerid, bool hasNeeds);
    }
}
