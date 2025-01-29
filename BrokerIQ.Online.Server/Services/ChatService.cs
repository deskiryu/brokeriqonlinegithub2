using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using BrokerIQ.Dto.Models;
using BrokerIQ.Dto.Request;
using BrokerIQ.Online.AppSettings;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Services.Base;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;
using Microsoft.Extensions.Options;

namespace BrokerIQ.Online.Services
{
    public class ChatService : IChatService
    {
        private readonly string ChatUrl = "Chat";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;
        private readonly IAccountService accountService;
        private readonly ReviewItAPIDetails api;

        public ChatService(IRequestProviderService requestProviderService, IMapper mapper, IAccountService accountService, IOptions<ReviewItAPIDetails> api)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
            this.accountService = accountService;
            this.api = api.Value;
        }

        public async Task<Chat> Get(int customerId, int brokerId = 0)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var localBrokerId = brokerId > 0 ? brokerId : user.MasterBrokerId;
            string newUrl = this.ChatUrl + $"/{customerId}/{localBrokerId}?markAsReadByBroker=true";

            var ChatDto = await requestProviderService.Get<ChatDto>(newUrl);
            var chat = mapper.Map<Chat>(ChatDto);
            if (this.api.IsYAHTheme)
            {
                if (chat != null && chat.Messages != null)
                {
                    chat.Messages = chat.Messages.Select(c => { c.YahTheme = true; return c; }).ToList();
                }

            }
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
                BrokerSource = true,
                NoNotification = false,
                HasEmbeddedUrl = !string.IsNullOrEmpty(message) && message.Contains("<--") && message.Contains("-->")
            };

            var answer = await requestProviderService.Post<CreateChatMessageDto, ChatMessageDto>(this.ChatUrl, createChatMessage);
            return answer != null && answer.Id > 0;
        }

        public async Task<bool> SendWithDoc(string message, int customerId, ChatDocument chatDocument, bool NoNotification = false)
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
                NoNotification = NoNotification,
                HasEmbeddedUrl = !string.IsNullOrEmpty(message) && message.Contains("<--") && message.Contains("-->"),
                ChatDocument = new CreateChatDocumentDto
                {
                    File = chatDocument.File,
                    FileName = chatDocument.FileName,
                    SupportingDocumentType = chatDocument.SupportingDocumentType,
                }
            };

            var answer = await requestProviderService.Post<CreateChatMessageDto, ChatMessageDto>(this.ChatUrl, createChatMessage);
            return answer != null && answer.Id > 0;
        }

        public async Task<ApiResponse<int>> GetUnRead(int customerId, int brokerId = 0)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var localBrokerId = brokerId > 0 ? brokerId : user.MasterBrokerId;
            string newUrl = this.ChatUrl + $"/UnreadByBroker/{customerId}/{localBrokerId}";

            var count = await this.requestProviderService.GetResponse<int>(newUrl);
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
                BrokerSource = true,
                NoNotification = false,
                HasEmbeddedUrl = !string.IsNullOrEmpty(message) && message.Contains("<--") && message.Contains("-->")
            };

            var answer = await requestProviderService.Post<CreateChatMessageDto, bool>(this.ChatUrl + "/multiple", createChatMessage);
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
                NoNotification = false,
                HasEmbeddedUrl = !string.IsNullOrEmpty(message) && message.Contains("<--") && message.Contains("-->"),
                ChatDocument = new CreateChatDocumentDto
                {
                    File = chatDocument.File,
                    FileName = chatDocument.FileName,
                    SupportingDocumentType = chatDocument.SupportingDocumentType
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
                BrokerSource = true,
                NoNotification = false,
            };

            var answer = await requestProviderService.Post<CreateChatMessageDto, bool>(this.ChatUrl + "/multipleAppLink", createChatMessage);
            return answer;
        }

        public async Task<bool> SendMultipleVideoLink(string message, List<int> listCustomerId, int brokerId, string videoUrl, string VideoThumbnailData)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;

            var result = Regex.Replace(VideoThumbnailData, @"^data:image\/[a-zA-Z]+;base64,", string.Empty);
            byte[] bytes = Convert.FromBase64String(result);


            var createChatMessage = new CreateChatMessageDto
            {
                Message = message,
                BrokerId = brokerId,
                CustomerIdList = listCustomerId,
                BrokerSource = true,
                VideoUrl = videoUrl,
                Image = bytes,
                IsVideo = true,
                NoNotification = false,
                HasEmbeddedUrl = !string.IsNullOrEmpty(message) && message.Contains("<--") && message.Contains("-->"),
            };

            var answer = await requestProviderService.Post<CreateChatMessageDto, bool>(this.ChatUrl + "/multiple", createChatMessage);
            return answer;
        }

        public async Task<bool> SendMultipleAudioLink(string message, List<int> listCustomerId, int brokerId, string audioUrl)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;

            var createChatMessage = new CreateChatMessageDto
            {
                Message = message,
                BrokerId = brokerId,
                CustomerIdList = listCustomerId,
                BrokerSource = true,
                AudioUrl = audioUrl,
                IsVideo = false,
                IsAudio = true,
                NoNotification = false,
                HasEmbeddedUrl = !string.IsNullOrEmpty(message) && message.Contains("<--") && message.Contains("-->"),
            };

            var answer = await requestProviderService.Post<CreateChatMessageDto, bool>(this.ChatUrl + "/multiple", createChatMessage);
            return answer;
        }

        public async Task<Chat> GetPaged(int customerId, int brokerId, int pageNumber = 1, int pageSize = 25)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var localBrokerId = brokerId > 0 ? brokerId : user.MasterBrokerId;

            string newUrl = this.ChatUrl + $"/pagedchat";
            var ChatDto = await requestProviderService.Post<GetChatRequestDto, ChatDto>(newUrl, new GetChatRequestDto()
            {
                BrokerId = brokerId,
                CustomerId = customerId,
                GetDocumentFileContents = true,
                GetImageDocumentFileContents = true,
                PageNumber = pageNumber,
                PageSize = pageSize,
                MarkAsRead = true
            });
            var chat = mapper.Map<Chat>(ChatDto);

            if (this.api.IsYAHTheme)
            {
                if (chat != null && chat.Messages != null)
                {
                    chat.Messages = chat.Messages.Select(c => { c.YahTheme = true; return c; }).ToList();
                }

            }

            return chat;
        }

        public async Task<bool> SendDraft(string message, int customerId, DateTime toBeSentOn)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var brokerId = user.MasterBrokerId;

            var createChatMessage = new CreateChatDraftMessageDto
            {
                BrokerId = brokerId,
                CustomerId = customerId,
                Message = message,
                NoNotification = false,
                HasEmbeddedUrl = !string.IsNullOrEmpty(message) && message.Contains("<--") && message.Contains("-->"),
                ToBeSentOn = toBeSentOn
            };

            var answer = await requestProviderService.Post<CreateChatDraftMessageDto, ChatDraftMessageDto>($"{ChatUrl}/draft", createChatMessage);
            return answer != null && answer.Id > 0;
        }

        public async Task<bool> CreateDraftWithDocs(string message, int customerId, IEnumerable<ChatDocument> chatDocuments, DateTime toBeSentOn, bool NoNotification = false)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var brokerId = user.MasterBrokerId;
            var createChatMessage = new CreateChatDraftMessageDto
            {
                BrokerId = brokerId,
                CustomerId = customerId,
                Message = message,
                NoNotification = NoNotification,
                HasEmbeddedUrl = !string.IsNullOrEmpty(message) && message.Contains("<--") && message.Contains("-->"),
                ToBeSentOn = toBeSentOn
            };

            var draftDocuments = new List<CreateChatDocumentDto>();

            foreach (var document in chatDocuments)
            {
                var draftDocument = new CreateChatDocumentDto
                {
                    File = document.File,
                    FileName = document.FileName,
                    SupportingDocumentType = document.SupportingDocumentType
                };

                draftDocuments.Add(draftDocument);
            }
            createChatMessage.ChatDocuments = draftDocuments;

            var answer = await requestProviderService.Post<CreateChatDraftMessageDto, ChatDraftMessageDto>($"{ChatUrl}/draft", createChatMessage);

            return answer != null && answer.Id > 0;
        }

        public async Task<bool> UpdateDraftWithDocs(ChatDraftMessage draft)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;

            var updateChatMessage = new UpdateChatDraftMessageDto
            {
                Id = draft.Id,
                Message = draft.Message,
                HasEmbeddedUrl = !string.IsNullOrEmpty(draft.Message) && draft.Message.Contains("<--") && draft.Message.Contains("-->"),
                ToBeSentOn = draft.ToBeSentOn.Value
            };

            var draftDocuments = new List<ChatDocumentDto>();

            foreach (var document in draft.ChatDocuments)
            {
                var draftDocument = new ChatDocumentDto
                {
                    File = document.File,
                    FileName = document.FileName,
                    SupportingDocumentType = document.SupportingDocumentType
                };

                draftDocuments.Add(draftDocument);
            }
            updateChatMessage.ChatDocuments = draftDocuments;

            var answer = await requestProviderService.Put<UpdateChatDraftMessageDto, ChatDraftMessageDto>($"{ChatUrl}/draft/{draft.Id}", updateChatMessage);

            return answer != null && answer.Id > 0;
        }

        public async Task<bool> DeleteDraft(ChatDraftMessage message)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;

            return await requestProviderService.Delete($"{ChatUrl}/draft/{message.Id}");
        }
    }
}
