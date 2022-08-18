using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Services.Interface
{
    using Dto.Models;
    using Models;
    using BrokerIQ.Dto.Request;

    public interface IChatService
    {
        Task<Chat> Get(int customerId, int brokerId = 0);

        Task<bool> Send(string message, int customerId);

        Task<bool> Send(string message, int customerId, ChatDocument chatDocument);

        Task<int> GetUnRead(int customerId, int brokerId = 0);
    }
}
