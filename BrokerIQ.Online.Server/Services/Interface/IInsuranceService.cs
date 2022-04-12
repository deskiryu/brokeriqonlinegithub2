using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Services.Interface
{
    using Dto.Models;
    using Models;

    public interface IInsuranceService
    {
        Task<Insurance> GetInsurance(int id, bool eager = true);

        Task<Insurance> UpdateInsurance(Insurance ins);

        Task<Insurance> AddInsurance(Insurance ins);

        Task<bool> DeleteInsurance(int id);
    }
}
