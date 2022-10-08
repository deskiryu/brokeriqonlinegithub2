using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BrokerIQ.Dto.Enum;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Server.Extensions;
using BrokerIQ.Online.Server.Models;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Server.Services
{
    public class BrokerDefinedMessageService : IBrokerDefinedMessageService
    {
        private readonly string brokerDefinedMessageUrl = "BrokerDefinedMessage";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;
        private readonly IAccountService accountService;

        public BrokerDefinedMessageService(IMapper mapper, IRequestProviderService requestProviderService, IAccountService accountService)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
            this.accountService = accountService;
        }

        public async Task<BrokerDefinedMessage> Get()
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var brokerId = user.MasterBrokerId;

            var url = this.brokerDefinedMessageUrl + $"/{brokerId}";
            try
            {
                var messagesDto = await requestProviderService.Get<BrokerDefinedMessageDto>(url);
                return this.mapper.Map<BrokerDefinedMessage>(messagesDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get: exception {ex.Message}");
            }

            return null;
        }

        public async Task<bool> UpdateOrCreate(List<DefinedMessagesDto> definedMessages)
        {
            var url = this.brokerDefinedMessageUrl;
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var brokerId = user.MasterBrokerId;
            CreateBrokerDefinedMessageDto brokerDefinedMessage = new CreateBrokerDefinedMessageDto
            {
                BrokerId = brokerId,
                BrokerDefinedMessages = definedMessages
            };

            bool response = false;
            try 
            {
                response = await requestProviderService.Post<CreateBrokerDefinedMessageDto, bool>(url, brokerDefinedMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateOrCreate: exception {ex.Message}");
            }
            return response;
        }
    }
}