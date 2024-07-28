using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;

namespace BrokerIQ.Online.Server.Pages.Message
{
    public partial class BrokerNotificationTable : ComponentBase
    {
        [Inject]
        public INotificationService NotificationService { get; set; }

        [Parameter]
        public User User { get; set; }

        [Parameter]
        public IEnumerable<Online.Models.Broker> Brokers { get; set; }

        [Parameter]
        public IEnumerable<BrokerNotification> Notifications { get; set; }

        [Parameter]
        public Action OnStatusToggle { get; set; }

        protected async Task SetReminderReadStatus(int reminderId)
        {
            await NotificationService.ToggleNotificationReadStatus(reminderId);

            OnStatusToggle();
        }
    }
}

