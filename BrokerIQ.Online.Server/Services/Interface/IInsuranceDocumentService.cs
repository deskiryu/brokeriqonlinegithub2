using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Services.Interface
{
    using Dto.Models;
    using Models;

    public interface IInsuranceDocumentService
    {
        Task<bool> DeleteInsuranceFile(Guid id);

        Task<bool> UploadInsuranceFile(InsuranceDocument sdoc);
    }
}
