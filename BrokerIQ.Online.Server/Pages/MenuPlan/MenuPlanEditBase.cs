using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Pages
{
    using System.ComponentModel.DataAnnotations;
    using BrokerIQ.Online.Server.Shared;
    using Microsoft.AspNetCore.Components;
    using Models;
    using MudBlazor;
    using Services.Interface;


    public class MenuPlanEditBase : ComponentBase
    {
        private int id;
        private string menuPlanId;

        private int customerId;
        private string strCustomerId;

        [Inject]
        public IMenuPlanService MenuPlanService { get; set; }

        [Inject] 
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IAccountService AccountService { get; set; }

        [Inject]
        public IBrokerService BrokerService { get; set; }

        [Inject]
        public IDialogService DialogService { get; set; }

        [Inject]
        public INotificationService NotificationService { get; set; }

        [Inject]
        public ICustomerService CustomerService { get; set; }

        public MenuPlan MenuPlan { get; set; }

        protected string Message = string.Empty;
        protected string StatusClass = string.Empty;
        protected bool Saved;
        public bool IsAdmin { get; set; }
        public IEnumerable<Broker> Brokers { get; set; }
        public Broker Broker{ get; set; }

        [Required]
        public int BrokerListId = 1;


        [Parameter]
        public string MenuPlanId {
            get => this.menuPlanId;
            set
            {
                this.menuPlanId = value;
                this.id = int.Parse(value);
            }
        }

        [Parameter]
        public string CustomerId
        {
            get => this.strCustomerId;
            set
            {
                this.strCustomerId = value;
                this.customerId = int.Parse(value);
            }
        }

        public string SpinnerVisible { get; set; }
        public string LoadFileStatus { get; set; }


        public MenuPlanEditBase()
        {
            MenuPlan = new MenuPlan();
        }

        protected override async Task OnInitializedAsync()
        {
            SpinnerVisible = "display:none";
        }

        protected override async Task OnParametersSetAsync()
        {
            var user = await AccountService.GetUser();
            IsAdmin = user.IsAdmin;
            if (user.IsBroker || user.IsBrokerStaff)
            {
                var broker = user.MasterBrokerId;

                BrokerListId = broker;
                try
                {
                    Broker = await BrokerService.GetBroker(broker);
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

            try
            {
                if (this.id > 0)
                {
                    MenuPlan = (await MenuPlanService.GetMenuPlan(this.id));
                    BrokerListId = MenuPlan.BrokerId;
                }
            }
            catch
            {
                StatusClass = "alert-danger";
                Message = "Something went wrong getting menuPlan details";
                Saved = true;
            }
            
        }

        protected void HandleInvalidSubmit()
        {
            StatusClass = "alert-danger";
            Message = "There are some validation errors. Please try again.";
        }

        protected async Task HandleValidSubmit()
        {
            MenuPlan.BrokerId = this.BrokerListId;

            if (MenuPlan.Id == 0)
            {
                MenuPlan.CustomerId = this.customerId;
                if (MenuPlan.ReviewDate == null || MenuPlan.ReviewDate == DateTime.MinValue)
                {
                    MenuPlan.ReviewDate = DateTime.Now;
                }
                //Midnight
                MenuPlan.ReviewDate = new DateTime(MenuPlan.ReviewDate.Value.Year, MenuPlan.ReviewDate.Value.Month, MenuPlan.ReviewDate.Value.Day, 0, 0, 0);

                //Nocostfornow
                MenuPlan.Cost = 0.0m;

                var customer = new Customer();
                try
                {
                    customer = await CustomerService.GetCustomer(customerId);
                }
                catch
                {
                    StatusClass = "alert-danger";
                    Message = "Something went wrong getting the client. Please try again.";
                    Saved = false;
                    return;
                }


                var dialogParams = new DialogParameters();
                dialogParams.Add("Message", $"MenuPlan will be added for {customer.Name}");


                var result = await DialogService.Show<ConfirmCancelDialog>("MenuPlan Add", dialogParams).Result;
                if (!result.Cancelled)
                {
                    try
                    {
                        await MenuPlanService.AddMenuPlan(MenuPlan);
                    }
                    catch
                    {
                        StatusClass = "alert-danger";
                        Message = "Something went wrong adding the new MenuPlan. Please try again.";
                        Saved = true;
                        return;
                    }

                    StatusClass = "alert-success";
                    Message = "New MenuPlan added successfully.";
                    Saved = true;
                }

            }
            else
            {
                //Midnight
                MenuPlan.ReviewDate = new DateTime(MenuPlan.ReviewDate.Value.Year, MenuPlan.ReviewDate.Value.Month, MenuPlan.ReviewDate.Value.Day, 0, 0, 0);

                try
                {
                    await MenuPlanService.UpdateMenuPlan(MenuPlan);
                }
                catch
                {
                    StatusClass = "alert-danger";
                    Message = "Something went wrong updating the MenuPlan. Please try again.";
                    Saved = true;
                    return;
                }

                StatusClass = "alert-success";
                Message = "MenuPlan updated successfully.";
                Saved = true;
            }
        }

        protected async Task DeleteMenuPlan()
        {
            var dialogParams = new DialogParameters();
            dialogParams.Add("Message", "Are you sure you want to delete this menu plan? All insurances within the plan will also be deleted.");
            var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Cancelled)
            {
                try
                {
                    await MenuPlanService.DeleteMenuPlan(MenuPlan.Id);
                }
                catch
                {
                    StatusClass = "alert-danger";
                    Message = "Something went wrong deleting the MenuPlan. Please try again.";
                    Saved = true;
                    return;
                }             

                StatusClass = "alert-success";
                Message = "Deleted successfully";
                Saved = true;
            }

        }

        protected void NavigateToOverview()
        {
            NavigationManager.NavigateTo($"/clientdetail/{CustomerId}");
        }

        public DateTimeOffset? ReviewDate
        {
            get {
                var date = MenuPlan.ReviewDate.HasValue ? MenuPlan.ReviewDate.Value : DateTime.Today;
                return GetDTtoDTO(date); 
            }
            set => MenuPlan.ReviewDate = SetDTtoDTO(value);
        }

        public DateTimeOffset? GetDTtoDTO(DateTime datetimeIn)
        {
            if (MenuPlan != null && datetimeIn != default(DateTime))
            {
                var localTime1 = DateTime.SpecifyKind(datetimeIn, DateTimeKind.Local);
                DateTimeOffset localTime2 = localTime1;
                return localTime2;
            }
            else
            {
                return DateTimeOffset.Now;
            }
        }

        public DateTime SetDTtoDTO(DateTimeOffset? value)
        {
            return (value.HasValue ? value.Value : DateTime.MinValue).ToLocalTime().DateTime;
        }

        private string GetMessageMenuPlanAdded(string customerName, string brokerName, string menuPlanName)
        {
            var messageToSend = $"{customerName}, your broker {brokerName} has added a new {menuPlanName} MenuPlan to your BrokerIQ app.";
            return messageToSend;
        }

    }
}
