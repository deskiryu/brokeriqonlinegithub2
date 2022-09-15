using System;
using System.Collections.Generic;

namespace BrokerIQ.Online.Server.Services
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using BrokerIQ.Online.Server.Models;
    using BrokerIQ.Online.Services.Abstract;
    using BrokerIQ.Online.Services.Interface;
    using BrokerIQ.Dto.Models;

    public class VideoService : IVideoService
    {
        private readonly string videoUrl = "Video";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;
        private readonly IAccountService accountService;

        public VideoService(IRequestProviderService requestProviderService, IMapper mapper, IAccountService accountService)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
            this.accountService = accountService;
        }
        public async Task<List<Video>> GetVideos(int brokerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;

            var url = this.videoUrl + $"?brokerId={brokerId}";
            var answer = new List<AzureVideoDto>();
            try
            {
                answer = await this.requestProviderService.Get<List<AzureVideoDto>>(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetVideos: exception {ex.Message}");
            }
            return this.mapper.Map<List<Video>>(answer);
        }

        public async Task<Dictionary<string, VideoThumbnail>> GetVideoThumbnails(int brokerId)
        {
            var url = this.videoUrl + $"/thumbnail?brokerId={brokerId}";
            var answer = new Dictionary<string, VideoThumbnail>();
            try
            {
                answer = await this.requestProviderService.Get<Dictionary<string, VideoThumbnail>>(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetVideoThumbnails: exception {ex.Message}");
            }
            return answer;
        }

        public async Task<VideoThumbnail> GetVideoThumbnail(string fileName, int brokerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;

            var url = this.videoUrl + $"/thumbnail/{fileName}?brokerId={brokerId}";
            VideoThumbnail answer = null;
            try
            {
                answer = await this.requestProviderService.Get<VideoThumbnail>(url);
            }
            catch
            {
                // we expect the thumbnail to not be available immediately - don't print exception to console
            }
            return answer;
        }

        public async Task<bool> UploadVideo(string fileName, MemoryStream videoStream, int brokerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.videoUrl + $"?brokerId={brokerId}&fileName={fileName}";
            var answer = false;
            try
            {
                answer = await this.requestProviderService.Post<MemoryStream, bool>(url, videoStream, "application/octet-stream");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UploadVideo: exception {ex.Message}");
            }
            return answer;
        }

        public async Task<bool> DeleteVideo(string fileName, int brokerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.videoUrl + $"?brokerId={brokerId}&fileName={fileName}";
            var answer = false;
            try
            {
                answer = await this.requestProviderService.Delete(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeleteVideo: exception {ex.Message}");
            }
            return answer;
        }

        public async Task<bool> DeleteVideoThumbnail(string fileName, int brokerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.videoUrl + $"/thumbnail?brokerId={brokerId}&fileName={fileName}";
            var answer = false;
            try
            {
                answer = await this.requestProviderService.Delete(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeleteVideoThumbnail: exception {ex.Message}");
            }
            return answer;
        }

        public async Task<bool> IsVetted(string fileName, int brokerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;

            var url = this.videoUrl + $"/vetted?brokerId={brokerId}&fileName={fileName}";
            var answer = false;
            try
            {
                answer = await this.requestProviderService.Get<bool>(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"IsVetted: exception {ex.Message}");
            }
            return answer;
        }

        public async Task<bool> NameAvailable(string fileName, int brokerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;

            var url = this.videoUrl + $"/nameavailable/{fileName}?brokerId={brokerId}";
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

        public async Task<(bool, string)> SetBirthdayVideo(string fileName, int brokerId, bool birthdayVideo = true)
        {
            var answer = (false, string.Empty);
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.videoUrl + $"/setbirthdayvideo?brokerId={brokerId}&fileName={fileName}&isBirthdayVideo={birthdayVideo}";
            try
            {
                answer = await this.requestProviderService.Post<(bool, string)>(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SetBirthdayVideo: exception {ex.Message}");
            }
            return answer;
        }

        public async Task<(bool, string)> SetVideoSendDate(string fileName, int brokerId, DateTime? sendDate)
        {
            var answer = (false, string.Empty);
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.videoUrl + $"/setsenddate?brokerId={brokerId}&fileName={fileName}&sendDate={sendDate.Value.ToString("yyyy-MM-dd")}";
            try
            {
                answer = await this.requestProviderService.Post<(bool, string)>(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SetVideoSendDate: exception {ex.Message}");
            }
            return answer;
        }

        public async Task<(bool, string)> SetVideoSendDateTick(string fileName, int brokerId, bool value)
        {
            var answer = (false, string.Empty);
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.videoUrl + $"/setsenddatetick?brokerId={brokerId}&fileName={fileName}&value={value}";
            try
            {
                answer = await this.requestProviderService.Post<(bool, string)>(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SetVideoSendDateTick: exception {ex.Message}");
            }
            return answer;
        }

        public async Task<(bool, string)> SetVetted(string fileName, bool value)
        {
            var answer = (false, string.Empty);
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.videoUrl + $"/vetted?fileName={fileName}&vetted={value}";
            try
            {
                answer = await this.requestProviderService.Post<(bool, string)>(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SetVetted: exception {ex.Message}");
            }
            return answer;
        }

        public async Task<(bool, string)> SetBroker(string fileName, int brokerId)
        {
            var answer = (false, string.Empty);
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.videoUrl + $"/brokerid?brokerId={brokerId}&fileName={fileName}";
            try
            {
                answer = await this.requestProviderService.Post<(bool, string)>(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SetBrokerId: exception {ex.Message}");
            }
            return answer;
        }
    }
}
