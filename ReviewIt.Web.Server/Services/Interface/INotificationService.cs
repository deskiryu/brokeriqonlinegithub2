using System;
using System.Collections.Generic;
using System.Text;

namespace ReviewIt.Web.Services.Interface
{
    using System.Threading.Tasks;

    public interface INotificationService
    {
        Task<bool> SendMessageNotification(string message, List<int> targets, int brokerId, bool sendAll = false, bool chat = false);

        Task<bool> SendVideoNotification(string message, string videoUrl, List<int> targets, bool sendAll = false);

        Task<bool> SendAudioNotification(string message, string audioUrl, List<int> targets, bool sendAll = false);

        Task<bool> SendBrokerNotification(string message, List<int> targets);

        Task<IEnumerable<ReviewIt.Web.Models.Notification>> GetNotifications();

        Task<IEnumerable<ReviewIt.Web.Models.Notification>> GetNotificationByCustomerId(int customerId);

        Task<IEnumerable<ReviewIt.Web.Models.Notification>> GetNotificationByBrokerId(int brokerId);

        Task<IEnumerable<ReviewIt.Web.Models.BrokerNotification>> GetBrokerNotifications();

        Task<IEnumerable<ReviewIt.Web.Models.BrokerNotification>> GetBrokerNotificationsByBrokerId();

        Task<int> GetNewBrokerNotificationsCountByBrokerId();

        Task MarkAsReadByBrokerId();

    }
}
