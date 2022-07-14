namespace BrokerIQ.Online.Server.Services
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using BrokerIQ.Online.Services.Abstract;
    using BrokerIQ.Online.Services.Interface;

    public class LogoService : ILogoService
    {
        private readonly string logoUrl = "Logo";
        private readonly IRequestProviderService requestProviderService;
        private readonly IAccountService accountService;

        public LogoService(IRequestProviderService requestProviderService, IAccountService accountService)
        {
            this.requestProviderService = requestProviderService;
            this.accountService = accountService;
        }

        public async Task<bool> UploadLogo(string fileName, MemoryStream logoStream, int brokerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
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