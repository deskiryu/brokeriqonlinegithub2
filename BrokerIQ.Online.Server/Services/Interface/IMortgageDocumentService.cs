using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Services.Interface
{
    using Dto.Models;
    using Models;

    public interface IMortgageDocumentService
    {
        Task<bool> DeleteMortgageFile(Guid id);

        Task<bool> UploadMortgageFile(MortgageDocument sdoc);
    }
}
