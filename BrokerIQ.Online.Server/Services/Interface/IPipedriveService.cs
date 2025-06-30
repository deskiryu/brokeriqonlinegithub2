using System.Threading.Tasks;

namespace BrokerIQ.Online.Server.Services.Interface;

public interface IPipedriveService
{
    Task<bool> Disconnect();

    Task<bool> RegisterConnection(string code);
}
