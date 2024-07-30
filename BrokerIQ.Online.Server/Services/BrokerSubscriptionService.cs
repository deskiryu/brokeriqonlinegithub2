using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BrokerIQ.Dto.CreateDto;
using BrokerIQ.Dto.Dto;
using BrokerIQ.Dto.UpdateDto;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services
{
    public class BrokerSubscriptionService : IBrokerSubscriptionService
    {
        private const string API_CONTROLLER = "BrokerSubscription";

        private readonly IMapper mapper;
        private readonly IRequestProviderService requestProviderService;
        private readonly IAccountService accountService;

        public BrokerSubscriptionService(IMapper mapper, IRequestProviderService requestProviderService, IAccountService accountService)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
            this.accountService = accountService;
        }

        public async Task<IEnumerable<BrokerSubscriptionDto>> GetAllForBroker(int brokerId)
        {
            try
            {
                var subscriptions = await requestProviderService.Get<IEnumerable<BrokerSubscriptionDto>>($"{API_CONTROLLER}/?brokerId={brokerId}");

                return mapper.Map<IEnumerable<BrokerSubscriptionDto>>(subscriptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get: exception {ex.Message}");
            }

            return Array.Empty<BrokerSubscriptionDto>();
        }

        public async Task<bool> Create(CreateBrokerSubscriptionDto subscription)
        {
            bool response = false;

            try
            {
                response = await requestProviderService.Post<CreateBrokerSubscriptionDto, bool>(API_CONTROLLER, subscription);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Create: exception {ex.Message}");
            }
            return response;
        }

        public async Task<bool> Update(UpdateBrokerSubscriptionDto subscription)
        {
            bool response = false;

            try
            {
                response = await requestProviderService.Put<UpdateBrokerSubscriptionDto, bool>(API_CONTROLLER, subscription);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update: exception {ex.Message}");
            }
            return response;
        }
    }
}