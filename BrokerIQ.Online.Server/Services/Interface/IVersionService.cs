using System;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Services.Interface
{
    public interface IVersionService
    {
        Task<string> GetApiVersion();

        String GetVersion();
    }
}