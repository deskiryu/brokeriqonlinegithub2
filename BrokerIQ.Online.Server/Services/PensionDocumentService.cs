using System;
using System.Threading.Tasks;
using AutoMapper;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services
{
    public class PensionDocumentService : IPensionDocumentService
    {
        private readonly string PensionFileUrl = "PensionDocument";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;

        public PensionDocumentService(IRequestProviderService requestProviderService, IMapper mapper)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
        }

        public async Task<bool> DeletePensionFile(Guid id)
        {
            return await this.requestProviderService.Delete(this.PensionFileUrl, id);
        }

        public async Task<bool> UploadPensionFile(PensionDocument sdocs)
        {
            var mapped = mapper.Map<CreatePensionDocumentDto>(sdocs);
            var answer = await this.requestProviderService.Post(this.PensionFileUrl, mapped);
            return answer;
        }
    }
}
