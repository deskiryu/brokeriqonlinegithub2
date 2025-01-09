using System;
using System.Threading.Tasks;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Server.Services.Base
{
    public abstract class BIQService
    {
        protected readonly IAccountService _accountService;
        private readonly CookieService _cookieService;

        protected readonly IRequestProviderService _requestProviderService;

        public BIQService(IAccountService accountService, IRequestProviderService requestProviderService, CookieService cookieService)
        {
            _accountService = accountService;
            _requestProviderService = requestProviderService;
            _cookieService = cookieService;
        }

        protected async Task<int> GetCurrentBrokerId()
        {
            var user = await _accountService.GetUser();

            return user.MasterBrokerId;
        }
    }
}