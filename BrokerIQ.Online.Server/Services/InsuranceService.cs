using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;

using BrokerIQ.Dto.Enum;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services
{
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

        public async Task<Insurance> GetInsurance(int id, bool eager = true)
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

        public async Task<Insurance> AddInsurance(Insurance ins, List<(string, byte[])> documents)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var mapped = mapper.Map<CreateInsuranceDto>(ins);

            if (documents != null && documents.Any())
            {
                mapped.Documents = new List<CreateInsuranceDocumentDto>();
                foreach (var document in documents)
                {
                    var insuranceDoc = new InsuranceDocument
                    {
                        File = document.Item2,
                        FileName = document.Item1,
                        SupportingDocumentType = DocumentTypeEnum.PDF
                    };
                    var mappedDoc = mapper.Map<CreateInsuranceDocumentDto>(insuranceDoc);
                    mapped.Documents.Add(mappedDoc);
                }

            }

            var answer = await this.requestProviderService.Post<CreateInsuranceDto, InsuranceDto>(this.InsuranceUrl, mapped);
            return this.mapper.Map<Insurance>(answer);
        }

        public async Task<bool> DeleteInsurance(int id)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            return await this.requestProviderService.Delete(this.InsuranceUrl, id);
        }

        public async Task<InsuranceFromDocumentDto> GetFromFile(DocumentDto dto)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;

            return await this.requestProviderService.Post<DocumentDto, InsuranceFromDocumentDto>($"{this.InsuranceUrl}/getfromdocumenttrained", dto);
        }
    }
}
