using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Services.Base;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Server.Services
{
    public class NotificationService : BIQService, INotificationService
    {
        private readonly string notificationUrl = "Notification";
        private readonly IMapper mapper;

        public NotificationService(IRequestProviderService requestProviderService, IMapper mapper, IAccountService accountService)
            :base(accountService, requestProviderService)
        {
            this.mapper = mapper;
        }

        public async Task<bool> SendMessageNotification(string message, List<int> targets, int brokerId, bool sendAll = false, bool chat = false, bool updateAppAlert = true)
        {
            var user = await this._accountService.GetUser();

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

            var url = this.notificationUrl;
            url += $"updateAppAlert={updateAppAlert}";
            var answer = await this._requestProviderService.Post<CreateNotificationDto, bool>(this.notificationUrl, notification);
            return answer;
        }

        public async Task<bool> SendVideoNotification(string message, string videoUrl, List<int> targets, bool sendAll = false)
        {
            var user = await this._accountService.GetUser();

            var notification = new CreateNotificationDto
            {
                Message = message,
                SendAll = sendAll,
                Targets = targets,
                VideoContentUrl = videoUrl,
                BrokerId = user.MasterBrokerId,
                BrokerStaffId = user.StaffBrokerId
            };
            var answer = await this._requestProviderService.Post<CreateNotificationDto, bool>(this.notificationUrl, notification);
            return answer;
        }

        public async Task<bool> SendAudioNotification(string message, string audioUrl, List<int> targets, bool sendAll = false)
        {
            var user = await this._accountService.GetUser();

            var notification = new CreateNotificationDto
            {
                Message = message,
                SendAll = sendAll,
                Targets = targets,
                AudioContentUrl = audioUrl,
                BrokerId = user.MasterBrokerId,
                BrokerStaffId = user.StaffBrokerId
            };
            var answer = await this._requestProviderService.Post<CreateNotificationDto, bool>(this.notificationUrl, notification);
            return answer;
        }

        public async Task<bool> SendMortgageVideoNotification(int customerId, string message)
        {
            var user = await this._accountService.GetUser();

            var notification = new CreateNotificationDto
            {
                Message = message,
                SendAll = false,
                Targets = new List<int> { customerId },
                AudioContentUrl = string.Empty,
                VideoContentUrl = string.Empty,
                BrokerId = user.MasterBrokerId,
                BrokerStaffId = user.StaffBrokerId,
                IsChat = false
            };
            var answer = await this._requestProviderService.Post<CreateNotificationDto, bool>(this.notificationUrl + "/functionSendMortgageVideo", notification);
            return answer;
        }

        public async Task<bool> SendBrokerNotification(string message, List<int> targets)
        {
            bool sent = true;
            foreach (var target in targets)
            {
                var notification = new CreateBrokerNotificationDto
                {
                    Message = message,
                    BrokerId = target,
                    SendBrokerNotificationToPhone = true,
                    SendBrokerNotificationToStaffPhone = true,

                };
                sent = await this._requestProviderService.Post<CreateBrokerNotificationDto, bool>(this.notificationUrl + "/broker", notification);
                if (!sent)
                    break;
            }

            return sent;
        }

        public async Task<IEnumerable<Notification>> GetNotifications()
        {
            var answer = await this._requestProviderService.Get<IEnumerable<NotificationDto>>(this.notificationUrl);
            return this.mapper.Map<IEnumerable<Notification>>(answer);
        }

        public async Task<IEnumerable<Notification>> GetNotificationByCustomerId(int customerId)
        {
            var brokerId = await GetCurrentBrokerId();

            var answer = await this._requestProviderService.Get<IEnumerable<NotificationDto>>(this.notificationUrl + $"/customer?customerId={customerId}&brokerId={brokerId}");
            return this.mapper.Map<IEnumerable<Notification>>(answer);
        }

        public async Task<IEnumerable<Notification>> GetNotificationByBrokerId(int brokerId)
        {
            var answer = await this._requestProviderService.Get<IEnumerable<NotificationDto>>(this.notificationUrl + $"/customer/bybroker?brokerId={brokerId}");
            return this.mapper.Map<IEnumerable<Notification>>(answer);
        }

        public async Task<IEnumerable<BrokerNotification>> GetBrokerNotifications()
        {
            var answer = await this._requestProviderService.Get<IEnumerable<BrokerNotificationDto>>(this.notificationUrl + $"/broker");
            return this.mapper.Map<IEnumerable<BrokerNotification>>(answer);
        }

        public async Task<IEnumerable<BrokerNotification>> GetBrokerNotificationsByBrokerId()
        {
            var brokerId = await GetCurrentBrokerId();

            var answer = await this._requestProviderService.Get<IEnumerable<BrokerNotificationDto>>(this.notificationUrl + $"/brokerById?brokerId={brokerId}");
            return this.mapper.Map<IEnumerable<BrokerNotification>>(answer);
        }

        public async Task<ApiResponse<int>> GetNewBrokerNotificationsCountByBrokerId()
        {
            var brokerId = await GetCurrentBrokerId();

            var answer = await this._requestProviderService.GetResponse<int>(this.notificationUrl + $"/brokerById/newcount?brokerId={brokerId}");
            return answer;
        }

        public async Task MarkAsReadByBrokerId()
        {
            var brokerId = await GetCurrentBrokerId();

            await this._requestProviderService.Post<int>(this.notificationUrl + $"/brokerById/markread?brokerid={brokerId}", brokerId);
        }

        public async Task ToggleNotificationReadStatus(int notificationId)
        {
            var url = $"{this.notificationUrl}/{notificationId}/{await GetCurrentBrokerId()}/toggleread";
            await this._requestProviderService.Post<int>(url);
        }

        public async Task MarkAllNotificationsAsRead()
        {
            var url = $"{this.notificationUrl}/{await GetCurrentBrokerId()}/markallread";
            await this._requestProviderService.Post<int>(url);
        }
    }
}
