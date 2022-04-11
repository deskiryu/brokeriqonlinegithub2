using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReviewIt.Web.Pages
{
    using Dto.Models;
    using Microsoft.AspNetCore.Components;
    using Models;
    using MudBlazor;
    using Services.Interface;

    public class BrokerStaffListBase : ComponentBase
    {
        [Inject]
        public IBrokerStaffService BrokerStaffService { get; set; }

        [Inject]
        public IBrokerService BrokerService { get; set; }

        [Inject]
        public IDialogService DialogService { get; set; }

        [Inject]
        public IAccountService AccountService { get; set; }


        public NavigationManager NavigationManager { get; set; }

        public List<BrokerStaff> BrokerStaff { get; set; }
        public List<Broker> Brokers { get; set; }

        protected string Message = string.Empty;
        protected string StatusClass = string.Empty;
        protected bool Saved;

        protected int BrokerId;

        public bool IsAdmin { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                BrokerId = 0;
                IsAdmin = false;

                var user = await AccountService.GetUser();
                if (user.IsAdmin)
                {
                    IsAdmin = true;
                    BrokerStaff = (await BrokerStaffService.GetBrokerStaff()).ToList();
                    Brokers = (await BrokerService.GetBrokers()).ToList();
                }
                else if (user.IsBroker)
                {
                    BrokerStaff = (await BrokerStaffService.GetBrokerStaffbyBrokerId(user.MasterBrokerId)).ToList();
                }       
            }
            catch
            {
                NavigationManager.NavigateTo($"account/logout");
            }


        }

        protected void NavigateToOverview()
        {
            NavigationManager.NavigateTo($"/brokerstafflist");
        }

    }
}
