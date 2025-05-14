using System.Collections.Generic;
using System.Threading.Tasks;

using BrokerIQ.Dto.Models;
using BrokerIQ.Dto.UpdateDto;

namespace BrokerIQ.Online.Services.Interface
{
    public interface IBlogService
    {
        Task<IEnumerable<BlogDto>> GetAll();

    }
}