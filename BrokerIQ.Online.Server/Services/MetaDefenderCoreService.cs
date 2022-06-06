using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BrokerIQ.Online.Server.AppSettings;
using BrokerIQ.Online.Services.Interface;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace BrokerIQ.Online.Server.Services
{
    public class MetaDefenderCoreService : IMetaDefenderCoreService
	{
        private MetaDefenderCoreDetails metaDefenderCoreDetails;
        private string targetMetaDefenderUrl;
        private readonly string fileEndpoint = "file";

        public MetaDefenderCoreService(IOptions<MetaDefenderCoreDetails> metaDefenderCoreDetails)
        {
            this.metaDefenderCoreDetails = metaDefenderCoreDetails.Value;
            targetMetaDefenderUrl = $"{this.metaDefenderCoreDetails.Url}/{this.metaDefenderCoreDetails.Version}";
        }

        /// <summary>
        /// Uploads a file to the OPSWAT service and kicks off the scanning process.
        /// </summary>
        /// <param name="fileName">Name of file to be scanned</param>
        /// <param name="data">MemoryStream representation of file</param>
        /// <returns>A dataId to be used to track the results of the scan</returns>
        public async Task<string> AnalyseFile(string fileName, MemoryStream data)
        {
            var httpClient = new HttpClient();
            
            var content = new ByteArrayContent(data.ToArray());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{targetMetaDefenderUrl}/{fileEndpoint}"),
                Method = HttpMethod.Post,
                Headers =
                {
                    { "apikey", this.metaDefenderCoreDetails.ApiKey },
                    { "filename", fileName }
                },
                Content = content,
            };

            try
            {
                HttpResponseMessage response = httpClient.SendAsync(request).Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    dynamic jsonResponse = JsonConvert.DeserializeObject<object>(responseContent);
                    return jsonResponse["data_id"];
                }
                else
                {
                    Console.WriteLine($"AnalyseFile: returned error code {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AnalyseFile: exception {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Probes OPSWAT for the results of a file scan
        /// </summary>
        /// <param name="dataId">Scan identifier</param>
        /// <returns>JSON object to be parsed by caller</returns>
        public async Task<object> FetchAnalysisResult(string dataId)
        {
            var httpClient = new HttpClient();
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{targetMetaDefenderUrl}/{fileEndpoint}/{dataId}"),
                Method = HttpMethod.Get,
                Headers =
                {
                    { "apikey", this.metaDefenderCoreDetails.ApiKey }
                },
            };

            try
            {
                var response = httpClient.SendAsync(request).Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<object>(responseContent);
                }
                else
                {
                    Console.WriteLine($"FetchAnalysisResult: returned error code {response.StatusCode} dataId {dataId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FetchAnalysisResult: exception {ex.Message}");
            }

            return null;
        }
    }
}

