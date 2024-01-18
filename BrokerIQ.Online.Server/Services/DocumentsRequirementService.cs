using System.Collections.Generic;

using System.Threading.Tasks;

using AutoMapper;

using BrokerIQ.Dto;
using BrokerIQ.Online.Services.Abstract;

using BrokerIQ.Dto.CreateDto;
using BrokerIQ.Online.Services.Interface;

using BrokerIQ.Online.Server.Models;

namespace BrokerIQ.Online.Services
{
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

        public async Task<DocumentsRequirement> Update(int documentRequirementId, IEnumerable<DocumentsCheckDto> documentChecks)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;

            var updateDocumentsRequirementDto = new UpdateDocumentsRequirementChecksDto
            {
                DocumentsRequirementId = documentRequirementId,
                DocumentChecks = documentChecks
            };

            var answer = await requestProviderService.Post<UpdateDocumentsRequirementChecksDto, DocumentsRequirementDto>($"{this.DocumentsRequirementUrl}/{documentRequirementId}", updateDocumentsRequirementDto);
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
