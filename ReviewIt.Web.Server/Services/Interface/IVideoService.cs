using System;
using System.Collections.Generic;
using System.Text;

namespace ReviewIt.Web.Services.Interface
{
    using System.IO;
    using System.Threading.Tasks;
    using ReviewIt.Web.Server.Models;

    public interface IVideoService
    {
        Task<List<Video>> GetVideos(int brokerId);

        Task<bool> UploadVideo(string fileName, Stream stream, int brokerId);

        Task<bool> DeleteVideo(string fileName);

        Task<bool> IsVetted(string fileName);

        Task<bool> NameAvailable(string name);

        Task<(bool, string)> SetBirthdayVideo(string fileName, int brokerId, bool birthdayVideo = true);
    }
}
