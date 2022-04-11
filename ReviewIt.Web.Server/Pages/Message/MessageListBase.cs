using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace ReviewIt.Web.Pages
{
    
    using Microsoft.AspNetCore.Components;
    using MudBlazor;
    using ReviewIt.Web.Models;
    using ReviewIt.Web.Server.Models;
    using ReviewIt.Web.Services.Interface;

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
                    BrokerNotifications = (await NotificationService.GetBrokerNotificationsByBrokerId()).OrderByDescending(x => x.SentDate).ToList();
                    await NotificationService.MarkAsReadByBrokerId();
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
                        Saved = false;
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
    }
}
