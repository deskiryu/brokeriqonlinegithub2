using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Services
{
    using System.Net.Http;
    using System.Text.Json;
    using Abstract;
    using AppSettings;
    using AutoMapper;
    using Dto.Models;
    using Interface;
    using Mapper;
    using Microsoft.Extensions.Options;
    using Models;
    using BrokerIQ.Online.Services.Interface;

    public class MortgageService : IMortgageService
    {
        private readonly string MortgageUrl = "Mortgage";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;
        private readonly IAccountService accountService;

        public MortgageService(IRequestProviderService requestProviderService, IMapper mapper, IAccountService accountService)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
            this.accountService = accountService;
        }

        public async Task<Mortgage> GetMortgage(int id, bool eager=true)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var answer = await this.requestProviderService.Get<MortgageDto>(this.MortgageUrl, id, eager);
            return this.mapper.Map<Mortgage>(answer);
        }

        public async Task<Mortgage> UpdateMortgage(Mortgage mortgage)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var mapped = mapper.Map<UpdateMortgageDto>(mortgage);
            var answer = await this.requestProviderService.Put<UpdateMortgageDto, MortgageDto>(this.MortgageUrl, mapped);
            return this.mapper.Map<Mortgage>(answer);
        }

        public async Task<Mortgage> AddMortgage(Mortgage mortgage)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var mapped = mapper.Map<CreateMortgageDto>(mortgage);
            var answer = await this.requestProviderService.Post<CreateMortgageDto, MortgageDto>(this.MortgageUrl, mapped);
            return this.mapper.Map<Mortgage>(answer);
        }

        public async Task<bool> DeleteMortgage(int id)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            return await this.requestProviderService.Delete(this.MortgageUrl, id);
        }
    }
}
