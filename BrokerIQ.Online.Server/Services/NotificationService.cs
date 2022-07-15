using System;
using System.Collections.Generic;
using System.Text;

namespace BrokerIQ.Online.Server.Services
{
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AutoMapper;
    using Newtonsoft.Json;
    using BrokerIQ.Dto.Models;
    using BrokerIQ.Online.Models;
    using BrokerIQ.Online.Services.Interface;
    using BrokerIQ.Online.Services.Abstract;

    public class NotificationService : INotificationService
    {
        private readonly string notificationUrl = "Notification";
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;
        private readonly IAccountService accountService;

        public NotificationService(IRequestProviderService requestProviderService, IMapper mapper, IAccountService accountService)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
            this.accountService = accountService;
        }

        public async Task<bool> SendMessageNotification(string message, List<int> targets,int brokerId, bool sendAll = false, bool chat = false)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;

            var notification = new CreateNotificationDto
            {
                Message = message,
                SendAll = sendAll,
                Targets = targets,
                VideoContentUrl = string.Empty,
                BrokerId = brokerId,
                BrokerStaffId = user.StaffBrokerId,
                IsChat = chat
            };
            var answer = await this.requestProviderService.Post<CreateNotificationDto, bool>(this.notificationUrl, notification);
            return answer;
        }

        public async Task<bool> SendVideoNotification(string message, string videoUrl, List<int> targets, bool sendAll = false)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;

            var notification = new CreateNotificationDto
            {
                Message = message,
                SendAll = sendAll,
                Targets = targets,
                VideoContentUrl = videoUrl,
                BrokerId = user.MasterBrokerId,
                BrokerStaffId = user.StaffBrokerId
            };
            var answer = await this.requestProviderService.Post<CreateNotificationDto, bool>(this.notificationUrl, notification);
            return answer;
        }

        public async Task<bool> SendAudioNotification(string message, string audioUrl, List<int> targets, bool sendAll = false)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;

            var notification = new CreateNotificationDto
            {
                Message = message,
                SendAll = sendAll,
                Targets = targets,
                AudioContentUrl = audioUrl,
                BrokerId = user.MasterBrokerId,
                BrokerStaffId = user.StaffBrokerId
            };
            var answer = await this.requestProviderService.Post<CreateNotificationDto, bool>(this.notificationUrl, notification);
            return answer;
        }

        public async Task<bool> SendBrokerNotification(string message, List<int> targets)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            bool sent = true;
            foreach(var target in targets)
            {
                var notification = new CreateNotificationDto
                {
                    Message = message,
                    BrokerId = target
                };
                sent = await this.requestProviderService.Post<CreateNotificationDto, bool>(this.notificationUrl+"/broker", notification);
                if (!sent)
                    break;
            }

            return sent;
        }

        public async Task<IEnumerable<Notification>> GetNotifications()
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var answer = await this.requestProviderService.Get<IEnumerable<NotificationDto>>(this.notificationUrl);
            return this.mapper.Map<IEnumerable<Notification>>(answer);
        }

        public async Task<IEnumerable<Notification>> GetNotificationByCustomerId(int customerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var brokerId = user.MasterBrokerId;

            var answer = await this.requestProviderService.Get<IEnumerable<NotificationDto>>(this.notificationUrl+$"/customer?customerId={customerId}&brokerId={brokerId}");
            return this.mapper.Map<IEnumerable<Notification>>(answer);
        }

        public async Task<IEnumerable<Notification>> GetNotificationByBrokerId(int brokerId)
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;

            var answer = await this.requestProviderService.Get<IEnumerable<NotificationDto>>(this.notificationUrl + $"/customer/bybroker?brokerId={brokerId}");
            return this.mapper.Map<IEnumerable<Notification>>(answer);
        }

        public async Task<IEnumerable<BrokerNotification>> GetBrokerNotifications()
        {
            var answer = await this.requestProviderService.Get<IEnumerable<BrokerNotificationDto>>(this.notificationUrl + $"/broker");
            return this.mapper.Map<IEnumerable<BrokerNotification>>(answer);
        }

        public async Task<IEnumerable<BrokerNotification>> GetBrokerNotificationsByBrokerId()
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var brokerId = user.MasterBrokerId;

            var answer = await this.requestProviderService.Get<IEnumerable<BrokerNotificationDto>>(this.notificationUrl + $"/brokerById?brokerId={brokerId}");
            return this.mapper.Map<IEnumerable<BrokerNotification>>(answer);
        }

        public async Task<int> GetNewBrokerNotificationsCountByBrokerId()
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var brokerId = user.MasterBrokerId;

            var answer = await this.requestProviderService.Get<int>(this.notificationUrl + $"/brokerById/newcount?brokerId={brokerId}");
            return answer;
        }

        public async Task MarkAsReadByBrokerId()
        {
            var user = await this.accountService.GetUser();
            this.requestProviderService.Token = user?.Token;
            var brokerId = user.MasterBrokerId;

            await this.requestProviderService.Post<int>(this.notificationUrl + $"/brokerById/markread?brokerid={brokerId}",brokerId);
        }
    }
}
