using System.Threading.Tasks;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Server.Services.Base
{
    public abstract class BrokerIQService
    {
        protected readonly IAccountService accountService;

        public readonly IRequestProviderService requestProviderService;

        public BrokerIQService(IAccountService accountService, IRequestProviderService requestProviderService)
        {
            this.accountService = accountService;
            this.requestProviderService = requestProviderService;
        }

        protected async Task<int> GetCurrentBrokerId()
        {
            var user = await accountService.GetUser();
            requestProviderService.Token = user?.Token;

            return user.MasterBrokerId;
        }
    }
}