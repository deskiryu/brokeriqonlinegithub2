using System.Threading.Tasks;
using BrokerIQ.Online.Models;

namespace BrokerIQ.Online.Server.Services.Interface;

public interface IPipedriveService
{
    Task<bool> Disconnect();

    Task<bool> RegisterConnection(string code);
    
    Task<Customer> SyncCustomer(int id);

    Task SyncChatMessages(int id);
}
