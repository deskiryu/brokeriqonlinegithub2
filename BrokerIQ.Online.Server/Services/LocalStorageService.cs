

namespace BrokerIQ.Online.Services
{
    using System.Text.Json;
    using System.Threading.Tasks;
    using BrokerIQ.Online.Services.Interface;
    using Microsoft.AspNetCore.Components;
    using Blazored.SessionStorage;
    using System;

    public class LocalStorageService : ILocalStorageService
    {

        ISessionStorageService sessionStorage;
        public LocalStorageService(ISessionStorageService sessionStorage)
        {
            this.sessionStorage = sessionStorage;
        }

        public async Task<T> GetItem<T>(string key)
        {
            try
            {
                var returned = await sessionStorage.GetItemAsync<T>(key);
                return returned;
            }
            catch (InvalidOperationException)
            {
            }
            return default(T);     
        }

        public async Task SetItem<T>(string key, T value)
        {
            await this.sessionStorage.SetItemAsync(key, value);
        }

        public async Task DeleteItem(string key)
        {
            await this.sessionStorage.RemoveItemAsync(key);
        }
    }
}