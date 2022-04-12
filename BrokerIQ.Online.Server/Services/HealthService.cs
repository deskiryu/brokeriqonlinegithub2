using BrokerIQ.Online.Services.Interface;
using BrokerIQ.Online.Services.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Server.Services
{
    public class HealthService : IHealthService
    {
        private readonly IRequestProviderService requestProviderService;
        public HealthService(IRequestProviderService requestProviderService)
        {
            this.requestProviderService = requestProviderService;
        }
        public async Task<bool> CanPingApi()
        {
            var pinged = false;
            try
            {
                pinged = await this.requestProviderService.Get<bool>("ping");
            }
            catch
            {
                pinged = false;   
            }
            return pinged;
        }
    }
}
