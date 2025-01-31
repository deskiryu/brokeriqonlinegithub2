using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BrokerIQ.Dto.Dto.Import;
using BrokerIQ.Dto.Enum;
using BrokerIQ.Dto.Import;
using BrokerIQ.Dto.Models;
using BrokerIQ.Dto.Request;
using BrokerIQ.Dto.Response;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Extensions;
using BrokerIQ.Online.Server.Models;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.JsonPatch;

namespace BrokerIQ.Online.Services
{
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

        public CustomerService()
        {
        }

        public async Task<IEnumerable<Customer>> GetAllCustomers(int brokerId = 0, int assignedToId = 0, int filterRecent = 0, int filterPeriod = 0, int filterCategory = 0,
            int filterAgeRange = 0, bool profilePictures = false, bool nonAppUsersOnly = false)
        {
            return await GetFilteredCustomers(new CustomerFilter()
            {
                BrokerId = brokerId,
                AssignedToId = assignedToId,
                Recent = filterRecent,
                Period = filterPeriod,
                Category = filterCategory,
                AgeRange = filterAgeRange,
                ProfilePictures = profilePictures,
                NonAppUsersOnly = nonAppUsersOnly
            });
        }

        public async Task<Customer> GetCustomer(int id)
        {
            var user = await this.accountService.GetUser();

            var url = this.customerUrl;
            var brokerId = 0;
            if (user.IsBroker || user.IsAdminStaff || user.IsBrokerStaff)
            {
                brokerId = user.MasterBrokerId;
            }
            var answer = await this.requestProviderService.Get<CustomerDto>(url, id, true, brokerId);
            return this.mapper.Map<Customer>(answer);
        }

        public async Task<IEnumerable<Customer>> GetCustomersByList(IEnumerable<int> ids)
        {
            var user = await this.accountService.GetUser();

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
            var updateCustomerDto = mapper.Map<UpdateCustomerDto>(customer);
            var answer = await this.requestProviderService.Put<UpdateCustomerDto, CustomerDto>(this.customerUrl, updateCustomerDto);
            return this.mapper.Map<Customer>(answer);
        }

        public async Task<bool> DeleteCustomer(int id)
        {
            var answer = await this.requestProviderService.Delete(this.customerUrl, id);
            return answer;
        }

        public async Task<int> GetCustomerCount(int brokerId = 0)
        {
            var answer = await this.requestProviderService.Get<int>(this.customerUrl + $"/count", brokerId);
            return answer;
        }

        public async Task<int> GetCustomerAppUserCount(int brokerId = 0)
        {
            var answer = await this.requestProviderService.Get<int>(this.customerUrl + $"/appusercount", brokerId);
            return answer;
        }

        public async Task<CustomerCategoryEnum> SetCustomerCategory(int customerid, CustomerCategoryEnum customerCategory)
        {
            var patchDoc = new JsonPatchDocument<Customer>();
            patchDoc.Replace(x => (int)x.CustomerCategory, (int)customerCategory);

            var answer = await this.requestProviderService.Patch<JsonPatchDocument<Customer>, CustomerDto>(this.customerUrl + $"/patch?customerid={customerid}", patchDoc);
            return answer.CustomerCategory;
        }

