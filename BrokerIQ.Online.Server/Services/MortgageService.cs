using System.Collections.Generic;
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
    public class MortgageService : IMortgageService
    {
        private readonly string MortgageUrl = "Mortgage";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;

        public MortgageService(IRequestProviderService requestProviderService, IMapper mapper)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
        }

        public async Task<Mortgage> GetMortgage(int id, bool eager=true)
        {
            var answer = await this.requestProviderService.Get<MortgageDto>(this.MortgageUrl, id, eager);
            return this.mapper.Map<Mortgage>(answer);
        }

        public async Task<Mortgage> UpdateMortgage(Mortgage mortgage)
        {
            var mapped = mapper.Map<UpdateMortgageDto>(mortgage);
            var answer = await this.requestProviderService.Put<UpdateMortgageDto, MortgageDto>(this.MortgageUrl, mapped);
            return this.mapper.Map<Mortgage>(answer);
        }

        public async Task<Mortgage> AddMortgage(Mortgage mortgage)
        {
            var mapped = mapper.Map<CreateMortgageDto>(mortgage);
            var answer = await this.requestProviderService.Post<CreateMortgageDto, MortgageDto>(this.MortgageUrl, mapped);
            return this.mapper.Map<Mortgage>(answer);
        }

        public async Task<bool> DeleteMortgage(int id)
        {
            return await this.requestProviderService.Delete(this.MortgageUrl, id);
        }

        public async Task<Mortgage> AddMortgage(Mortgage ins, List<(string, byte[])> documents)
        {
            var mapped = mapper.Map<CreateMortgageDto>(ins);

            if (documents != null && documents.Any())
            {
                mapped.Documents = new List<CreateMortgageDocumentDto>();
                foreach (var document in documents)
                {
                    var MortgageDoc = new MortgageDocument
                    {
                        File = document.Item2,
                        FileName = document.Item1,
                        SupportingDocumentType = DocumentTypeEnum.PDF
                    };
                    var mappedDoc = mapper.Map<CreateMortgageDocumentDto>(MortgageDoc);
                    mapped.Documents.Add(mappedDoc);
                }

            }

            var answer = await this.requestProviderService.Post<CreateMortgageDto, MortgageDto>(this.MortgageUrl, mapped);
            return this.mapper.Map<Mortgage>(answer);
        }

        public async Task<MortgageFromDocumentDto> GetFromFile(DocumentDto dto)
        {
            return await this.requestProviderService.Post<DocumentDto, MortgageFromDocumentDto>($"{this.MortgageUrl}/getfromdocument", dto);
        }
    }
}
