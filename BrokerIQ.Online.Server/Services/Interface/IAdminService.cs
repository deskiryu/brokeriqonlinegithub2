using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Services.Interface
{
    using Dto.Models;
    using Models;
    using BrokerIQ.Dto.Request;

    public interface IAdminService
    {
        Task<BoolResponseDto> VerifyAdmin();

        Task<BoolResponseDto> VerifyMinorAdmin();
    }
}
