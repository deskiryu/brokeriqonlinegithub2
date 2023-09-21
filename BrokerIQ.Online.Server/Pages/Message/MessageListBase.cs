using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using MudBlazor;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Services.Interface;
using BrokerIQ.Online.Server.Helper;

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

        public List<BrokerNotification> BrokerNotifications { get; set; }

        public bool IsAdmin { get; set; }

        public IEnumerable<Broker> Brokers { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var user = await AccountService.GetUser();
                IsAdmin = user.IsAdmin;
                if (user.IsBroker || user.IsBrokerStaff)
                {
                    BrokerNotifications = (await NotificationService.GetBrokerNotificationsByBrokerId()).OrderBy(n => n.Read).ThenByDescending(x => x.SentDate).ToList();
                    CurrentMessageCount.MessageCount = BrokerNotifications.Count;
                }
                else
                {
                    BrokerNotifications = (await NotificationService.GetBrokerNotifications()).OrderByDescending(x => x.SentDate).ToList();
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
            catch
            {
                NavigationManager.NavigateTo($"account/logout");
            }
        }

        protected void NavigateToOverview()
        {
            Saved = false;
        }

        protected async Task SetReminderReadStatus(int reminderId)
        {
            await NotificationService.ToggleNotificationReadStatus(reminderId);

            CurrentMessageCount.MessageCount--;
        }
    }
}
