using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Services.Base;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services
{
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
            var answer = await this.requestProviderService.Get<CustomerDocumentDto>(this.CustomerDocumentUrl + @"/profile_picture/" + $"{customerId}");
            return answer;
        }

        public async Task<ApiResponse<IEnumerable<CustomerDocument>>> Get(int customerId)
        {
            var answer = await this.requestProviderService.GetResponse<IEnumerable<CustomerDocumentDto>>(this.CustomerDocumentUrl + @"/bycustomer/" + $"{customerId}?eagerLoadPdf=true");
            if (answer.IsSuccess)
            {
                return new ApiResponse<IEnumerable<CustomerDocument>>(answer.StatusCode, this.mapper.Map<IEnumerable<CustomerDocument>>(answer.Data));
            }

            return new ApiResponse<IEnumerable<CustomerDocument>>(answer.StatusCode, Array.Empty<CustomerDocument>());
        }

        public async Task<bool> DeleteCustomerDocument(Guid id)
        {
            return await this.requestProviderService.Delete(this.CustomerDocumentUrl, id);
        }
    }
}
