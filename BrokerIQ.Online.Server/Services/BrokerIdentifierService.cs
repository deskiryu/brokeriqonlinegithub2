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
        private readonly IAccountService accountService;

        public BrokerIdentifierService(IRequestProviderService requestProviderService, IMapper mapper, IAccountService accountService)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
            this.accountService = accountService;
        }

        public async Task<BrokerIdentifier> UpdateBrokerIdentifier(BrokerIdentifier brokerIdentifier)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var mapped = mapper.Map<UpdateBrokerIdentifierDto>(brokerIdentifier);
            var answer = await this.requestProviderService.Put<UpdateBrokerIdentifierDto, BrokerIdentifierDto>(this.BrokerIdentifierUrl, mapped);
            return this.mapper.Map<BrokerIdentifier>(answer);
        }

        public async Task<BrokerIdentifier> AddBrokerIdentifier(CreateBrokerIdentifierDto brokerIdentifier)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var answer = await this.requestProviderService.Post<CreateBrokerIdentifierDto, BrokerIdentifierDto>(this.BrokerIdentifierUrl, brokerIdentifier);
            return this.mapper.Map<BrokerIdentifier>(answer);
        }

        public async Task<bool> DeleteBrokerIdentifier(int id)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            return await this.requestProviderService.Delete(this.BrokerIdentifierUrl + $"/{id}");
        }

    }
}
