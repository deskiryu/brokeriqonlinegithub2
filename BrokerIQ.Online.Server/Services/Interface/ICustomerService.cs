using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Enum;
using BrokerIQ.Dto.Import;
using BrokerIQ.Dto.Request;
using BrokerIQ.Dto.Response;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Models;

namespace BrokerIQ.Online.Services.Interface
{
    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetAllCustomers(int brokerId = 0, int assignedToId = 0, int filterRecent = 0, int filterPeriod = 0, int filterCategory = 0,
            int filterAgeRange = 0, bool profilePictures = false, bool nonAppUsersOnly = false);

        Task<IEnumerable<Customer>> GetFilteredCustomers(CustomerFilter filter);

        Task<Customer> GetCustomer(int id);

        Task<IEnumerable<Customer>> GetCustomersByList(IEnumerable<int> ids);

        Task<Customer> UpdateCustomer(Customer customer);

        Task<bool> DeleteCustomer(int id);

        Task<int> GetCustomerCount(int brokerId = 0);

        Task<CustomerCategoryEnum> SetCustomerCategory(int customerid, CustomerCategoryEnum customerCategory);

        Task<bool> SetCustomerNeeds(int customerid, bool hasNeeds);

        Task<Customer> GetConnection(int id);

        Task Disconnect(int connectedCustomerId);

        Task<bool> SetProfilePicture(int customerId, byte[] picture);

        Task<ImportResponse> Import(ImportRequest request);

        Task<CsvImportCustomerDto> GetImportDetails(int customerId);

        Task<bool> SendAppInvite(int customerId);

        Task<bool> SendAppInvites(int[] customerIds);

        Task<IEnumerable<Customer>> Search(int brokerId, string value);

        Task<bool> Connect(int brokerId, int mainCustomerId, int connectedCustomerId);

        Task<PagedResponse<Customer>> GetPagedCustomers(int brokerId = 0, int assignedToId = 0, int filterRecent = 0, int filterPeriod = 0, int filterCategory = 0,
            int filterAgeRange = 0, bool nonAppUsersOnly = false, ProfilingOptionEnum? profilingOption = null, SortOrderEnum sortOrder = SortOrderEnum.Id, SortByEnum sortBy = SortByEnum.Descending,
            bool profilePictures = false, int pageNumber = 1, int pageSize = 10);

        Task<PagedResponse<Customer>> GetPagedFilteredCustomers(CustomerFilter filter);
    }
}
