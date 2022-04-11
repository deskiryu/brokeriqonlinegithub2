using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReviewIt.Web.Services
{
    using System.Net.Http;
    using System.Text.Json;
    using Abstract;
    using AppSettings;
    using AutoMapper;
    using Dto.Entities;
    using Dto.Models;
    using Interface;
    using Mapper;
    using Microsoft.Extensions.Options;
    using Models;

    public class MortgageDocumentService : IMortgageDocumentService
    {
        private readonly string MortgageFileUrl = "MortgageDocument";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;

        public MortgageDocumentService(IRequestProviderService requestProviderService, IMapper mapper)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
        }

        public async Task<bool> DeleteMortgageFile(Guid id)
        {
            return await this.requestProviderService.Delete(this.MortgageFileUrl, id);
        }

        public async Task<bool> UploadMortgageFile(MortgageDocument sdocs)
        {
            var mapped = mapper.Map<CreateMortgageDocumentDto>(sdocs);
            var answer = await this.requestProviderService.Post<CreateMortgageDocumentDto>(this.MortgageFileUrl, mapped);
            return answer;
        }
    }
}
