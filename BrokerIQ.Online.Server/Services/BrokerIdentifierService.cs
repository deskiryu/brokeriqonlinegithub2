using System.Threading.Tasks;

using AutoMapper;

using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Server.Models;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services
{
    public class BrokerIdentifierService : IBrokerIdentifierService
    {
        private readonly string BrokerIdentifierUrl = "BrokerIdentifier";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;

        public BrokerIdentifierService(IRequestProviderService requestProviderService, IMapper mapper)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
        }

        public async Task<BrokerIdentifier> UpdateBrokerIdentifier(BrokerIdentifier brokerIdentifier)
        {
            var mapped = mapper.Map<UpdateBrokerIdentifierDto>(brokerIdentifier);
            var answer = await this.requestProviderService.Put<UpdateBrokerIdentifierDto, BrokerIdentifierDto>(this.BrokerIdentifierUrl, mapped);
            return this.mapper.Map<BrokerIdentifier>(answer);
        }

        public async Task<BrokerIdentifier> AddBrokerIdentifier(CreateBrokerIdentifierDto brokerIdentifier)
        {
            var answer = await this.requestProviderService.Post<CreateBrokerIdentifierDto, BrokerIdentifierDto>(this.BrokerIdentifierUrl, brokerIdentifier);
            return this.mapper.Map<BrokerIdentifier>(answer);
        }

        public async Task<bool> DeleteBrokerIdentifier(int id)
        {
            return await this.requestProviderService.Delete(this.BrokerIdentifierUrl + $"/{id}");
        }

        public async Task<BrokerIdentifier> GetDefaultBrokerIdentifier()
        {
            var answer = await this.requestProviderService.Get<BrokerIdentifierDto>(this.BrokerIdentifierUrl+"/GetDefault");
            return this.mapper.Map<BrokerIdentifier>(answer);
        }
    }
}
