namespace BrokerIQ.Online.Services.Interface
{
    using System.Threading.Tasks;

    public interface ILocalStorageService
    {
        Task<T> GetItem<T>(string key);
        Task SetItem<T>(string key, T value);
        Task DeleteItem(string key);
    }
}
