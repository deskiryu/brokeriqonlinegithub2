using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BrokerIQ.Online.AppSettings;
using BrokerIQ.Online.Server.Services.Base;
using BrokerIQ.Online.Services.Abstract;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace BrokerIQ.Online.Services.Concrete
{
    public class RequestProviderService : IRequestProviderService
    {
        protected ReviewItAPIDetails api { get; set; }

        protected string BaseUrl => $"{this.api.Url}api";

        protected string VideoConvertUrl => $"{this.api.VideoConvertUrl}";

        HttpClient _rememberhttpClient;

        public string Token { get; set; }

        public RequestProviderService(IOptions<ReviewItAPIDetails> api)
        {
            this.api = api.Value;
        }

        public async Task<bool> Post<T>(string url, T data)
        {
            HttpClient httpClient = CreateHttpClient();

            var content = new StringContent(JsonConvert.SerializeObject(data));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await httpClient.PostAsync($"{this.BaseUrl}/{url}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<TReturn> Post<T, TReturn>(string url, T data)
        {
            HttpClient httpClient = CreateHttpClient();

            var content = new StringContent(JsonConvert.SerializeObject(data));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await httpClient.PostAsync($"{this.BaseUrl}/{url}", content);
            return ConsumeResponse<TReturn>(response);
        }

        public async Task<TReturn> FirstFactorPost<T, TReturn>(string url, T data)
        {
            HttpClient httpClient = CreateHttpClient();
            _rememberhttpClient = httpClient;

            var content = new StringContent(JsonConvert.SerializeObject(data));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await _rememberhttpClient.PostAsync($"{this.BaseUrl}/{url}", content);
            return ConsumeResponse<TReturn>(response);
        }

        public async Task<TReturn> SecondFactorPost<T, TReturn>(string url, T data)
        {
            var content = new StringContent(JsonConvert.SerializeObject(data));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await _rememberhttpClient.PostAsync($"{this.BaseUrl}/{url}", content);

            var consumed = ConsumeResponse<TReturn>(response);
            DisposeClient();
            return (consumed);
        }

        public void DisposeClient()
        {
            _rememberhttpClient?.Dispose();
            _rememberhttpClient = null;
        }

        public async Task<TReturn> Post<T, TReturn>(string url, MemoryStream data, string mediaType)
        {
            HttpClient httpClient = CreateHttpClient();
            var content = new ByteArrayContent(data.ToArray());
            content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

            HttpResponseMessage response = await httpClient.PostAsync($"{this.BaseUrl}/{url}", content);
            return ConsumeResponse<TReturn>(response);
        }

        public async Task<TReturn> Post<TReturn>(string url)
        {
            HttpClient httpClient = CreateHttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(string.Empty));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await httpClient.PostAsync($"{this.BaseUrl}/{url}", content);
            return ConsumeResponse<TReturn>(response);

        }

        public async Task<TReturn> Get<TReturn>(string url, int id, bool eager = false)
        {
            string newUrl = $"{url}/{id}?eagerload={eager}";
            return await Get<TReturn>(newUrl);
        }

        public async Task<TReturn> Get<TReturn>(string url, int id, bool eager = false, int brokerId = 0)
        {
            string newUrl = $"{url}/{id}?eagerload={eager}&&brokerId={brokerId}";
            return await Get<TReturn>(newUrl);
        }

        public async Task<TReturn> Get<TReturn>(string url, Guid id)
        {
            string newUrl = $"{url}/{id}";
            return await Get<TReturn>(newUrl);
        }

        public async Task<TReturn> Get<TReturn>(string url)
        {
            HttpClient httpClient = CreateHttpClient();
            HttpResponseMessage response = await httpClient.GetAsync($"{this.BaseUrl}/{url}");
            return ConsumeResponse<TReturn>(response);
        }

        public async Task<ApiResponse<TReturn>> GetResponse<TReturn>(string url)
        {
            HttpClient httpClient = CreateHttpClient();
            HttpResponseMessage response = await httpClient.GetAsync($"{this.BaseUrl}/{url}");
            return response.IsSuccessStatusCode ? new ApiResponse<TReturn>(response.StatusCode, ConsumeResponse<TReturn>(response)) : new ApiResponse<TReturn>(response.StatusCode);
        }

        public async Task<TReturn> Put<T, TReturn>(string url, T data)
        {
            HttpClient httpClient = CreateHttpClient();

            var content = new StringContent(JsonConvert.SerializeObject(data));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await httpClient.PutAsync($"{this.BaseUrl}/{url}", content);
            return ConsumeResponse<TReturn>(response);
        }

        public async Task<TReturn> Patch<T, TReturn>(string url, T data)
        {
            HttpClient httpClient = CreateHttpClient();

            var content = new StringContent(JsonConvert.SerializeObject(data));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await httpClient.PatchAsync($"{this.BaseUrl}/{url}", content);
            return ConsumeResponse<TReturn>(response);
        }

        public async Task<bool> Delete(string url)
        {
            HttpClient httpClient = CreateHttpClient();
            HttpResponseMessage response = await httpClient.DeleteAsync($"{this.BaseUrl}/{url}");
            return response.StatusCode == HttpStatusCode.NoContent;
        }

        public async Task<bool> Delete(string url, Guid id)
        {
            string newUrl = $"{url}/{id}";
            return await Delete(newUrl);
        }

        public async Task<bool> Delete(string url, int id)
        {
            string newUrl = $"{url}/{id}";
            return await Delete(newUrl);
        }

        public async Task<TReturn> PostVideoApi<T, TReturn>(string url, MemoryStream data, string mediaType)
        {
            HttpClient httpClient = CreateHttpClient();
            var content = new ByteArrayContent(data.ToArray());
            content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

            HttpResponseMessage response = await httpClient.PostAsync($"{this.VideoConvertUrl}/{url}", content);
            return ConsumeResponse<TReturn>(response);
        }

        private HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (!string.IsNullOrEmpty(Token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            }

            return httpClient;
        }

        private T ConsumeResponse<T>(HttpResponseMessage hrm)
        {
            if (hrm.IsSuccessStatusCode && hrm.StatusCode != HttpStatusCode.Conflict)
            {
                var returned = hrm.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<T>(returned);
            }
            else
            {
                var result = hrm.Content.ReadAsStringAsync().Result;
                throw new Exception(result);
            }

            throw new Exception("Oops, it didn't work. Please email admin@brokeriq.co.uk");
        }
    }
}
