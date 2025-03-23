using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BrokerIQ.Dto.Entities;
using BrokerIQ.Dto.Models;
using BrokerIQ.Dto.UpdateDto;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services
{
    public class CustomerGoalService : ICustomerGoalService
    {
        private readonly string API_CONTROLLER = "CustomerGoal";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;

        public CustomerGoalService(IRequestProviderService requestProviderService, IMapper mapper)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
        }

        public async Task<IEnumerable<CustomerGoalDto>> Get(int customerId)
        {
           return await requestProviderService.Get<IEnumerable<CustomerGoalDto>>(this.API_CONTROLLER + @"/bycustomer/" + $"{customerId}");
        }

        public async Task<bool> Create(CreateCustomerGoalDto Goal)
        {
            try
            {
                await requestProviderService.Post<CreateCustomerGoalDto, CustomerGoalDto>(API_CONTROLLER, Goal);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Create: exception {ex.Message}");
            }

            return false;
        }

        public async Task<bool> Update(UpdateCustomerGoalDto Goal)
        {
            try
            {
                await requestProviderService.Put<UpdateCustomerGoalDto, CustomerGoalDto>(API_CONTROLLER, Goal);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateOrCreate: exception {ex.Message}");
            }

            return false;
        }

        public async Task<bool> Delete(int id)
        {
            return await this.requestProviderService.Delete(this.API_CONTROLLER, id);
        }
    }
}
