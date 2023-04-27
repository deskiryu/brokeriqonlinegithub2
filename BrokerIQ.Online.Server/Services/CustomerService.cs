using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Services
{
    using Abstract;
    using AutoMapper;
    using Dto.Models;
    using Interface;
    using Models;
    using BrokerIQ.Online.Server.Extensions;
    using BrokerIQ.Dto.Enum;
    using Microsoft.AspNetCore.JsonPatch;

    public class CustomerService : ICustomerService
    {
        private readonly string customerUrl = "Customer";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;
        private readonly IAccountService accountService;

        public CustomerService(IRequestProviderService requestProviderService, IMapper mapper, IAccountService accountService)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
            this.accountService = accountService;
        }

        public async Task<IEnumerable<Customer>> GetAllCustomers(int brokerId=0, int filterRecent=0, int filterPeriod = 0, int filterCategory = 0, int filterAgeRange = 0, bool profilePictures=false)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var option = (RecentEnum)filterRecent;
            var ts = ((RecentPeriodEnum)filterPeriod).TransformToTS();
            var searchOption = new SearchOptionDto
            {
                InsuranceEndingSoon = option==RecentEnum.RecentInsurance,
                MortgagePromotionEndingSoon = option == RecentEnum.RecentMortgage,
                InsuranceRecentPeriod = ts,
                MortgagePromotionRecentPeriod = ts,
                CustomerCategory = (CustomerCategoryEnum)filterCategory,
                AgeRange = (AgeRangeEnum)filterAgeRange
            };

            var url = this.customerUrl;
            if (user.IsBroker || user.IsBrokerStaff)
            {
                 url += "/broker/" + $"{user.MasterBrokerId}";

            }
            else if (user.IsAdmin)
            {
                if (brokerId > 0)
                {
                    url += "/broker/" + $"{brokerId}";
                }
                else
                {
                    url += "/getwithfilter";
                }
            }
            url += $"?profilePictures={profilePictures}";

            var answer = await this.requestProviderService.Post<SearchOptionDto,IEnumerable<CustomerDto>>(url, searchOption);
            return this.mapper.Map<IEnumerable<Customer>>(answer);

            throw new UnauthorizedAccessException();
        }

        public async Task<Customer> GetCustomer(int id)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.customerUrl;
            var brokerId = 0;
            if (user.IsBroker || user.IsBrokerStaff)
            {
                brokerId = user.MasterBrokerId;
            }
            var answer = await this.requestProviderService.Get<CustomerDto>(url, id, true, brokerId);
            return this.mapper.Map<Customer>(answer);
        }

        public async Task<IEnumerable<Customer>> GetCustomersByList(IEnumerable<int> ids)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            if (user.IsAdmin)
            {
                var url = this.customerUrl + "/list?";
                foreach (var id in ids)
                {
                    url += $"ids={id}&";
                }
                url.TrimEnd('&');

                var answer = await this.requestProviderService.Get<IEnumerable<CustomerDto>>(url);
                return this.mapper.Map<IEnumerable<Customer>>(answer);
            }

            throw new UnauthorizedAccessException();
        }

        public async Task<Customer> UpdateCustomer(Customer customer)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var updateCustomerDto = mapper.Map<UpdateCustomerDto>(customer);
            var answer = await this.requestProviderService.Put<UpdateCustomerDto, CustomerDto>(this.customerUrl, updateCustomerDto);
            return this.mapper.Map<Customer>(answer);
        }

        public async Task<bool> DeleteCustomer(int id)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var answer = await this.requestProviderService.Delete(this.customerUrl, id);
            return answer;
        }

        public async Task<int> GetCustomerCount(int brokerId = 0)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var answer = await this.requestProviderService.Get<int>(this.customerUrl+$"/count", brokerId);
            return answer;
        }

        public async Task<CustomerCategoryEnum> SetCustomerCategory(int customerid, CustomerCategoryEnum customerCategory)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var patchDoc = new JsonPatchDocument<Customer>();
            patchDoc.Replace(x => (int)x.CustomerCategory, (int)customerCategory);

            var answer = await this.requestProviderService.Patch<JsonPatchDocument<Customer>,CustomerDto>(this.customerUrl+$"/patch?customerid={customerid}", patchDoc);
            return answer.CustomerCategory;
        }

    }
}
