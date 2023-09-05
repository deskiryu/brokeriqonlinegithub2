using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Dto;

namespace BrokerIQ.Online.Server.Services.Interface
{
    public interface IOccupationService
    {
        Task<OccupationDto> GetById(int occupationId);

        Task<IEnumerable<OccupationDto>> Search(string partial);
    }
}