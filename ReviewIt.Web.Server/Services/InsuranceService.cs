using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReviewIt.Web.Services
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
    using ReviewIt.Web.Services.Interface;

    public class InsuranceService : IInsuranceService
    {
        private readonly string InsuranceUrl = "Insurance";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;
        private readonly IAccountService accountService;

        public InsuranceService(IRequestProviderService requestProviderService, IMapper mapper, IAccountService accountService)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
            this.accountService = accountService;
        }

        public async Task<Insurance> GetInsurance(int id, bool eager=true)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var answer = await this.requestProviderService.Get<InsuranceDto>(this.InsuranceUrl, id, eager);
            return this.mapper.Map<Insurance>(answer);
        }

        public async Task<Insurance> UpdateInsurance(Insurance ins)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var mapped = mapper.Map<UpdateInsuranceDto>(ins);
            var answer = await this.requestProviderService.Put<UpdateInsuranceDto, InsuranceDto>(this.InsuranceUrl, mapped);
            return this.mapper.Map<Insurance>(answer);
        }

        public async Task<Insurance> AddInsurance(Insurance ins)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var mapped = mapper.Map<CreateInsuranceDto>(ins);
            var answer = await this.requestProviderService.Post<CreateInsuranceDto, InsuranceDto>(this.InsuranceUrl, mapped);
            return this.mapper.Map<Insurance>(answer);
        }

        public async Task<bool> DeleteInsurance(int id)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            return await this.requestProviderService.Delete(this.InsuranceUrl, id);
        }
    }
}
