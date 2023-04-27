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
    using BrokerIQ.Dto.Request;

    public class AdminService : IAdminService
    {
        private readonly string AdminAuthUrl = "AdminAuth";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;
        private readonly IAccountService accountService;

        public AdminService(IRequestProviderService requestProviderService, IMapper mapper, IAccountService accountService)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
            this.accountService = accountService;
        }
        public async Task<BoolResponseDto> VerifyAdmin()
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var urlToGo = "AdminAuth/verifyadmin";
            return await this.requestProviderService.Post<BoolResponseDto>(urlToGo);
        }

        public async Task<BoolResponseDto> VerifyMinorAdmin()
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var urlToGo = "AdminAuth/verifyminoradmin";
            return await this.requestProviderService.Post<BoolResponseDto>(urlToGo);
        }
    }
}
