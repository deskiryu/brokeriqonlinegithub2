using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReviewIt.Web.Services.Interface
{
    using Dto.Models;
    using Models;

    public interface IMortgageService
    {
        Task<Mortgage> GetMortgage(int id, bool eager = true);

        Task<Mortgage> UpdateMortgage(Mortgage ins);

        Task<Mortgage> AddMortgage(Mortgage ins);

        Task<bool> DeleteMortgage(int id);
    }
}
