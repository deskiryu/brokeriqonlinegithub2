using System;
using System.Threading.Tasks;
using AutoMapper;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services
{
    public class WealthDocumentService : IWealthDocumentService
    {
        private readonly string WealthFileUrl = "WealthDocument";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;

        public WealthDocumentService(IRequestProviderService requestProviderService, IMapper mapper)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
        }

        public async Task<bool> DeleteWealthFile(Guid id)
        {
            return await this.requestProviderService.Delete(this.WealthFileUrl, id);
        }

        public async Task<bool> UploadWealthFile(WealthDocument sdocs)
        {
            var mapped = mapper.Map<CreateWealthDocumentDto>(sdocs);
            var answer = await this.requestProviderService.Post(this.WealthFileUrl, mapped);
            return answer;
        }
    }
}
