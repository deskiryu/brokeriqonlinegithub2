using System;
using System.Threading.Tasks;
using BrokerIQ.Online.Models;

namespace BrokerIQ.Online.Services.Interface
{
    public interface IWealthDocumentService
    {
        Task<bool> DeleteWealthFile(Guid id);

        Task<bool> UploadWealthFile(WealthDocument sdoc);
    }
}
