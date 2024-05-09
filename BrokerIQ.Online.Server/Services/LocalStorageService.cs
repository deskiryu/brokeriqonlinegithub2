

namespace BrokerIQ.Online.Services
{
    using System.Text.Json;
    using System.Threading.Tasks;
    using BrokerIQ.Online.Services.Interface;
    using Microsoft.AspNetCore.Components;
    using Blazored.SessionStorage;
    using System;
    using BrokerIQ.Online.Server.Helper;
    using Microsoft.JSInterop;

    public class LocalStorageService : ILocalStorageService
    {
        [Inject]
        protected IJSRuntime js { get; set; }

        ISessionStorageService sessionStorage;
        public LocalStorageService(ISessionStorageService sessionStorage)
        {
            this.sessionStorage = sessionStorage;
        }

        public async Task<T> GetItem<T>(string key)
        {
            var isRunningWasm = await RunningWasm.IsWebAssembly(js);
            if (isRunningWasm)
            {
                var returned = await sessionStorage.GetItemAsync<T>(key);
                return returned;
            }
            else
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