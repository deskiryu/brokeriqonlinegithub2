using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Services.Interface
{
    using Dto.Models;
    using Models;
    using BrokerIQ.Dto.Request;

    public interface IBrokerService
    {
        Task<Broker> GetBroker(int id);

        Task<IEnumerable<Broker>> GetBrokers();

        Task<IEnumerable<Broker>> GetBrokersByList(IEnumerable<int> ids);

        Task<Broker> UpdateBroker(Broker ins);

        Task<Broker> AddBroker(Broker ins);

        Task<bool> DeleteBroker(int id);

        Task<bool> UpdateBrokerBirthdayVideoUrl(int id, string url);

        Task<BoolResponseDto> VerifyBroker(int id);
    }
}
