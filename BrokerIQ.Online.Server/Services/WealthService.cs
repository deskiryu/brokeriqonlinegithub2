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
    public class WealthService : BIQService, IWealthService
    {
        private readonly string WealthUrl = "Wealth";
        private readonly IMapper mapper;

        public WealthService(IRequestProviderService requestProviderService, IMapper mapper, IAccountService accountService)
            :base(accountService, requestProviderService)

        {
            this.mapper = mapper;
        }

        public async Task<IEnumerable<Wealth>> GetForCustomer(int id)
        {
            var answer = await this._requestProviderService.Get<IEnumerable<WealthDto>>($"{this.WealthUrl}/customer/{id}");
            return this.mapper.Map<IEnumerable<Wealth>>(answer);
        }

        public async Task<Wealth> Get(int id)
        {
            var answer = await this._requestProviderService.Get<WealthDto>(this.WealthUrl, id);
            return this.mapper.Map<Wealth>(answer);
        }

        public async Task<Wealth> Update(Wealth ins)
        {
            var mapped = mapper.Map<UpdateWealthDto>(ins);

            var answer = await this._requestProviderService.Put<UpdateWealthDto, WealthDto>(this.WealthUrl, mapped);
            return this.mapper.Map<Wealth>(answer);
        }
        
        public async Task<Wealth> Add(Wealth wealth, int brokerId, List<(string, byte[])> documents)
        {
            var mapped = mapper.Map<CreateWealthDto>(wealth);
            mapped.BrokerId = brokerId;

            if (documents != null && documents.Any())
            {
                mapped.Documents = new List<CreateWealthDocumentDto>();
                foreach (var document in documents)
                {
                    var WealthDoc = new WealthDocument
                    {
                        File = document.Item2,
                        FileName = document.Item1,
                        SupportingDocumentType = DocumentTypeEnum.PDF
                    };
                    var mappedDoc = mapper.Map<CreateWealthDocumentDto>(WealthDoc);
                    mapped.Documents.Add(mappedDoc);
                }

            }

            var answer = await this._requestProviderService.Post<CreateWealthDto, WealthDto>(this.WealthUrl, mapped);
            return this.mapper.Map<Wealth>(answer);
        }

        public async Task<bool> Delete(int id)
        {
            return await this._requestProviderService.Delete(this.WealthUrl, id);
        }
    }
}
