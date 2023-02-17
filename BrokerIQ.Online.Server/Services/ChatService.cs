using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Services
{
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using Abstract;
    using AppSettings;
    using AutoMapper;
    using Dto.Models;
    using Interface;
    using Models;
    using BrokerIQ.Dto.Request;

    public class ChatService : IChatService
    {
        private readonly string ChatUrl = "Chat";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;
        private readonly IAccountService accountService;

        public ChatService(IRequestProviderService requestProviderService, IMapper mapper, IAccountService accountService)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
            this.accountService = accountService;
        }

        public async Task<Chat> Get(int customerId, int brokerId=0)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var localBrokerId = brokerId > 0 ? brokerId :  user.MasterBrokerId;
            string newUrl = this.ChatUrl + $"/{customerId}/{localBrokerId}?markAsReadByBroker=true";

            var ChatDto = await requestProviderService.Get<ChatDto>(newUrl);
            var chat = mapper.Map<Chat>(ChatDto);

            return chat;
        }

        public async Task<bool> Send(string message, int customerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var brokerId = user.MasterBrokerId;
            var createChatMessage = new CreateChatMessageDto
            {
                BrokerId = brokerId,
                CustomerId = customerId,
                Message = message,
                BrokerSource = true
            };

            var answer = await requestProviderService.Post<CreateChatMessageDto,ChatMessageDto>(this.ChatUrl, createChatMessage);
            return answer!=null&&answer.Id>0;
        }

        public async Task<bool> SendWithDoc(string message, int customerId, ChatDocument chatDocument)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var brokerId = user.MasterBrokerId;
            var createChatMessage = new CreateChatMessageDto
            {
                BrokerId = brokerId,
                CustomerId = customerId,
                Message = message,
                BrokerSource = true,
                ChatDocument = new CreateChatDocumentDto
                {
                     File = chatDocument.File,
                     FileName = chatDocument.FileName
                }
            };

            var answer = await requestProviderService.Post<CreateChatMessageDto, ChatMessageDto>(this.ChatUrl, createChatMessage);
            return answer != null && answer.Id > 0;
        }

        public async Task<int> GetUnRead(int customerId, int brokerId = 0)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var localBrokerId = brokerId > 0 ? brokerId : user.MasterBrokerId;
            string newUrl = this.ChatUrl + $"/UnreadByBroker/{customerId}/{localBrokerId}";

            var count = await requestProviderService.Get<int>(newUrl);
            return count;
        }

        public async Task<bool> SendMultiple(string message, List<int> listCustomerId, int brokerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var createChatMessage = new CreateChatMessageDto
            {
                BrokerId = brokerId,
                CustomerIdList = listCustomerId,
                Message = message,
                BrokerSource = true
            };

            var answer = await requestProviderService.Post<CreateChatMessageDto, bool>(this.ChatUrl+"/multiple", createChatMessage);
            return answer;
        }

        public async Task<bool> SendMultipleWithDoc(string message, List<int> listCustomerId, int brokerId, ChatDocument chatDocument)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var createChatMessage = new CreateChatMessageDto
            {
                BrokerId = brokerId,
                CustomerIdList = listCustomerId,
                Message = message,
                BrokerSource = true,
                ChatDocument = new CreateChatDocumentDto
                {
                    File = chatDocument.File,
                    FileName = chatDocument.FileName
                }
            };

            var answer = await requestProviderService.Post<CreateChatMessageDto, bool>(this.ChatUrl + "/multiple", createChatMessage);
            return answer;
        }

        public async Task<bool> SendMultipleAppLink(List<int> listCustomerId, int brokerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var createChatMessage = new CreateChatMessageDto
            {
                BrokerId = brokerId,
                CustomerIdList = listCustomerId,
                BrokerSource = true
            };

            var answer = await requestProviderService.Post<CreateChatMessageDto, bool>(this.ChatUrl + "/multipleAppLink", createChatMessage);
            return answer;
        }
    }
}
