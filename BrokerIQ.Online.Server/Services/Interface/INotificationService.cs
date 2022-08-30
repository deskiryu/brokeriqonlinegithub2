using System;
using System.Collections.Generic;
using System.Text;

namespace BrokerIQ.Online.Services.Interface
{
    using BrokerIQ.Online.Models;
    using System.Threading.Tasks;

    public interface INotificationService
    {
        Task<bool> SendMessageNotification(string message, List<int> targets, int brokerId, bool sendAll = false, bool chat = false, bool updateAppAlert = true);

        Task<bool> SendVideoNotification(string message, string videoUrl, List<int> targets, bool sendAll = false);

        Task<bool> SendAudioNotification(string message, string audioUrl, List<int> targets, bool sendAll = false);

        Task<bool> SendBrokerNotification(string message, List<int> targets);

        Task<IEnumerable<Notification>> GetNotifications();

        Task<IEnumerable<Notification>> GetNotificationByCustomerId(int customerId);

        Task<IEnumerable<Notification>> GetNotificationByBrokerId(int brokerId);

        Task<IEnumerable<BrokerNotification>> GetBrokerNotifications();

        Task<IEnumerable<BrokerNotification>> GetBrokerNotificationsByBrokerId();

        Task<int> GetNewBrokerNotificationsCountByBrokerId();

        Task MarkAsReadByBrokerId();

    }
}
