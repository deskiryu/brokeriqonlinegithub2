namespace ReviewIt.Web.Pages
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    
    using Microsoft.AspNetCore.Components;
    using Models;
    using BrokerIQ.Dto.Models;
    using ReviewIt.Web.Models.Account;
    using ReviewIt.Web.Server.Models;
    using Services.Interface;

    public class BrokerStaffAddBase : ComponentBase
    {
        private int id;
        private int customerId;
        private string strBrokerStaffId;

        [Inject]
        public IBrokerStaffService BrokerStaffService { get; set; }

        [Inject]
        public IAlertService AlertService { get; set; }

        [Inject]
        public IAccountService AccountService { get; set; }

        [Inject] 
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IMapper mapper { get; set; }

        public AddStaff BrokerStaff { get; set; }

        protected string Message = string.Empty;
        protected string StatusClass = string.Empty;
        protected bool Saved;
        private bool loading;

        [Parameter]
        public string BrokerStaffId { get; set; }

        public BrokerStaffAddBase()
        {
            BrokerStaff = new AddStaff();
        }

        protected override async Task OnInitializedAsync()
        {

        }

        protected void HandleInvalidSubmit()
        {
            StatusClass = "alert-danger";
            Message = "There are some validation errors. Please try again.";
        }

        protected async Task HandleValidSubmit()
        {
            // reset alerts on submit
            AlertService.Clear();

            loading = true;
            
            try
            {
                var user = await AccountService.GetUser();
                var brokerReturned = new BrokerStaffDto();
                var brokerId = 0;
                if (user.IsBroker)
                {
                    if (user.IsBroker)
                    {
                        brokerId = Int32.Parse(user.Id);
                    }
                }
                if (brokerId == 0)
                {
                    throw new Exception("No broker found");
                }

                var createBroker = mapper.Map<CreateBrokerStaffDto>(BrokerStaff);
                createBroker.BrokerId = brokerId;
                brokerReturned = await AccountService.RegisterStaff(createBroker);
                if (brokerReturned == null || string.IsNullOrEmpty(brokerReturned.EmailAddress))
                {
                    throw new Exception("Registration failed");
                }

                AlertService.Success("Registration successful", keepAfterRouteChange: true);
                NavigateToOverview();

            }
            catch (Exception ex)
            {
                AlertService.Error(ex.Message);
            }
            loading = false;
            StateHasChanged();
        }

        protected void NavigateToOverview()
        {
            NavigationManager.NavigateTo($"/brokerstafflist");
        }

    }
}
