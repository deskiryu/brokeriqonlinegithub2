using System;
using System.Collections.Generic;
using System.Text;

namespace BrokerIQ.Online.Services.Interface
{
    using System.IO;
    using System.Threading.Tasks;
    using BrokerIQ.Online.Server.Models;

    public interface IAzureService
    {
        Task Initialise();
        Task<bool> TransferVideoStreamToAzureBlob(string fileName, DateTime? sendDate, Stream stream, int brokerId);
        Task<bool> TransferAudioStreamToAzureBlob(string fileName, Stream stream, int brokerId);
        Task<bool> TransferLogoStreamToAzureBlob(string fileName, Stream stream, int brokerId);
        Task<List<Video>> GetVideoBlobs(int brokerId);
        Task<Dictionary<string, VideoThumbnail>> GetVideoThumbnailBlobs(int brokerId);
        Task<VideoThumbnail> GetVideoThumbnailBlob(string fileName);
        Task<List<Audio>> GetAudioBlobs(int brokerId);
        Task<bool> DeleteVideoBlob(string fileName);
        Task<bool> DeleteVideoThumbnailBlob(string fileName);
        Task<bool> DeleteAudioBlob(string fileName);
        Task<bool> IsVettedVideo(string fileName);
        Task<(bool, string)> SetBirthdayVideo(string fileName, int brokerId, bool birthdayVideo = true);
        Task<(bool, string)> SetVideoSendDate(string fileName, int brokerId, DateTime? sendDate);
    }
}
