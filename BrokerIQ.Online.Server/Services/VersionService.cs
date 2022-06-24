using BrokerIQ.Online.Services.Interface;
using BrokerIQ.Online.Services.Abstract;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

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
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersionInfo.ProductVersion;
        }
    }
}