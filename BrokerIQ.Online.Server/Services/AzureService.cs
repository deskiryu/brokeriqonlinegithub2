using System;
using System.Globalization;
using System.ComponentModel;
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
    using System.Text;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage;

    public class AzureService : IAzureService
    {
        private AzureStorageDetails azureStorageDetails;
        private BlobServiceClient blobServiceClient;
        private CloudStorageAccount cloudStorageAccount;
        private CloudBlobClient cloudBlobClient;
        private BlobContainerClient videoContainerClient;
        private BlobContainerClient audioContainerClient;
        private BlobContainerClient logoContainerClient;
        private CloudBlobContainer videoThumbnailContainer;
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

            // Separate CloudBloudClient required to retrieve video thumbnail container (quirk of Azure function BlobTrigger)
            this.cloudStorageAccount = CloudStorageAccount.Parse(this.azureStorageDetails.ConnectionString);
            this.cloudBlobClient = this.cloudStorageAccount.CreateCloudBlobClient();
            this.videoThumbnailContainer = this.cloudBlobClient.GetContainerReference(this.azureStorageDetails.VideoThumbnailContainerName);

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

        public async Task<bool> TransferVideoStreamToAzureBlob(string fileName, DateTime? sendDate, Stream stream, int brokerId)
        {
            if (!this.initialised)
                await Initialise();

            BlobClient blobClient = this.videoContainerClient.GetBlobClient(fileName);
            var result = await TransferStreamToAzureBlob(blobClient, fileName, stream, brokerId);
            var tags = new Dictionary<string,string>();
            tags.Add("Broker", brokerId.ToString());
         
            await blobClient.SetTagsAsync(tags);
            return result;
        }

        private async Task<bool> TransferStreamToAzureBlob(BlobClient blobClient, string fileName, Stream stream, int brokerId)
        {
            Console.WriteLine("Uploading content to Blob storage as blob:\n\t {0}\n", blobClient.Uri);

            var response = await blobClient.UploadAsync(stream, true);
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
                var sendTick = false;

                DateTime? sendDate = null;

                if(blobItem.Tags!=null){
                    blobItem.Tags.TryGetValue("Broker", out foundBrokerId);    
                    blobItem.Tags.TryGetValue("Vetted", out string vettedVideo); 
                    blobItem.Tags.TryGetValue("BirthdayVideo", out string birthdayVideo);
                    blobItem.Tags.TryGetValue("SendDate", out string sendDateStr);
                    blobItem.Tags.TryGetValue("SendDateTick", out string sendDateTickStr);

                    if (vettedVideo != null && !string.IsNullOrEmpty(vettedVideo))
                    {
                        vetted = vettedVideo.Equals("true") ? true : false;   
                    }
                    if (birthdayVideo != null && !string.IsNullOrEmpty(birthdayVideo))
                    {
                        birthday = birthdayVideo.Equals("true") ? true : false;
                    }
                    if (sendDateTickStr != null && !string.IsNullOrEmpty(sendDateTickStr))
                    {
                        sendTick = sendDateTickStr.Equals("true") ? true : false;
                    }
                    if (sendDateStr!= null && !string.IsNullOrEmpty(sendDateStr))
                    {
                        if(DateTime.TryParse(sendDateStr, System.Globalization.CultureInfo.GetCultureInfo("en-GB"),
                            System.Globalization.DateTimeStyles.None, out DateTime trySendDate))
                        {
                            sendDate = trySendDate;
                        }
                    }
                }
        
                if(brokerId==0 || foundBrokerId == brokerId.ToString())
                {
                    videoList.Add(new Video{
                        Name = blobItem.Name,
                        Url = this.videoContainerClient.Uri.AbsoluteUri+'/'+blobItem.Name,
                        Vetted = vetted,
                        UploadDate = blobItem.Properties.LastModified,
                        SendDate = sendDate,
                        BirthdayVideo = birthday,
                        SendDateTick = sendTick
                    });
                }
            }
            return videoList;
        }

        public async Task<Dictionary<string, VideoThumbnail>> GetVideoThumbnailBlobs(int brokerId)
        {
            if (!this.initialised)
                await Initialise();

            var videoThumbnails = new Dictionary<string, VideoThumbnail>();

            // ListBlobsSegmentedAsync override required to fetch metadata from the blob 
            BlobResultSegment blobResultSegment = await videoThumbnailContainer.ListBlobsSegmentedAsync(
                prefix: null,useFlatBlobListing: true, blobListingDetails: BlobListingDetails.Metadata,
                maxResults: null, currentToken: null, options: null, operationContext: null,
                cancellationToken: default);

            foreach (IListBlobItem item in blobResultSegment.Results)
            {
                CloudBlockBlob blob = (CloudBlockBlob)item;
                string foundBrokerId = "";
                blob.Metadata.TryGetValue("Broker", out foundBrokerId);

                if (brokerId == 0 || foundBrokerId == brokerId.ToString()) {
                    MemoryStream ms = new MemoryStream();
                    await blob.DownloadToStreamAsync(ms);
                    videoThumbnails.Add(blob.Name,
                        new VideoThumbnail
                        {
                            Data = String.Format("data:image/jpeg;base64,{0}", Convert.ToBase64String(ms.ToArray()))
                        });
                }
            }

            return videoThumbnails;
        }

        public async Task<VideoThumbnail> GetVideoThumbnailBlob(string fileName)
        {
            if (!this.initialised)
                await Initialise();

            VideoThumbnail videoThumbnail = null;
            CloudBlockBlob blob = videoThumbnailContainer.GetBlockBlobReference(fileName);

            if (blob != null) {
                MemoryStream ms = new MemoryStream();
                await blob.DownloadToStreamAsync(ms);

                videoThumbnail = new VideoThumbnail
                {
                    Data = String.Format("data:image/jpeg;base64,{0}", Convert.ToBase64String(ms.ToArray()))
                };
            }

            return videoThumbnail;
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

        public async Task<bool> DeleteVideoThumbnailBlob(string fileName)
        {
            if (!this.initialised)
                await Initialise();

            CloudBlockBlob blob = this.videoThumbnailContainer.GetBlockBlobReference(fileName);

            return await blob.DeleteIfExistsAsync();
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

                    // If this broker is the uploading broker -- admin cannot set birthday video
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

        public async Task<(bool,string)> SetVideoSendDate(string fileName, int brokerId, DateTime? sendDate)
        {
            bool succeeded = true;
                try
                {
                    if (!this.initialised)
                        await Initialise();

                    await foreach (BlobItem blobItem in videoContainerClient.GetBlobsAsync(BlobTraits.All))
                    {
                        if (blobItem.Name == fileName) {
                            BlobClient blobClient = this.videoContainerClient.GetBlobClient(blobItem.Name);
                            var tags = await blobClient.GetTagsAsync();

                            var foundBrokerId = "";
                            if (blobItem.Tags != null)
                            {
                                blobItem.Tags.TryGetValue("Broker", out foundBrokerId);
                            }

                            // If this broker is the uploading broker, OR Admin user (0)
                            if (foundBrokerId == brokerId.ToString() || brokerId == 0)
                            {
                                var newtags = tags.Value.Tags;
                                newtags["SendDate"] = ((DateTimeOffset)sendDate).ToString("g", new CultureInfo("en-GB"));
                                await blobClient.SetTagsAsync(newtags);
                            }
                            break;
                        }
                    }
                }
                catch
                {
                    succeeded = false;
                }

                return (succeeded, succeeded ? this.videoContainerClient.Uri.AbsoluteUri + '/' + fileName : string.Empty);
            
        }

        public async Task<(bool, string)> SetVideoSendDateTick(string fileName, int brokerId, bool value)
        {
            bool succeeded = true;
            try
            {
                if (!this.initialised)
                    await Initialise();

                await foreach (BlobItem blobItem in videoContainerClient.GetBlobsAsync(BlobTraits.All))
                {
                    if (blobItem.Name == fileName)
                    {
                        BlobClient blobClient = this.videoContainerClient.GetBlobClient(blobItem.Name);
                        var tags = await blobClient.GetTagsAsync();

                        var foundBrokerId = "";
                        if (blobItem.Tags != null)
                        {
                            blobItem.Tags.TryGetValue("Broker", out foundBrokerId);
                        }

                        // If this broker is the uploading broker, OR Admin user (0)
                        if (foundBrokerId == brokerId.ToString() || brokerId == 0)
                        {
                            var newtags = tags.Value.Tags;
                            var newSetting = value ? "true" : "false";
                            newtags["SendDateTick"] = newSetting;
                            await blobClient.SetTagsAsync(newtags);
                        }
                        break;
                    }
                }
            }
            catch
            {
                succeeded = false;
            }

            return (succeeded, succeeded ? this.videoContainerClient.Uri.AbsoluteUri + '/' + fileName : string.Empty);

        }

    }
}