using System;
using System.Collections.Generic;
using System.Text;

namespace BrokerIQ.Online.Server.Services
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using BrokerIQ.Online.Server.Models;
    using BrokerIQ.Online.Services.Interface;

    public class VideoService : IVideoService
    {
        private readonly IAzureService azureService;

        public VideoService(IAzureService azureService)
        {
            this.azureService = azureService;
        }
        public async Task<List<Video>> GetVideos(int brokerId)
        {
            var blobs = new List<Video>();
            try
            {
                blobs = await azureService.GetVideoBlobs(brokerId);
            }
            catch
            {

            }
            return blobs;
        }

        public async Task<bool> UploadVideo(string fileName, Stream stream, int brokerId)
        {
            bool succeeded = false;
            try
            {
                succeeded = await azureService.TransferVideoStreamToAzureBlob(fileName, stream, brokerId);
            }
            catch
            {

            }
            return succeeded;
        }

        public async Task<bool> DeleteVideo(string fileName)
        {
            bool succeeded = false;
            try
            {
                succeeded = await azureService.DeleteVideoBlob(fileName);
            }
            catch
            {

            }
            return succeeded;
        }

        public async Task<bool> IsVetted(string fileName)
        {
            bool succeeded = false;
            try
            {
                succeeded = await azureService.IsVettedVideo(fileName);
            }
            catch
            {

            }
            return succeeded;
        }

        public async Task<bool> NameAvailable(string name)
        {
            bool foundName = true;
            try
            {
                var blobs = await azureService.GetVideoBlobs(0);
                foundName = blobs.Any(x => Path.GetFileNameWithoutExtension(x.Name).Equals(name, StringComparison.InvariantCultureIgnoreCase));
            }
            catch
            {

            }
            return !foundName;
        }

        public async Task<(bool,string)> SetBirthdayVideo(string fileName, int brokerId, bool birthdayVideo = true)
        {
            var returned = (false,string.Empty);
            try
            {
                returned = await azureService.SetBirthdayVideo(fileName, brokerId, birthdayVideo);
            }
            catch
            {

            }
            return returned;
        }
    }
}
