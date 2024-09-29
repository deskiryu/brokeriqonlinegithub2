using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Services.Base;

namespace BrokerIQ.Online.Services.Interface
{
    public interface ICustomerDocumentService
    {
        Task<bool> DeleteProfilePicture(Guid id);

        Task<bool> UploadProfilePicture(CustomerDocument sdoc);

        Task<CustomerDocumentDto> GetProfilePicture(int customerId);

        Task<ApiResponse<IEnumerable<CustomerDocument>>> Get(int customerId);

        Task<bool> DeleteCustomerDocument(Guid id);

    }
}
