using System;
using System.Threading.Tasks;
using BrokerIQ.Online.Models;

namespace BrokerIQ.Online.Services.Interface
{
    public interface IPensionDocumentService
    {
        Task<bool> DeletePensionFile(Guid id);

        Task<bool> UploadPensionFile(PensionDocument sdoc);
    }
}
