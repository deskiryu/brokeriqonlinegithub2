using System;
using System.IO;
using System.Threading.Tasks;
using BrokerIQ.Dto.Response;
using BrokerIQ.Online.Models.Account;
using BrokerIQ.Online.Server.Services.Base;

namespace BrokerIQ.Online.Services.Abstract
{
    public interface IRequestProviderService
    {
        Task<bool> Post<T>(string url, T data);

        Task<TReturn> Post<T, TReturn>(string url, T data);

        Task<LoginResponseDto> FirstFactorPost(string url, Login data);

        Task<LoginResponseDto> SecondFactorPost(string url, Login data);

        Task<TReturn> Post<T, TReturn>(string url, MemoryStream data, string mediaType);

        Task<TReturn> Post<TReturn>(string url);

        Task<TReturn> Get<TReturn>(string url);

        Task<ApiResponse<TReturn>> GetResponse<TReturn>(string url);

        Task<TReturn> Get<TReturn>(string url, int id, bool eager = false);

        Task<TReturn> Get<TReturn>(string url, int id, bool eager, int brokerId = 0);

        Task<TReturn> Get<TReturn>(string url, Guid id);

        Task<TReturn> Put<T, TReturn>(string url, T data);

        Task<TReturn> Patch<T, TReturn>(string url, T data);

        Task<bool> Delete(string url);

        Task<bool> Delete(string url, int id);

        Task<bool> Delete(string url, Guid id);

        Task<TReturn> PostVideoApi<T, TReturn>(string url, MemoryStream data, string mediaType);
    }
}
