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

    public class InsuranceDocumentService : IInsuranceDocumentService
    {
        private readonly string InsuranceFileUrl = "InsuranceDocument";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;

        public InsuranceDocumentService(IRequestProviderService requestProviderService, IMapper mapper)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
        }

        public async Task<bool> DeleteInsuranceFile(Guid id)
        {
            return await this.requestProviderService.Delete(this.InsuranceFileUrl, id);
        }

        public async Task<bool> UploadInsuranceFile(InsuranceDocument sdocs)
        {
            var mapped = mapper.Map<CreateInsuranceDocumentDto>(sdocs);
            var answer = await this.requestProviderService.Post<CreateInsuranceDocumentDto>(this.InsuranceFileUrl, mapped);
            return answer;
        }
    }
}
