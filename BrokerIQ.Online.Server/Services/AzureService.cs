using System;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Server.Services
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using Microsoft.Extensions.Options;
    using BrokerIQ.Online.Server.AppSettings;
    using BrokerIQ.Online.Server.Models;
    using BrokerIQ.Online.Services.Interface;

    public class AzureService : IAzureService
    {
        private AzureStorageDetails azureStorageDetails;
        private BlobServiceClient blobServiceClient;
        private BlobContainerClient videoContainerClient;
        private BlobContainerClient audioContainerClient;
        private BlobContainerClient logoContainerClient;
        private bool initialised;

        public AzureService(IOptions<AzureStorageDetails> azureStorageDetails)
        {
            this.azureStorageDetails = azureStorageDetails.Value;
            this.initialised = false;
        }

        public async Task Initialise()
        {
            // Create a BlobServiceClient object which will be used to create a container client
            this.blobServiceClient = new BlobServiceClient(this.azureStorageDetails.ConnectionString);

            // Access the container and return a container client object
            this.videoContainerClient = this.blobServiceClient.GetBlobContainerClient(this.azureStorageDetails.VideoContainerName);
            this.audioContainerClient = this.blobServiceClient.GetBlobContainerClient(this.azureStorageDetails.AudioContainerName);
            this.logoContainerClient = this.blobServiceClient.GetBlobContainerClient(this.azureStorageDetails.LogoContainerName);

            this.initialised = true;
        }

        public async Task<bool> TransferAudioStreamToAzureBlob(string fileName, Stream stream, int brokerId)
        {
            if (!this.initialised)
                await Initialise();

            BlobClient blobClient = blobClient = this.audioContainerClient.GetBlobClient(fileName);

            return await TransferStreamToAzureBlob(blobClient, fileName, stream, brokerId);
        }

        public async Task<bool> TransferLogoStreamToAzureBlob(string fileName, Stream stream, int brokerId)
        {
            if (!this.initialised)
                await Initialise();

            BlobClient blobClient = blobClient = this.logoContainerClient.GetBlobClient(fileName);

            return await TransferStreamToAzureBlob(blobClient, fileName, stream, brokerId);
        }

        public async Task<bool> TransferVideoStreamToAzureBlob(string fileName, Stream stream, int brokerId)
        {
            if (!this.initialised)
                await Initialise();

            BlobClient blobClient = this.videoContainerClient.GetBlobClient(fileName);                

            return await TransferStreamToAzureBlob(blobClient, fileName, stream, brokerId);
        }

        private async Task<bool> TransferStreamToAzureBlob(BlobClient blobClient, string fileName, Stream stream, int brokerId)
        {
            Console.WriteLine("Uploading content to Blob storage as blob:\n\t {0}\n", blobClient.Uri);

            var response = await blobClient.UploadAsync(stream, true);

            var tags = new Dictionary<string,string>();
            tags.Add("Broker", brokerId.ToString());
            await blobClient.SetTagsAsync(tags);

            return response.GetRawResponse().Status == (int) HttpStatusCode.Created;
        } 

        public async Task<List<Video>> GetVideoBlobs(int brokerId)
        {
            if (!this.initialised)
                await Initialise();
            

            var videoList = new List<Video>();
            
            await foreach (BlobItem blobItem in videoContainerClient.GetBlobsAsync(BlobTraits.All))
            {
                var foundBrokerId="";
                var vetted=false;
                var birthday = false;
                if(blobItem.Tags!=null){
                    blobItem.Tags.TryGetValue("Broker", out foundBrokerId);    
                    blobItem.Tags.TryGetValue("Vetted", out string vettedVideo); 
                    if(vettedVideo != null && !string.IsNullOrEmpty(vettedVideo))
                    {
                        vetted = vettedVideo.Equals("true") ? true : false;   
                    }
                    blobItem.Tags.TryGetValue("BirthdayVideo", out string birthdayVideo);
                    if (birthdayVideo != null && !string.IsNullOrEmpty(birthdayVideo))
                    {
                        birthday = birthdayVideo.Equals("true") ? true : false;
                    }
                }
        
                if(brokerId==0 || foundBrokerId == brokerId.ToString())
                {
                    videoList.Add(new Video{
                        Name = blobItem.Name,
                        Url = this.videoContainerClient.Uri.AbsoluteUri+'/'+blobItem.Name,
                        Vetted = vetted,
                        BirthdayVideo = birthday                
                    });
                }
            }
            return videoList;
        }

        public async Task<List<Audio>> GetAudioBlobs(int brokerId)
        {
            if (!this.initialised)
                await Initialise();
            

            var audioList = new List<Audio>();
            
            await foreach (BlobItem blobItem in this.audioContainerClient.GetBlobsAsync(BlobTraits.All))
            {
                var foundBrokerId="";
                var vetted=false;
                if(blobItem.Tags!=null){
                    blobItem.Tags.TryGetValue("Broker", out foundBrokerId);    
                    blobItem.Tags.TryGetValue("Vetted", out string vettedVideo); 
                    if(vettedVideo != null && !string.IsNullOrEmpty(vettedVideo))
                    {
                        vetted = vettedVideo.Equals("true") ? true : false;   
                    }              
                }
        
                if(brokerId==0 || foundBrokerId == brokerId.ToString())
                {
                    audioList.Add(new Audio{
                        Name = blobItem.Name,
                        Url = this.audioContainerClient.Uri.AbsoluteUri+'/'+blobItem.Name,
                        Vetted = vetted                   
                    });
                }
            }
            return audioList;
        }        

        public async Task<bool> DeleteVideoBlob(string fileName)
        {
            if (!this.initialised)
                await Initialise();

            BlobClient blobClient = this.videoContainerClient.GetBlobClient(fileName);                

            return await DeleteBlob(blobClient, fileName);
        }

        public async Task<bool> DeleteAudioBlob(string fileName)
        {
            if (!this.initialised)
                await Initialise();

            BlobClient blobClient = this.audioContainerClient.GetBlobClient(fileName);
            return await DeleteBlob(blobClient, fileName);
        }

        private async Task<bool> DeleteBlob(BlobClient blobClient, string fileName)
        {
            return await blobClient.DeleteIfExistsAsync();
        }                

        public async Task<bool> IsVettedVideo(string fileName)
        {
            var vetted=false;
            BlobClient blobClient = this.videoContainerClient.GetBlobClient(fileName);
            var tags= await blobClient.GetTagsAsync();
            if(tags != null && tags.Value != null && tags.Value.Tags!=null)
            {  
                tags.Value.Tags.TryGetValue("Vetted", out string vettedVideo); 
                if(vettedVideo != null && !string.IsNullOrEmpty(vettedVideo))
                {
                    vetted = vettedVideo.Equals("true") ? true : false;   
                }
            }
            return vetted;
        }

        public async Task<(bool,string)> SetBirthdayVideo(string fileName, int brokerId, bool birthdayVideo=true)
        {
            bool succeeded = true;
            try
            {
                if (!this.initialised)
                    await Initialise();

                await foreach (BlobItem blobItem in videoContainerClient.GetBlobsAsync(BlobTraits.All))
                {
                    BlobClient blobClient = this.videoContainerClient.GetBlobClient(blobItem.Name);
                    var tags = await blobClient.GetTagsAsync();

                    var foundBrokerId = "";
                    if (blobItem.Tags != null)
                    {
                        blobItem.Tags.TryGetValue("Broker", out foundBrokerId);
                    }

                    if (foundBrokerId == brokerId.ToString())
                    {
                        //All false except the new setting
                        var newSetting = birthdayVideo ? "true" : "false";
                        var setting = blobItem.Name == fileName ? newSetting : "false";
                        var newtags = tags.Value.Tags;  
                        if(newtags.Any(x => x.Key == "BirthdayVideo"))
                        {
                            var found = newtags.FirstOrDefault(x => x.Key == "BirthdayVideo");
                            newtags.Remove(found);
                            found = new KeyValuePair<string, string>("BirthdayVideo", setting);
                            newtags.Add(found);
                        }
                        else
                        {
                            newtags.Add("BirthdayVideo", setting);
                        }
                
                        await blobClient.SetTagsAsync(newtags);
                    }

                }
            }
            catch
            {
                succeeded = false;
            }

            return (succeeded, birthdayVideo ? this.videoContainerClient.Uri.AbsoluteUri + '/' + fileName : string.Empty);
        }
    }
}
