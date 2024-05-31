using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Pages
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

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        public List<BrokerStaff> BrokerStaffBase { get; set; }
        public List<BrokerStaff> BrokerStaff { get; set; }
        public List<Broker> Brokers { get; set; }

        protected string Message = string.Empty;
        protected string StatusClass = string.Empty;
        protected bool Saved;

        protected int BrokerId;

        public bool IsAdmin { get; set; }

        public bool IsMinorAdmin { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                BrokerId = 0;
                IsAdmin = false;

                var user = await AccountService.GetUser();
                if (user.IsAdmin || user.IsMinorAdmin)
                {
                    IsAdmin = user.IsAdmin;
                    IsMinorAdmin= user.IsMinorAdmin;
                    BrokerStaffBase = BrokerStaff = (await BrokerStaffService.GetBrokerStaff()).ToList();
                    Brokers = (await BrokerService.GetBrokers()).ToList();
                }
                else if (user.IsBroker || user.IsAdminStaff)
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

        // Initialize SearchTerm to "" to prevent null's
        protected string SearchTerm { get; set; } = "";

        //filter
        protected List<BrokerIQ.Online.Models.BrokerStaff>
        FilteredBrokerStaff => BrokerStaff.Where(i => !string.IsNullOrEmpty(i.FullName) && i.FullName.ToLower().Contains(SearchTerm.ToLower())).ToList();

        protected async Task AutoCompleteClickBroker()
        {
            if (BrokerId == 0)
            {
                BrokerStaff = BrokerStaffBase;
            }
            else if (BrokerId > 0)
            {
                BrokerStaff = BrokerStaffBase.Where(x => x.BrokerId == BrokerId).ToList();
            }
        }

        protected async Task<IEnumerable<string>> OnFilter(string value)
        {
            if (!string.IsNullOrEmpty(value) && BrokerStaff != null && BrokerStaff.Any())
            {
                // In real life use an asynchronous function for fetching data from an api.
                var filtered = BrokerStaff.Where(
                i => !string.IsNullOrEmpty(i.FirstName) && i.FirstName.ToLower().Contains(value.ToLower()) ||
                     !string.IsNullOrEmpty(i.LastName) && i.LastName.ToLower().Contains(value.ToLower()) ||
                     !string.IsNullOrEmpty(i.EmailAddress) && i.EmailAddress.ToLower().Contains(value.ToLower()) ||
                     !string.IsNullOrEmpty(i.TwoFactorPhoneNumber) && i.TwoFactorPhoneNumber.ToLower().Contains(value.ToLower()));

                var results = await Task.FromResult(filtered.Select(x => x.FullName).Distinct().ToList());
                return results;
            }
            else
            {
                return new List<string>();
            }

        }

        public void AutoCompleteClick(string args)
        {
            var brokerStaff = BrokerStaff.FirstOrDefault(x => x.FullName == args);
            if (brokerStaff != null)
            {
                NavigationManager.NavigateTo($"brokerstaffedit/{brokerStaff.Id}");
            }

        }

    }
}