        public async Task<IEnumerable<Customer>> GetFilteredCustomers(CustomerFilter filter)
        {
            var user = await this.accountService.GetUser();

            var option = (RecentEnum)filter.Recent;
            var ts = ((RecentPeriodEnum)filter.Period).TransformToTS();
            var searchOption = new SearchOptionDto
            {
                InsuranceEndingSoon = option == RecentEnum.RecentInsurance,
                MortgagePromotionEndingSoon = option == RecentEnum.RecentMortgage,
                InsuranceRecentPeriod = ts,
                MortgagePromotionRecentPeriod = ts,
                CustomerCategory = (CustomerCategoryEnum)filter.Category,
                AgeRange = (AgeRangeEnum)filter.AgeRange,
                ProfilingOption = filter.ProfilingOption,
                AssignedToId = filter.AssignedToId,
                NonAppUsersOnly = filter.NonAppUsersOnly
            };

            var url = this.customerUrl;
            if (user.IsBroker || user.IsAdminStaff || user.IsBrokerStaff)
            {
                url += "/broker/" + $"{user.MasterBrokerId}";
            }
            else if (user.IsAdmin)
            {
                if (filter.BrokerId > 0)
                {
                    url += "/broker/" + $"{filter.BrokerId}";
                }
                else
                {
                    url += "/getwithfilter";
                }
            }
            url += $"?profilePictures={filter.ProfilePictures}";

            var answer = await requestProviderService.Post<SearchOptionDto, IEnumerable<CustomerDto>>(url, searchOption);
            return mapper.Map<IEnumerable<Customer>>(answer);
        }

        public async Task<bool> SetCustomerNeeds(int customerid, bool hasNeeds)
        {
            var patchDoc = new JsonPatchDocument<Customer>();
            patchDoc.Replace(x => x.HasNeeds, hasNeeds);

            var answer = await this.requestProviderService.Patch<JsonPatchDocument<Customer>, CustomerDto>(this.customerUrl + $"/patch?customerid={customerid}", patchDoc);
            return answer.HasNeeds;
        }

        public async Task<Customer> GetConnection(int id)
        {
            var url = $"{this.customerUrl}/{id}/connection";
            var answer = await this.requestProviderService.Get<CustomerDto>(url);

            return this.mapper.Map<Customer>(answer);
        }

        public async Task Disconnect(int mainCustomerId)
        {
            var url = $"{this.customerUrl}/{mainCustomerId}/disconnect";

            await this.requestProviderService.Post<bool>(url);

            return;
        }

        public async Task<bool> SetProfilePicture(int customerId, byte[] picture)
        {
            var postData = new ProfilePictureDto() { CustomerId = customerId, File = picture };

            await this.requestProviderService.Post<ProfilePictureDto, CustomerDto>($"{this.customerUrl}/profile_picture", postData);

            return true;
        }

        public async Task<IEnumerable<CustomerImportDto>> PreviewImportData(ImportRequest request)
        {
            return await this.requestProviderService.Post<ImportRequest, IEnumerable<CustomerImportDto>>($"{this.customerUrl}/previewimportdata", request);
        }

        public async Task<ImportResponse> Import(ImportRequest request)
        {
            return await this.requestProviderService.Post<ImportRequest, ImportResponse>($"{this.customerUrl}/import", request);
        }

        public async Task<CsvImportCustomerDto> GetImportDetails(int customerId)
        {
            return await this.requestProviderService.Get<CsvImportCustomerDto>($"{this.customerUrl}/{customerId}/import");
        }

        public async Task<bool> UpdateImportDetails(CsvImportCustomerDto toUpdate)
        {
            return await this.requestProviderService.Post<CsvImportCustomerDto, bool>($"{this.customerUrl}/{toUpdate.CustomerId}/import", toUpdate);
        }

        public async Task<bool> SendAppInvite(int customerId)
        {
            return await this.requestProviderService.Post<int, bool>($"{this.customerUrl}/{customerId}/sendappinvite", customerId);
        }

        public async Task<bool> SendAppInvites(int[] customerIds)
        {
            return await this.requestProviderService.Post<int[], bool>($"{this.customerUrl}/sendappinvites", customerIds);
        }

        public async Task<IEnumerable<Customer>> Search(int brokerId, string value)
        {
            var answer = await requestProviderService.Get<IEnumerable<CustomerDto>>($"{customerUrl}/broker/{brokerId}/search?value={value}");
            return mapper.Map<IEnumerable<Customer>>(answer);
        }

