using System.Threading.Tasks;

namespace BrokerIQ.Online.Server.Services.Interface;

public interface ICalendlyService
{
    Task<bool> Disconnect();

    Task<bool> RegisterCalendlyConnection(string code);
}
