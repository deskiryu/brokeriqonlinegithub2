using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Server.Services.Base;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services
{
    public class BrokerDefinedMessageService : BIQService, IBrokerDefinedMessageService
    {
        private const string API_CONTROLLER = "BrokerDefinedMessage";

        private readonly IMapper mapper;

        public BrokerDefinedMessageService(IAccountService accountService, IRequestProviderService requestProviderService, IMapper mapper)
            : base(accountService, requestProviderService)
        {
            this.mapper = mapper;
        }

        public async Task<IEnumerable<BrokerDefinedMessageDto>> GetAllForCurrentBroker()
        {
            var brokerId = await GetCurrentBrokerId();

            try
            {
                var messages = await _requestProviderService.Get<IEnumerable<BrokerDefinedMessageDto>>($"{API_CONTROLLER}/{brokerId}");

                return mapper.Map<IEnumerable<BrokerDefinedMessageDto>>(messages);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get: exception {ex.Message}");
            }

            return Array.Empty<BrokerDefinedMessageDto>();
        }

        public async Task<bool> Create(CreateBrokerDefinedMessageDto message)
        {
            message.BrokerId = await GetCurrentBrokerId();

            try
            {
                var response = await _requestProviderService.Post<CreateBrokerDefinedMessageDto, BrokerDefinedMessageDto>(API_CONTROLLER, message);

                return response.Id > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Create: exception {ex.Message}");

                return false;
            }
        }

        public async Task<bool> Update(BrokerDefinedMessageDto message)
        {
            message.BrokerId = await GetCurrentBrokerId();

            try
            {
                var response = await _requestProviderService.Put<BrokerDefinedMessageDto, BrokerDefinedMessageDto>(API_CONTROLLER, message);

                return response.Id > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update: exception {ex.Message}");

                return false;
            }
        }

        public async Task<bool> Delete(BrokerDefinedMessageDto message)
        {
            try
            {
                return await _requestProviderService.Delete($"{API_CONTROLLER}?brokerId={message.BrokerId}&id={message.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete: exception {ex.Message}");

                return false;
            }
        }
    }
}