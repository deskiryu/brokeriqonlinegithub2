using System.Threading.Tasks;

namespace BrokerIQ.Online.Services
{
    using Abstract;
    using AutoMapper;
    using Dto.Models;
    using Interface;
    using Models;
    using System.Collections.Generic;

    public class MenuPlanService : IMenuPlanService
    {
        private readonly string MenuPlanUrl = "MenuPlan";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;
        private readonly IAccountService accountService;

        public MenuPlanService(IRequestProviderService requestProviderService, IMapper mapper, IAccountService accountService)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
            this.accountService = accountService;
        }

        public async Task<MenuPlan> GetMenuPlan(int id)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var answer = await this.requestProviderService.Get<MenuPlanDto>(this.MenuPlanUrl, id);
            return this.mapper.Map<MenuPlan>(answer);
        }

        public async Task<IEnumerable<MenuPlan>> GetMenuPlanByCustomer(int customerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var url = this.MenuPlanUrl + $"/customer/{customerId}";
            var answer = await this.requestProviderService.Get<IEnumerable<MenuPlanDto>>(url);
            return this.mapper.Map<IEnumerable<MenuPlan>>(answer);
        }

        public async Task<MenuPlan> UpdateMenuPlan(MenuPlan ins)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var mapped = mapper.Map<UpdateMenuPlanDto>(ins);
            var answer = await this.requestProviderService.Put<UpdateMenuPlanDto, MenuPlanDto>(this.MenuPlanUrl, mapped);
            return this.mapper.Map<MenuPlan>(answer);
        }

        public async Task<MenuPlan> AddMenuPlan(MenuPlan ins)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var mapped = mapper.Map<CreateMenuPlanDto>(ins);
            var answer = await this.requestProviderService.Post<CreateMenuPlanDto, MenuPlanDto>(this.MenuPlanUrl, mapped);
            return this.mapper.Map<MenuPlan>(answer);
        }

        public async Task<bool> DeleteMenuPlan(int id)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            return await this.requestProviderService.Delete(this.MenuPlanUrl, id);
        }
    }
}
