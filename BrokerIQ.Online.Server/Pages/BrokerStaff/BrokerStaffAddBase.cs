namespace BrokerIQ.Online.Pages
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;

    using Microsoft.AspNetCore.Components;
    using Models;
    using BrokerIQ.Dto.Models;
    using BrokerIQ.Online.Models.Account;
    using BrokerIQ.Online.Server.Models;
    using Services.Interface;
    using BrokerIQ.Online.Server.Helper;
    using Microsoft.JSInterop;
    using System.Collections.Generic;
    using BrokerIQ.Online.Services;
    using System.ComponentModel.DataAnnotations;
    using BrokerIQ.Dto.Enum;
    using Microsoft.AspNetCore.WebUtilities;

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
        public IBrokerService BrokerService { get; set; }


        [Inject]
        public IAccountService AccountService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IMapper mapper { get; set; }

        [Inject]
        protected IJSRuntime js { get; set; }

        public AddStaff BrokerStaff { get; set; }

        protected string Message = string.Empty;
        protected string StatusClass = string.Empty;
        protected bool Saved;
        private bool loading;

        [Parameter]
        public string BrokerStaffId { get; set; }

        public IEnumerable<Broker> Brokers { get; set; }
        public Broker Broker { get; set; }

        public bool IsAdmin { get; set; }


        [Required]
        public int BrokerListId = 0;

        public BrokerStaffAddBase()
        {
            BrokerStaff = new AddStaff();
        }

        protected override async Task OnParametersSetAsync()
        {
            var user = await AccountService.GetUser();
            IsAdmin = user.IsAdmin;
            if (user.IsBroker || user.IsAdminStaff || user.IsBrokerStaff)
            {
                var brokerId = user.MasterBrokerId;

                BrokerListId = brokerId;
                try
                {
                    Broker = await BrokerService.GetBroker(brokerId);
                }
                catch
                {
                    StatusClass = "alert-danger";
                    Message = "Something went wrong getting broker details";
                    Saved = true;
                }
            }
            else
            {
                try
                {
                    Brokers = await BrokerService.GetBrokers();
                }
                catch
                {
                    StatusClass = "alert-danger";
                    Message = "Something went wrong getting broker details";
                    Saved = true;
                }
            }
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
                else
                {
                    brokerId = BrokerListId;
                }

                if (brokerId == 0)
                {
                    throw new Exception("No broker found");
                }

                var createBrokerStaff = mapper.Map<CreateBrokerStaffDto>(BrokerStaff);
                createBrokerStaff.BrokerId = brokerId;
                var isRunningWasm = await RunningWasm.IsWebAssembly(js);
                if (isRunningWasm)
                {
                    createBrokerStaff.TurnOnTwoFactor = true;
                }
                else
                {
                    createBrokerStaff.TurnOnTwoFactor = false;
                }
                brokerReturned = await AccountService.RegisterStaff(createBrokerStaff);
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
