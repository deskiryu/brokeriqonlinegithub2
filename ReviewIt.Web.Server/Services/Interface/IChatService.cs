using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReviewIt.Web.Services.Interface
{
    using Dto.Models;
    using Models;
    using ReviewIt.Dto.Request;

    public interface IChatService
    {
        Task<Chat> Get(int customerId, int brokerId = 0);

        Task<bool> Send(string message, int customerId);
    }
}
