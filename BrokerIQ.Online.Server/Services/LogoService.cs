using System;
using System.IO;
using System.Threading.Tasks;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Server.Services
{
    public class LogoService : ILogoService
    {
        private readonly string logoUrl = "Logo";
        private readonly IRequestProviderService requestProviderService;

        public LogoService(IRequestProviderService requestProviderService)
        {
            this.requestProviderService = requestProviderService;
        }

        public async Task<bool> UploadLogo(string fileName, MemoryStream logoStream, int brokerId)
        {
            var url = this.logoUrl + $"?brokerId={brokerId}&fileName={fileName}";
            var answer = false;
            try
            {
                answer = await this.requestProviderService.Post<MemoryStream, bool>(url, logoStream, "application/octet-stream");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UploadLogo: exception {ex.Message}");
            }
            return answer;
        }
    }
}