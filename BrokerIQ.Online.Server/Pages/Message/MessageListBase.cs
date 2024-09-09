using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Helper;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Pages
{
    public class MessageListBase : ComponentBase
    {
        protected string Message = string.Empty;
        protected string StatusClass = string.Empty;
        protected bool Saved;

        public string VideoName { get; set; }
        public string ExtensionName { get; set; }
        public bool RenameUploadVisibility { get; set; }

        public string status;

        protected bool AllMessagesRead => !BrokerNotificationsSubset.Where(n => !n.Read).Any();

        [Inject]
        public INotificationService NotificationService { get; set; }

        [Inject]
        public IAccountService AccountService { get; set; }

        [Inject]
        public IBrokerService BrokerService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IDialogService DialogService { get; set; }

        [Inject]
        protected MessageCountState CurrentMessageCount { get; set; }

        protected User User { get; set; }

        public List<BrokerNotification> BrokerNotifications { get; set; }

        public List<BrokerNotification> BrokerNotificationsSubset { get; set; }

        public IEnumerable<Broker> Brokers { get; set; }

        public int BrokerId { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                User = await AccountService.GetUser();
                await RefreshMessages();
            }
            catch
            {
                NavigationManager.NavigateTo($"account/logout");
            }
        }

        protected async Task AutoCompleteClickBroker()
        {
            if (BrokerId == 0)
            {
                BrokerNotificationsSubset = BrokerNotifications;
            }
            else if (BrokerId > 0)
            {
                BrokerNotificationsSubset = BrokerNotifications.Where(x => x.BrokerId == BrokerId).ToList();
            }
        }

        public async Task RefreshMessages()
        {
            if (User.IsBroker || User.IsAdminStaff || User.IsBrokerStaff)
            {
                BrokerNotificationsSubset = BrokerNotifications = (await NotificationService.GetBrokerNotificationsByBrokerId()).ToList();
                CurrentMessageCount.MessageCount = BrokerNotifications.Count;
            }
            else
            {
                BrokerNotificationsSubset = BrokerNotifications = (await NotificationService.GetBrokerNotifications()).ToList();
                try
                {
                    Brokers = await BrokerService.GetBrokers();
                }
                catch
                {
                    StatusClass = "alert-danger";
                    Message = "Something went wrong getting customer details";
                    Saved = true;
                }
            }
            StateHasChanged();
        }

        protected void NavigateToOverview()
        {
            Saved = false;
        }

        protected async Task ClearAllNotifications()
        {
            await NotificationService.MarkAllNotificationsAsRead();

            await RefreshMessages();
        }
    }
}
