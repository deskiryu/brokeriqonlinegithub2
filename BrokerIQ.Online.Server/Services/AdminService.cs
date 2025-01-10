using System.Threading.Tasks;
using BrokerIQ.Dto.Request;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services
{
    public class AdminService : IAdminService
    {
        private readonly string AdminAuthUrl = "AdminAuth";

        private readonly IRequestProviderService requestProviderService;

        public AdminService(IRequestProviderService requestProviderService)
        {
            this.requestProviderService = requestProviderService;
        }
        public async Task<BoolResponseDto> VerifyAdmin()
        {
            var urlToGo = $"{AdminAuthUrl}/verifyadmin";
            return await this.requestProviderService.Post<BoolResponseDto>(urlToGo);
        }

        public async Task<BoolResponseDto> VerifyMinorAdmin()
        {
            var urlToGo = $"{AdminAuthUrl}/verifyminoradmin";
            return await this.requestProviderService.Post<BoolResponseDto>(urlToGo);
        }
    }
}
