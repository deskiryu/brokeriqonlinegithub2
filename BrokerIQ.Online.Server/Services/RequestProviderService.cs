using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BrokerIQ.Dto.Response;
using BrokerIQ.Online.AppSettings;
using BrokerIQ.Online.Models.Account;
using BrokerIQ.Online.Server.Data;
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

        private readonly CookieService _cookieService;

        public RequestProviderService(IOptions<ReviewItAPIDetails> api, CookieService cookieService)
        {
            this.api = api.Value;
            this._cookieService = cookieService;
        }

        public async Task<bool> Post<T>(string url, T data)
        {
            HttpClient httpClient = await CreateHttpClient();

            var content = new StringContent(JsonConvert.SerializeObject(data));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await httpClient.PostAsync($"{this.BaseUrl}/{url}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<TReturn> Post<T, TReturn>(string url, T data)
        {
            HttpClient httpClient = await CreateHttpClient();
            var asJson = JsonConvert.SerializeObject(data);
            var content = new StringContent(asJson);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await httpClient.PostAsync($"{this.BaseUrl}/{url}", content);
            return ConsumeResponse<TReturn>(response);
        }

        public async Task<LoginResponseDto> FirstFactorPost(string url, Login data)
        {
            HttpClient httpClient = await CreateHttpClient();
            _rememberhttpClient = httpClient;

            var content = new StringContent(JsonConvert.SerializeObject(data));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await _rememberhttpClient.PostAsync($"{this.BaseUrl}/{url}", content);

            var dto = ConsumeResponse<LoginResponseDto>(response);

            if (!dto.RequiresTwoFactor) {
                await SetAccessTokens(dto);
            }

            DisposeClient();

            return dto;
        }

        public async Task<LoginResponseDto> SecondFactorPost(string url, Login data)
        {
            var content = new StringContent(JsonConvert.SerializeObject(data));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await _rememberhttpClient.PostAsync($"{this.BaseUrl}/{url}", content);

            var dto = ConsumeResponse<LoginResponseDto>(response);

            await SetAccessTokens(dto);

            DisposeClient();

            return (dto);
        }

        public void DisposeClient()
        {
            _rememberhttpClient?.Dispose();
            _rememberhttpClient = null;
        }

        public async Task<TReturn> Post<T, TReturn>(string url, MemoryStream data, string mediaType)
        {
            HttpClient httpClient = await CreateHttpClient();
            var content = new ByteArrayContent(data.ToArray());
            content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

            HttpResponseMessage response = await httpClient.PostAsync($"{this.BaseUrl}/{url}", content);
            return ConsumeResponse<TReturn>(response);
        }

        public async Task<TReturn> Post<TReturn>(string url)
        {
            HttpClient httpClient = await CreateHttpClient();
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
            HttpClient httpClient = await CreateHttpClient();
            HttpResponseMessage response = await httpClient.GetAsync($"{this.BaseUrl}/{url}");
            return ConsumeResponse<TReturn>(response);
        }

        public async Task<ApiResponse<TReturn>> GetResponse<TReturn>(string url)
        {
            HttpClient httpClient = await CreateHttpClient();
            HttpResponseMessage response = await httpClient.GetAsync($"{this.BaseUrl}/{url}");
            return response.IsSuccessStatusCode ? new ApiResponse<TReturn>(response.StatusCode, ConsumeResponse<TReturn>(response)) : new ApiResponse<TReturn>(response.StatusCode);
        }

        public async Task<TReturn> Put<T, TReturn>(string url, T data)
        {
            HttpClient httpClient = await CreateHttpClient();

            var content = new StringContent(JsonConvert.SerializeObject(data));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await httpClient.PutAsync($"{this.BaseUrl}/{url}", content);
            return ConsumeResponse<TReturn>(response);
        }

        public async Task<TReturn> Patch<T, TReturn>(string url, T data)
        {
            HttpClient httpClient = await CreateHttpClient();

            var content = new StringContent(JsonConvert.SerializeObject(data));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await httpClient.PatchAsync($"{this.BaseUrl}/{url}", content);
            return ConsumeResponse<TReturn>(response);
        }

        public async Task<bool> Delete(string url)
        {
            HttpClient httpClient = await CreateHttpClient();
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
            HttpClient httpClient = await CreateHttpClient();
            var content = new ByteArrayContent(data.ToArray());
            content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

            HttpResponseMessage response = await httpClient.PostAsync($"{this.VideoConvertUrl}/{url}", content);
            return ConsumeResponse<TReturn>(response);
        }

        private async Task<HttpClient> CreateHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            await CheckAuthCookies(httpClient);

            var token = await _cookieService.GetCookieAsync(ApplicationKeys.ACCESS_TOKEN_KEY);

            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return httpClient;
        }

        private async Task CheckAuthCookies(HttpClient httpClient)
        {
            var expirationValue = await _cookieService.GetCookieAsync(ApplicationKeys.ACCESS_EXPIRATION_KEY);
            var wasParsed = DateTime.TryParse(WebUtility.UrlDecode(expirationValue), out DateTime expiration);

            if (wasParsed && expiration <= DateTime.UtcNow)
            {
                var refreshToken = WebUtility.UrlDecode(await _cookieService.GetCookieAsync(ApplicationKeys.REFRESH_TOKEN_KEY));

                var content = new StringContent($"\"{refreshToken}\"");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await httpClient.PostAsync($"{BaseUrl}/auth/refresh", content);

                if (!response.IsSuccessStatusCode)
                {
                    await _cookieService.DeleteCookieAsync(ApplicationKeys.ACCESS_TOKEN_KEY);
                    await _cookieService.DeleteCookieAsync(ApplicationKeys.ACCESS_EXPIRATION_KEY);
                    await _cookieService.DeleteCookieAsync(ApplicationKeys.REFRESH_TOKEN_KEY);
                    return;
                }

                var loginDto = ConsumeResponse<LoginResponseDto>(response);

                await SetAccessTokens(loginDto);
            }
        }

        private async Task SetAccessTokens(LoginResponseDto dto)
        {
            await _cookieService.SetCookieAsync(ApplicationKeys.ACCESS_TOKEN_KEY, dto.Token, 15);
            await _cookieService.SetCookieAsync(ApplicationKeys.ACCESS_EXPIRATION_KEY, dto.TokenExpirationDate.ToString("s"), 15);
            await _cookieService.SetCookieAsync(ApplicationKeys.REFRESH_TOKEN_KEY, dto.RefreshToken, 15);
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
