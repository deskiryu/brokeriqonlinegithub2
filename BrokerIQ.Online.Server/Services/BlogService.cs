using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using BrokerIQ.Dto.Models;
using BrokerIQ.Dto.UpdateDto;
using BrokerIQ.Online.Server.Services.Base;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services
{
    public class BlogService : BIQService, IBlogService
    {
        private const string API_CONTROLLER = "Blog";

        public BlogService(IAccountService accountService, IRequestProviderService requestProviderService)
            : base(accountService, requestProviderService)
        {
        }

        public async Task<IEnumerable<BlogDto>> GetAll()
        {
            try
            {
                return await _requestProviderService.Get<IEnumerable<BlogDto>>($"{API_CONTROLLER}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get: exception {ex.Message}");
            }

            return Array.Empty<BlogDto>();
        }
    }
}