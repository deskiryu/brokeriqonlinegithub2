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

        Task<Dictionary<string, VideoThumbnail>> GetVideoThumbnails(int brokerId);

        Task<VideoThumbnail> GetVideoThumbnail(string fileName, int brokerId);

        Task<bool> UploadAndConvertVideo(string fileName, MemoryStream videoStream, int brokerId);

        Task<bool> DeleteVideo(string fileName, int brokerId);

        Task<bool> DeleteVideoThumbnail(string fileName, int brokerId);

        Task<bool> IsVetted(string fileName, int brokerId);

        Task<bool> NameAvailable(string name, int brokerId);

        Task<(bool, string)> SetBirthdayVideo(string fileName, int brokerId, bool birthdayVideo = true);

        Task<(bool, string)> SetMortgageVideo(string fileName, int brokerId, bool isMortgageVideo = true);

        Task<(bool, string)> SetVideoSendDate(string fileName, int brokerId, DateTime? sendDate);

        Task<(bool, string)> SetVideoSendDateTick(string fileName, int brokerId, bool value);

        Task<(bool, string)> SetVetted(string fileName, bool value);

        Task<(bool, string)> SetBroker(string fileName, int brokerId);

        Task<string> MetaDefenderAnalyseFile(string fileName, MemoryStream videoStream);

        Task<object> MetaDefenderFetchAnalysisResult(string dataId);

    }
}
