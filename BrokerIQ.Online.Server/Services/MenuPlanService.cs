using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services
{
    public class MenuPlanService : IMenuPlanService
    {
        private readonly string MenuPlanUrl = "MenuPlan";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;

        public MenuPlanService(IRequestProviderService requestProviderService, IMapper mapper)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
        }

        public async Task<MenuPlan> GetMenuPlan(int id)
        {
            var answer = await this.requestProviderService.Get<MenuPlanDto>(this.MenuPlanUrl, id);
            return this.mapper.Map<MenuPlan>(answer);
        }

        public async Task<IEnumerable<MenuPlan>> GetMenuPlanByCustomer(int customerId)
        {
            var url = this.MenuPlanUrl + $"/customer/{customerId}";
            var answer = await this.requestProviderService.Get<IEnumerable<MenuPlanDto>>(url);
            return this.mapper.Map<IEnumerable<MenuPlan>>(answer);
        }

        public async Task<MenuPlan> UpdateMenuPlan(MenuPlan ins)
        {
            var mapped = mapper.Map<UpdateMenuPlanDto>(ins);
            var answer = await this.requestProviderService.Put<UpdateMenuPlanDto, MenuPlanDto>(this.MenuPlanUrl, mapped);
            return this.mapper.Map<MenuPlan>(answer);
        }

        public async Task<MenuPlan> AddMenuPlan(MenuPlan ins)
        {
            var mapped = mapper.Map<CreateMenuPlanDto>(ins);
            var answer = await this.requestProviderService.Post<CreateMenuPlanDto, MenuPlanDto>(this.MenuPlanUrl, mapped);
            return this.mapper.Map<MenuPlan>(answer);
        }

        public async Task<bool> DeleteMenuPlan(int id)
        {
            return await this.requestProviderService.Delete(this.MenuPlanUrl, id);
        }
    }
}
