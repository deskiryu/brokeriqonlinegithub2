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
    using Microsoft.AspNetCore.Components;

    public class AudioService : IAudioService
    {
        private readonly IAzureService azureService;

        [Inject]
        public IMetaDefenderCoreService MetaDefenderCoreService { get; set; }

        public bool AudioUploading { get; set; }

        public bool AudioScanUploading { get; set; }

        public bool AudioScanning { get; set; }

        public int AudioScanningProgress { get; set; }

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

        public async Task<bool> UploadAudio(string fileName, MemoryStream audioStream, int brokerId)
        {
            bool succeeded = false;
            try
            {
                succeeded = await azureService.TransferAudioStreamToAzureBlob(fileName, audioStream, brokerId);
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