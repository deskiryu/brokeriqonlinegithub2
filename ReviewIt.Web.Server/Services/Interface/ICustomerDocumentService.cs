using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReviewIt.Web.Services.Interface
{
    using Dto.Models;
    using Models;

    public interface ICustomerDocumentService
    {
        Task<bool> DeleteProfilePicture(Guid id);

        Task<bool> UploadProfilePicture(CustomerDocument sdoc);

        Task<CustomerDocumentDto> GetProfilePicture(int customerId);

        Task<IEnumerable<CustomerDocumentDto>> Get(int customerId);

    }
}
