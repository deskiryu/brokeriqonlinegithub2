using System;
using System.Collections.Generic;
using System.Text;

namespace BrokerIQ.Online.Services.Interface
{
    using System.IO;
    using System.Threading.Tasks;
    using BrokerIQ.Online.Server.Models;

    public interface IVideoService
    {
        Task<List<Video>> GetVideos(int brokerId);

        Task<Dictionary<string, VideoThumbnail>> GetVideoThumbnails(int brokerId);

        Task<VideoThumbnail> GetVideoThumbnail(string fileName);

        Task<bool> UploadVideo(string fileName, DateTime? sendDate, Stream stream, int brokerId);

        Task<bool> DeleteVideo(string fileName);

        Task<bool> DeleteVideoThumbnail(string fileName);

        Task<bool> IsVetted(string fileName);

        Task<bool> NameAvailable(string name);

        Task<(bool, string)> SetBirthdayVideo(string fileName, int brokerId, bool birthdayVideo = true);

        Task<(bool, string)> SetVideoSendDate(string fileName, int brokerId, DateTime? sendDate);

        Task<(bool, string)> SetVideoSendDateTick(string fileName, int brokerId, bool value);

    }
}
