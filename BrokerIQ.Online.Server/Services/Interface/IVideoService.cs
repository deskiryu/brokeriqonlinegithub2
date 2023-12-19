using System;
using System.Collections.Generic;
using System.Text;

namespace BrokerIQ.Online.Services.Interface
{
    using System.IO;
    using System.Threading.Tasks;
    using BrokerIQ.Dto.Models;
    using BrokerIQ.Online.Server.Models;

    public interface IVideoService
    {
        Task<List<Video>> GetVideos(int brokerId);

        Task<Video> GetVideo(int id, int brokerId);

        Task<Dictionary<string, VideoThumbnail>> GetVideoThumbnails(int brokerId);

        Task<VideoThumbnail> GetVideoThumbnail(int uploadedVideoId, int brokerId);

        Task<int> UploadAnalyseAndConvertVideo(string fileName, MemoryStream videoStream, int brokerId);

        Task<bool> DeleteVideo(int id, int brokerId);

        Task<bool> IsVetted(int id, int brokerId);

        Task<(bool, string)> SetNoVideo(int id, int brokerId);

        Task<(bool, string)> SetWelcomeVideo(int id, int brokerId, bool welcomeVideo = true);

        Task<(bool, string)> SetBirthdayVideo(int id, int brokerId, bool birthdayVideo = true);

        Task<(bool, string)> SetMortgageVideo(int id, int brokerId, bool isMortgageVideo = true);

        Task<(bool, string)> SetInsuranceVideo(int id, int brokerId, bool isInsuranceVideo = true);

        Task<(bool, string)> SetVideoSendDate(int id, int brokerId, DateTime? sendDate);

        Task<(bool, string)> SetVideoSendDateTick(int id, int brokerId, bool value);

        Task<(bool, string)> SetVetted(int id, bool value);

        Task<(bool, string)> SetBroker(int id, int brokerId);

    }
}
