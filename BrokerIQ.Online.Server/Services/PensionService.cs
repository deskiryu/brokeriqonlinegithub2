using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;
using BrokerIQ.Dto.Enum;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Services.Base;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services
{
    public class PensionService : BIQService, IPensionService
    {
        private readonly string PensionUrl = "Pension";
        private readonly IMapper mapper;

        public PensionService(IRequestProviderService requestProviderService, IMapper mapper, IAccountService accountService)
            :base(accountService, requestProviderService)

        {
            this.mapper = mapper;
        }

        public async Task<IEnumerable<Pension>> GetForCustomer(int id)
        {
            var answer = await this._requestProviderService.Get<IEnumerable<PensionDto>>($"{this.PensionUrl}/customer/{id}/{GetCurrentBrokerId()}");
            return this.mapper.Map<IEnumerable<Pension>>(answer);
        }

        public async Task<Pension> Get(int id)
        {
            var answer = await this._requestProviderService.Get<PensionDto>(this.PensionUrl, id);
            return this.mapper.Map<Pension>(answer);
        }

        public async Task<Pension> Update(Pension ins)
        {
            var mapped = mapper.Map<UpdatePensionDto>(ins);

            var answer = await this._requestProviderService.Put<UpdatePensionDto, PensionDto>(this.PensionUrl, mapped);
            return this.mapper.Map<Pension>(answer);
        }
        
        public async Task<Pension> Add(Pension pension, int brokerId, List<(string, byte[])> documents)
        {
            var mapped = mapper.Map<CreatePensionDto>(pension);
            mapped.BrokerId = brokerId;

            if (documents != null && documents.Any())
            {
                mapped.Documents = new List<CreatePensionDocumentDto>();
                foreach (var document in documents)
                {
                    var PensionDoc = new PensionDocument
                    {
                        File = document.Item2,
                        FileName = document.Item1,
                        SupportingDocumentType = DocumentTypeEnum.PDF
                    };
                    var mappedDoc = mapper.Map<CreatePensionDocumentDto>(PensionDoc);
                    mapped.Documents.Add(mappedDoc);
                }

            }

            var answer = await this._requestProviderService.Post<CreatePensionDto, PensionDto>(this.PensionUrl, mapped);
            return this.mapper.Map<Pension>(answer);
        }

        public async Task<bool> Delete(int id)
        {
            return await this._requestProviderService.Delete(this.PensionUrl, id);
        }
    }
}
