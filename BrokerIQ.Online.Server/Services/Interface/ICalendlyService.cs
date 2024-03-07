using System.Threading.Tasks;

namespace BrokerIQ.Online.Server.Services.Interface;

public interface ICalendlyService
{
    Task<bool> RegisterCalendlyConnection(string code);
}
