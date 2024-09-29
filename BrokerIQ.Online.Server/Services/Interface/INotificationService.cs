using System.Collections.Generic;
using BrokerIQ.Online.Models;
using System.Threading.Tasks;
using BrokerIQ.Online.Server.Services.Base;

namespace BrokerIQ.Online.Services.Interface
{
    public interface INotificationService
    {
        Task<bool> SendMessageNotification(string message, List<int> targets, int brokerId, bool sendAll = false, bool chat = false, bool updateAppAlert = true);

        Task<bool> SendVideoNotification(string message, string videoUrl, List<int> targets, bool sendAll = false);

        Task<bool> SendAudioNotification(string message, string audioUrl, List<int> targets, bool sendAll = false);

        Task<bool> SendBrokerNotification(string message, List<int> targets);

        Task<bool> SendMortgageVideoNotification(int customerId, string message);

        Task<IEnumerable<Notification>> GetNotifications();

        Task<IEnumerable<Notification>> GetNotificationByCustomerId(int customerId);

        Task<IEnumerable<Notification>> GetNotificationByBrokerId(int brokerId);

        Task<IEnumerable<BrokerNotification>> GetBrokerNotifications();

        Task<IEnumerable<BrokerNotification>> GetBrokerNotificationsByBrokerId();

        Task<ApiResponse<int>> GetNewBrokerNotificationsCountByBrokerId();

        Task MarkAsReadByBrokerId();

        Task ToggleNotificationReadStatus(int notificationId);

        Task MarkAllNotificationsAsRead();
    }
}
