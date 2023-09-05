using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Dto;
using BrokerIQ.Online.Server.Services.Interface;
using BrokerIQ.Online.Services.Abstract;

namespace BrokerIQ.Online.Server.Services
{
    public class OccupationService : IOccupationService
    {
        private readonly string URL = "Occupation";

        private readonly IRequestProviderService requestProviderService;

        public OccupationService(IRequestProviderService requestProviderService)
        {
            this.requestProviderService = requestProviderService;
        }

        public async Task<OccupationDto> GetById(int occupationId)
        {
            var occupation = await requestProviderService.Get<OccupationDto>($"{URL}/{occupationId}");

            return occupation;
        }

        public async Task<IEnumerable<OccupationDto>> Search(string partial)
        {
            var occupations = await requestProviderService.Get<IEnumerable<OccupationDto>>($"{URL}?partial={partial}");

            return occupations;
        }
    }
}