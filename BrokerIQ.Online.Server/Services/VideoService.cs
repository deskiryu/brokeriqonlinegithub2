using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using BrokerIQ.Online.Server.Models;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;
using BrokerIQ.Dto.Models;

namespace BrokerIQ.Online.Server.Services
{
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
            var answer = new List<VideoDto>();
            try
            {
                answer = await this.requestProviderService.Get<List<VideoDto>>(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetVideos: exception {ex.Message}");
            }
            return this.mapper.Map<List<Video>>(answer);
        }

        public async Task<Video> GetVideo(int id, int brokerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;

            var url = this.videoUrl + $"/single/{id}?brokerid={brokerId}";
            var answer = new VideoDto();
            try
            {
                answer = await this.requestProviderService.Get<VideoDto>(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetVideos: exception {ex.Message}");
            }
            return this.mapper.Map<Video>(answer);
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

        public async Task<VideoThumbnail> GetVideoThumbnail(int uploadedVideoId, int brokerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;

            var url = this.videoUrl + $"/thumbnail/{uploadedVideoId}?brokerId={brokerId}";
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

        public async Task<(int,string)> UploadAnalyseAndConvertVideo(string fileName, MemoryStream videoStream, int brokerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.videoUrl + $"/UploadAnalyseAndConvertVideo?brokerId={brokerId}&fileName={fileName}";
            var videoId = (0,"");
            try
            {
                videoId.Item1 = await this.requestProviderService.Post<MemoryStream, int>(url, videoStream, "application/octet-stream");
            }
            catch (Exception ex)
            {
                videoId.Item2 = ex.Message;
            }
            return videoId;
        }

        public async Task<bool> DeleteVideo(int id, int brokerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.videoUrl + $"?brokerId={brokerId}&id={id}";
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

        public async Task<bool> IsVetted(int id, int brokerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;

            var url = this.videoUrl + $"/vetted?brokerId={brokerId}&id={id}";
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

        public async Task<(bool, string)> SetNoVideo(int id, int brokerId)
        {
            var answer = (false, string.Empty);
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.videoUrl + $"/setnovideo?brokerId={brokerId}&id={id}";
            try
            {
                answer = await this.requestProviderService.Post<(bool, string)>(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SetWelcomeVideo: exception {ex.Message}");
            }
            return answer;
        }

        public async Task<(bool, string)> SetWelcomeVideo(int id, int brokerId, bool isWelcomeVideo = true)
        {
            var answer = (false, string.Empty);
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.videoUrl + $"/setwelcomevideo?brokerId={brokerId}&id={id}&iswelcomeVideo={isWelcomeVideo}";
            try
            {
                answer = await this.requestProviderService.Post<(bool, string)>(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SetWelcomeVideo: exception {ex.Message}");
            }
            return answer;
        }

        public async Task<(bool, string)> SetBirthdayVideo(int id, int brokerId, bool isBirthdayVideo = true)
        {
            var answer = (false, string.Empty);
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.videoUrl + $"/setbirthdayvideo?brokerId={brokerId}&id={id}&isbirthdayVideo={isBirthdayVideo}";
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

        public async Task<(bool, string)> SetMortgageVideo(int id, int brokerId, bool isMortgageVideo = true)
        {
            var result = (false, string.Empty);

            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.videoUrl + $"/setmortgagevideo?brokerId={brokerId}&id={id}&isMortgageVideo={isMortgageVideo}";

            try
            {
                result = await this.requestProviderService.Post<(bool, string)>(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SetMortgageVideo: exception {ex.Message}");
            }

            return result;
        }

        public async Task<(bool, string)> SetInsuranceVideo(int id, int brokerId, bool isInsuranceVideo = true)
        {
            var result = (false, string.Empty);

            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.videoUrl + $"/setinsurancevideo?brokerId={brokerId}&id={id}&isInsuranceVideo={isInsuranceVideo}";

            try
            {
                result = await this.requestProviderService.Post<(bool, string)>(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SetMortgageVideo: exception {ex.Message}");
            }

            return result;
        }
        public async Task<(bool, string)> SetVideoSendDate(int id, int brokerId, DateTime? sendDate)
        {
            var answer = (false, string.Empty);
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.videoUrl + $"/setsenddate?brokerId={brokerId}&id={id}&sendDate={sendDate.Value.ToString("yyyy-MM-dd")}";
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

        public async Task<(bool, string)> SetVideoSendDateTick(int id, int brokerId, bool value)
        {
            var answer = (false, string.Empty);
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.videoUrl + $"/setsenddatetick?brokerId={brokerId}&id={id}&value={value}";
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

        public async Task<(bool, string)> SetVetted(int id, bool value)
        {
            var answer = (false, string.Empty);
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.videoUrl + $"/vetted?id={id}&vetted={value}";
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

        public async Task<(bool, string)> SetBroker(int id, int brokerId)
        {
            var answer = (false, string.Empty);
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.videoUrl + $"/brokerid?brokerId={brokerId}&id={id}";
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
