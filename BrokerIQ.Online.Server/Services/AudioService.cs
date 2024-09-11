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

        public async Task<Audio> GetAudio(int id, int brokerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;

            var url = this.audioUrl + $"/single/{id}?brokerid={brokerId}";
            var answer = new AudioDto();
            try
            {
                answer = await this.requestProviderService.Get<AudioDto>(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAudios: exception {ex.Message}");
            }
            return this.mapper.Map<Audio>(answer);
        }
        public async Task<(int, string)> UploadAndAnalyseAudio(string fileName, MemoryStream audioStream, int brokerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;


            //Don't like ambersands in name
            fileName = fileName.Replace("&", "%26");

            var url = this.audioUrl + $"/UploadAndAnalyseAudio?brokerId={brokerId}&fileName={fileName}";
            var audioResponse = (0, "");
            try
            {
                audioResponse.Item1 = await this.requestProviderService.Post<MemoryStream, int>(url, audioStream, "application/octet-stream");
            }
            catch (Exception ex)
            {
                audioResponse.Item2 = ex.Message;
            }
            return audioResponse;
        }

        public async Task<bool> DeleteAudio(int id, int brokerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.audioUrl + $"?brokerId={brokerId}&id={id}";
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
    }
}