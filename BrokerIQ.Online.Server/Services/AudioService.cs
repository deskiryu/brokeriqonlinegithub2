using System.Collections.Generic;
using System.Text;

namespace BrokerIQ.Online.Server.Services
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using BrokerIQ.Online.Server.Models;
    using BrokerIQ.Online.Services.Interface;

    public class AudioService : IAudioService
    {
        private readonly IAzureService azureService;

        public AudioService(IAzureService azureService)
        {
            this.azureService = azureService;
        }

        public async Task<List<Audio>> GetAudios(int brokerId)
        {
            var blobs = new List<Audio>();
            try
            {
                blobs = await azureService.GetAudioBlobs(brokerId);
            }
            catch
            {

            }
            return blobs;
        }

        public async Task<bool> UploadAudio(string fileName, string audioAsBase64, int brokerId)
        {
            bool succeeded = false;
            try
            {
                var bytes = Convert.FromBase64String(audioAsBase64);
                var stream = new MemoryStream(bytes);
                succeeded = await azureService.TransferAudioStreamToAzureBlob(fileName, stream, brokerId);
            }
            catch
            {

            }
            return succeeded;
        }

        public async Task<bool> DeleteAudio(string fileName)
        {
            bool succeeded = false;
            try
            {
                succeeded = await azureService.DeleteAudioBlob(fileName);
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
                var blobs = await azureService.GetAudioBlobs(0);
                foundName = blobs.Any(x => Path.GetFileNameWithoutExtension(x.Name).Equals(name, StringComparison.InvariantCultureIgnoreCase));
            }
            catch
            {

            }
            return !foundName;
        }
    }
}