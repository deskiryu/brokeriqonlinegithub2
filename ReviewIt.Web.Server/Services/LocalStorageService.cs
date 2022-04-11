

namespace ReviewIt.Web.Services
{
    using Microsoft.JSInterop;
    using System.Text.Json;
    using System.Threading.Tasks;
    using ReviewIt.Web.Services.Interface;
    using Microsoft.AspNetCore.Components;
    using Blazored.SessionStorage;

    public class LocalStorageService : ILocalStorageService
    {

        ISessionStorageService sessionStorage;
        public LocalStorageService(IJSRuntime jsRuntime, ISessionStorageService sessionStorage)
        {
            this.sessionStorage = sessionStorage;
        }

        public async Task<T> GetItem<T>(string key)
        {
            var returned = await sessionStorage.GetItemAsync<T>(key); 
            return returned;      
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