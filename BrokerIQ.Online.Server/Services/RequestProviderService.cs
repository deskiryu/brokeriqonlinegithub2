namespace BrokerIQ.Online.Services.Concrete
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Abstract;
    using AppSettings;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using BrokerIQ.Online.Services.Interface;
    using System.IO;

    public class RequestProviderService : IRequestProviderService
    {
        protected ReviewItAPIDetails api;

        protected string BaseUrl => $"{this.api.Url}api";
        protected string VideoConvertUrl => $"{this.api.VideoConvertUrl}";

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

        public async Task<TReturn> Get<TReturn>(string url, int id, bool eager=false)
        {
            string newUrl = $"{url}/{id}?eagerload={eager}";
            return await Get<TReturn>(newUrl);
        }

        public async Task<TReturn> Get<TReturn>(string url, int id, bool eager = false, int brokerId=0)
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

            throw new Exception("Oops, it didn't work. Please email admin@brokeriq.co.uk");
        }
    }
}
