using BrokerIQ.Online.Services.Interface;
using BrokerIQ.Online.Services.Abstract;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;

namespace BrokerIQ.Online.Server.Services
{
    public class VersionService : IVersionService
    {
        private readonly IRequestProviderService requestProviderService;
        public VersionService(IRequestProviderService requestProviderService)
        {
            this.requestProviderService = requestProviderService;
        }
        public async Task<string> GetApiVersion()
        {
            var version = await this.requestProviderService.Get<string>("ping/version");
            return version;
        }

        public String GetVersion()
        {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            var version = asm.GetName().Version;

            return version.ToString();
        }
    }
}