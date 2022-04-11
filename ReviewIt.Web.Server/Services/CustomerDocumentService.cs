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
    using ReviewIt.Dto.Enum;

    public class CustomerDocumentService : ICustomerDocumentService
    {
        private readonly string CustomerDocumentUrl = "CustomerDocument";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;

        public CustomerDocumentService(IRequestProviderService requestProviderService, IMapper mapper)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
        }
        public async Task<bool> DeleteProfilePicture(Guid id)
        {
            return await this.requestProviderService.Delete(this.CustomerDocumentUrl, id);
        }

        public async Task<bool> UploadProfilePicture(CustomerDocument sdoc)
        {
            var mapped = mapper.Map<CreateCustomerDocumentDto>(sdoc);
            var answer = await this.requestProviderService.Post<CreateCustomerDocumentDto>(this.CustomerDocumentUrl + @"/profile_picture", mapped);
            return answer;
        }

        public async Task<CustomerDocumentDto> GetProfilePicture(int customerId)
        {
            var answer =  await this.requestProviderService.Get<CustomerDocumentDto>(this.CustomerDocumentUrl+@"/profile_picture/"+$"{customerId}" );
            return answer;
        }

        public async Task<IEnumerable<CustomerDocumentDto>> Get(int customerId)
        {
            var answer = await this.requestProviderService.Get<IEnumerable<CustomerDocumentDto>>(this.CustomerDocumentUrl + @"/bycustomer/" + $"{customerId}");
            answer = answer.Where(x => x.DocuVaultType != DocuVaultTypeEnum.ProfilePicture);
            return answer;
        }
    }
}
