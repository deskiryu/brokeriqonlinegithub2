using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Services.Interface
{
    using Dto.Models;
    using Models;

    public interface IMenuPlanService
    {
        Task<MenuPlan> GetMenuPlan(int id);

        Task<IEnumerable<MenuPlan>> GetMenuPlanByCustomer(int customerId);

        Task<MenuPlan> UpdateMenuPlan(MenuPlan ins);

        Task<MenuPlan> AddMenuPlan(MenuPlan ins);

        Task<bool> DeleteMenuPlan(int id);
    }
}
