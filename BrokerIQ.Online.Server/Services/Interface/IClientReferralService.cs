using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Threading.Tasks;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;
using Microsoft.AspNetCore.Authorization;

namespace BrokerIQ.Online.Services.Interface
{
    public interface IClientReferralService
    {
        Task<List<ClientReferral>> GetReferralsByBrokerId(int brokerId);
        Task<ClientReferral> Update(ClientReferral updateClientReferral);
        Task<bool> Delete(int id, int brokerId);
    }
}