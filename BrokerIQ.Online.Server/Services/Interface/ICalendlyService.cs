using System.Threading.Tasks;

using BrokerIQ.Dto.Models;

namespace BrokerIQ.Online.Server.Services.Interface;

public interface ICalendlyService
{
    Task<CalendlyUserDto> GetUser();

    Task<bool> RegisterCalendlyConnection(string code);
}
