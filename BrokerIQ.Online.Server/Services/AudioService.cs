using System.Collections.Generic;

namespace BrokerIQ.Online.Server.Services
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using BrokerIQ.Dto.Models;
    using BrokerIQ.Online.Server.Models;
    using BrokerIQ.Online.Services.Abstract;
    using BrokerIQ.Online.Services.Interface;

    public class AudioService : IAudioService
    {
        private readonly string audioUrl = "Audio";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;
        private readonly IAccountService accountService;

        public AudioService(IRequestProviderService requestProviderService, IMapper mapper, IAccountService accountService)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
            this.accountService = accountService;
        }

        public async Task<List<Audio>> GetAudios(int brokerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;

            var url = this.audioUrl + $"?brokerId={brokerId}";
            var answer = new List<AudioDto>();
            try
            {
                answer = await this.requestProviderService.Get<List<AudioDto>>(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAudios: exception {ex.Message}");
            }
            return this.mapper.Map<List<Audio>>(answer);
        }

        public async Task<bool> UploadAndAnalyseAudio(string fileName, MemoryStream audioStream, int brokerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.audioUrl + $"/UploadAndAnalyseAudio?brokerId={brokerId}&fileName={fileName}";
            var answer = false;
            try
            {
                answer = await this.requestProviderService.Post<MemoryStream, bool>(url, audioStream, "application/octet-stream");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UploadAudio: exception {ex.Message}");
            }
            return answer;
        }

        public async Task<bool> DeleteAudio(string fileName, int brokerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.audioUrl + $"?brokerId={brokerId}&fileName={fileName}";
            var answer = false;
            try
            {
                answer = await this.requestProviderService.Delete(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeleteAudio: exception {ex.Message}");
            }
            return answer;
        }

        public async Task<bool> NameAvailable(string fileName, int brokerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;

            var url = this.audioUrl + $"/nameavailable/{fileName}?brokerId={brokerId}";
            var answer = false;
            try
            {
                answer = await this.requestProviderService.Get<bool>(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"NameAvailable: exception {ex.Message}");
            }
            return answer;
        }
    }
}