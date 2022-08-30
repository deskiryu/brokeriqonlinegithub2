using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Services
{
    using Abstract;
    using AutoMapper;
    using Interface;
    using BrokerIQ.Online.Server.Models;
    using BrokerIQ.Dto;
    using BrokerIQ.Dto.CreateDto;

    public class DocumentsRequirementService : IDocumentsRequirementService
    {
        private readonly string DocumentsRequirementUrl = "DocumentsRequirement";
        private readonly IRequestProviderService requestProviderService;
        private readonly IAccountService accountService;
        private readonly IMapper mapper;

        public DocumentsRequirementService(IRequestProviderService requestProviderService, IMapper mapper, IAccountService accountService)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
            this.accountService = accountService;
        }

        public async Task<DocumentsRequirement> Get(int customerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var brokerId = user.MasterBrokerId;
            var answer = await this.requestProviderService.Get<DocumentsRequirementDto>(this.DocumentsRequirementUrl + $"/{customerId}/{brokerId}");
            return this.mapper.Map<DocumentsRequirement>(answer);
        }

        public async Task<DocumentsRequirement> Create(int customerId, ICollection<CreateDocumentsCheckDto> documentChecks)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var brokerId = user.MasterBrokerId;
            var createDocumentsRequirementDto = new CreateDocumentsRequirementDto
            {
                BrokerId = brokerId,
                CustomerId = customerId,
                DocumentChecks = documentChecks
            };

            var answer = await requestProviderService.Post<CreateDocumentsRequirementDto, DocumentsRequirementDto>(this.DocumentsRequirementUrl, createDocumentsRequirementDto);
            return this.mapper.Map<DocumentsRequirement>(answer);
        }

        public async Task<bool> Delete(int id)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            return await this.requestProviderService.Delete(this.DocumentsRequirementUrl, id);
        }
    }
}