        public async Task<bool> Connect(int brokerId, int mainCustomerId, int connectedCustomerId)
        {
            return await requestProviderService.Post<ConnectCustomersRequest, bool>($"{customerUrl}/connect", new ConnectCustomersRequest()
            {
                BrokerId = brokerId,
                MainCustomerId = mainCustomerId,
                ConnectedCustomerId = connectedCustomerId
            });
        }

        public async Task<PagedResponse<Customer>> GetPagedCustomers(
            int brokerId,
            int assignedToId,
            int filterRecent,
            int filterPeriod,
            int filterCategory,
            int filterAgeRange, 
            bool nonAppUsersOnly, 
            SortOrderEnum sortOrder, 
            SortByEnum sortBy,
            string partialName, bool profilePictures,
            int pageNumber,
            int pageSize,
            int lastMaxId,
            int lastMinId,
            PagingDirectionEnum pagingDirectionEnum = PagingDirectionEnum.FirstPage,
            ProfilingOptionEnum? profilingOption = null
            )
        {
            return await GetPagedFilteredCustomers(new CustomerFilter()
            {
                BrokerId = brokerId,
                AssignedToId = assignedToId,
                Recent = filterRecent,
                Period = filterPeriod,
                Category = filterCategory,
                AgeRange = filterAgeRange,
                ProfilePictures = profilePictures,
                NonAppUsersOnly = nonAppUsersOnly,
                ProfilingOption = profilingOption,
                PartialName = partialName,
                SortOrder = sortOrder,
                SortBy = sortBy,
                PageNumber = pageNumber,
                PageSize = pageSize,
                LastMaxId = lastMaxId,
                LastMinId = lastMinId,
                PagingDirectionEnum = pagingDirectionEnum
            });
        }

        private async Task<PagedResponse<Customer>> GetPagedFilteredCustomers(CustomerFilter filter)
        {
            var user = await this.accountService.GetUser();

            var option = (RecentEnum)filter.Recent;
            var ts = ((RecentPeriodEnum)filter.Period).TransformToTS();
            var searchOption = new SearchOptionDto
            {
                InsuranceEndingSoon = option == RecentEnum.RecentInsurance,
                MortgagePromotionEndingSoon = option == RecentEnum.RecentMortgage,
                InsuranceRecentPeriod = ts,
                MortgagePromotionRecentPeriod = ts,
                CustomerCategory = (CustomerCategoryEnum)filter.Category,
                AgeRange = (AgeRangeEnum)filter.AgeRange,
                ProfilingOption = filter.ProfilingOption,
                PartialName = filter.PartialName,
                SortOrder = filter.SortOrder,
                SortBy = filter.SortBy,
                AssignedToId = filter.AssignedToId,
                NonAppUsersOnly = filter.NonAppUsersOnly
            };

            var url = this.customerUrl;

            
            if (user.IsBroker || user.IsBrokerStaff)
            {
                url += $"/brokerpaged/{filter.BrokerId}";
            }
            else if (user.IsAdmin)
            {
                if (filter.BrokerId > 0)
                {
                    url += "/brokerpaged/" + $"{filter.BrokerId}";
                }
                else
                {
                    url += "/adminpaged";
                }
            }
            url += $"?profilePictures={filter.ProfilePictures}";
            url += $"&pagenumber={filter.PageNumber}&pageSize={filter.PageSize}&lastMaxId={filter.LastMaxId}&lastMinId={filter.LastMinId}&PagingDirectionEnum={filter.PagingDirectionEnum}";

            var answer = await requestProviderService.Post<SearchOptionDto, PagedResponse<CustomerDto>>(url, searchOption);

            return new PagedResponse<Customer>()
            {
                CurrentPage = answer.CurrentPage,
                TotalPages = answer.TotalPages,
                PageData = mapper.Map<IEnumerable<Customer>>(answer.PageData),
                TotalRecords = answer.TotalRecords
            };
        }
    }
}
