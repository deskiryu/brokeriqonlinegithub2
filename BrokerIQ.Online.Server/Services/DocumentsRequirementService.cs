using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BrokerIQ.Dto;
using BrokerIQ.Dto.CreateDto;
using BrokerIQ.Online.Server.Models;
using BrokerIQ.Online.Server.Services.Base;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services
{
    public class DocumentsRequirementService : BIQService , IDocumentsRequirementService
    {
        private readonly string DocumentsRequirementUrl = "DocumentsRequirement";
        private readonly IMapper mapper;

        public DocumentsRequirementService(IRequestProviderService requestProviderService, IMapper mapper, IAccountService accountService)
            :base(accountService, requestProviderService)
        {
            this.mapper = mapper;
        }

        public async Task<DocumentsRequirement> Get(int customerId)
        {
            var brokerId = await GetCurrentBrokerId();

            var answer = await this._requestProviderService.Get<DocumentsRequirementDto>(this.DocumentsRequirementUrl + $"/{customerId}/{brokerId}");
            return this.mapper.Map<DocumentsRequirement>(answer);
        }

        public async Task<DocumentsRequirement> Create(int customerId, ICollection<CreateDocumentsCheckDto> documentChecks)
        {
            var brokerId = await GetCurrentBrokerId();

            var createDocumentsRequirementDto = new CreateDocumentsRequirementDto
            {
                BrokerId = brokerId,
                CustomerId = customerId,
                DocumentChecks = documentChecks
            };

            var answer = await _requestProviderService.Post<CreateDocumentsRequirementDto, DocumentsRequirementDto>(this.DocumentsRequirementUrl, createDocumentsRequirementDto);
            return this.mapper.Map<DocumentsRequirement>(answer);
        }

        public async Task<DocumentsRequirement> Update(int documentRequirementId, IEnumerable<DocumentsCheckDto> documentChecks)
        {
            var updateDocumentsRequirementDto = new UpdateDocumentsRequirementChecksDto
            {
                DocumentsRequirementId = documentRequirementId,
                DocumentChecks = documentChecks
            };

            var answer = await _requestProviderService.Post<UpdateDocumentsRequirementChecksDto, DocumentsRequirementDto>($"{this.DocumentsRequirementUrl}/{documentRequirementId}", updateDocumentsRequirementDto);
            return this.mapper.Map<DocumentsRequirement>(answer);
        }

        public async Task<bool> Delete(int id)
        {
            return await this._requestProviderService.Delete(this.DocumentsRequirementUrl, id);
        }
    }
}
