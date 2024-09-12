using System.Collections.Generic;
using System.Threading.Tasks;

using AutoMapper;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services
{
    public class PensionService : IPensionService
    {
        private readonly string PensionUrl = "Pension";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;
        private readonly IAccountService accountService;

        public PensionService(IRequestProviderService requestProviderService, IMapper mapper, IAccountService accountService)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
            this.accountService = accountService;
        }

        public async Task<IEnumerable<Pension>> GetForCustomer(int id)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var answer = await this.requestProviderService.Get<IEnumerable<PensionDto>>($"{this.PensionUrl}/customer/{id}/{user.MasterBrokerId}");
            return this.mapper.Map<IEnumerable<Pension>>(answer);
        }

        public async Task<Pension> Get(int id)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var answer = await this.requestProviderService.Get<PensionDto>(this.PensionUrl, id);
            return this.mapper.Map<Pension>(answer);
        }

        public async Task<Pension> Update(Pension ins)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;

            var mapped = mapper.Map<UpdatePensionDto>(ins);

            var answer = await this.requestProviderService.Put<UpdatePensionDto, PensionDto>(this.PensionUrl, mapped);
            return this.mapper.Map<Pension>(answer);
        }
        
        public async Task<Pension> Add(Pension pension, int brokerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;

            var mapped = mapper.Map<CreatePensionDto>(pension);
            mapped.BrokerId = brokerId;

            var answer = await this.requestProviderService.Post<CreatePensionDto, PensionDto>(this.PensionUrl, mapped);
            return this.mapper.Map<Pension>(answer);
        }

        public async Task<bool> Delete(int id)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            return await this.requestProviderService.Delete(this.PensionUrl, id);
        }
    }
}
