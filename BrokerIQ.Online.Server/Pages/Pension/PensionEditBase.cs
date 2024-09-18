using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Shared;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Pages
{
    public class PensionEditBase : ComponentBase
    {
        [Inject]
        public IPensionService PensionService { get; set; }

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

        public Pension Pension { get; set; }

        protected string Message = string.Empty;
        protected string StatusClass = string.Empty;
        protected bool Saved;
        public bool IsAdmin { get; set; }
        public IEnumerable<Broker> Brokers { get; set; }
        public Broker Broker { get; set; }

        public string DragEnterStyle { get; set; }

        [Required]
        public int BrokerListId = 1;

        [Parameter]
        public string PensionId { get; set; }

        [Parameter]
        public string CustomerId { get; set; }

        protected int pensionId;

        protected int customerId;

        public string SpinnerVisible { get; set; }

        public bool SendNotification { get; set; } = true;

        protected string FormId = "PensionForm";

        protected string HoverClass;

        public PensionEditBase()
        {
            Pension = new Pension();
        }

        protected override async Task OnParametersSetAsync()
        {
            customerId = Int32.Parse(CustomerId);

            pensionId = Int32.Parse(PensionId);

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

                    var customer = await CustomerService.GetCustomer(customerId);
                    BrokerListId = customer.ChosenBrokerId;
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
                if (pensionId > 0)
                {
                    Pension = (await PensionService.Get(pensionId));
                }
            }
            catch
            {
                StatusClass = "alert-danger";
                Message = "Something went wrong getting Pension details";
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
            if (Pension.Id == 0)
            {
                Pension.CustomerId = customerId;

                Customer customer;
                try
                {
                    customer = await CustomerService.GetCustomer(customerId);
                }
                catch
                {
                    StatusClass = "alert-danger";
                    Message = "Something went wrong getting the client. Please try again.";
                    Saved = true;
                    return;
                }

                var dialogParams = new DialogParameters();
                if (customer.EmailConfirmed && SendNotification == true)
                {
                    dialogParams.Add("Message", $"Pension will be added and a notification will be sent to {customer.Name} about this new Pension.");
                }
                else if (!customer.EmailConfirmed)
                {
                    dialogParams.Add("Message", $"Pension will be added however a notification will be NOT be sent to {customer.Name} about this new Pension as their email address is not confirmed");
                }
                else
                {
                    dialogParams.Add("Message", $"Pension will be added however a notification will be NOT be sent to {customer.Name}.");
                }

                try
                {
                    await PensionService.Add(Pension, IsAdmin ? this.BrokerListId : customer.ChosenBrokerId);
                }
                catch
                {
                    StatusClass = "alert-danger";
                    Message = "Something went wrong adding the new Pension. Please try again.";
                    Saved = true;
                    return;
                }

                try
                {
                    if (SendNotification)
                    {
                        await SendMessageNotification(customer);
                    }
                }
                catch
                {

                }

                StatusClass = "alert-success";
                Message = "New Pension added successfully.";
                Saved = true;

            }
            else
            {
                try
                {
                    await PensionService.Update(Pension);
                }
                catch
                {
                    StatusClass = "alert-danger";
                    Message = "Something went wrong updating the Pension. Please try again.";
                    Saved = true;
                    return;
                }

                StatusClass = "alert-success";
                Message = "Pension updated successfully.";
                Saved = true;
            }
        }

        protected async Task DeletePension()
        {
            var dialogParams = new DialogParameters();
            dialogParams.Add("Message", "Are you sure you want to delete this pension?");
            var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Canceled)
            {
                try
                {
                    await PensionService.Delete(Pension.Id);
                }
                catch
                {
                    StatusClass = "alert-danger";
                    Message = "Something went wrong deleting the Pension. Please try again.";
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

        private string GetMessagePensionAdded(string customerName, string brokerName, string PensionName)
        {
            var messageToSend = $"{customerName}, your broker {brokerName} has added a new {PensionName} Pension to your app.";
            return messageToSend;
        }

        private async Task SendMessageNotification(Customer customer)
        {
            var brokerId = 0;
            var brokerName = "";
            var PensionName = "";

            if (IsAdmin)
            {
                brokerName += Brokers.FirstOrDefault(x => x.Id == BrokerListId)?.Name ?? "";
                brokerId = Brokers.FirstOrDefault(x => x.Id == BrokerListId)?.Id ?? 0;
            }
            else
            {
                brokerName += Broker?.Name ?? "";
                brokerId = Broker?.Id ?? 0;
            }

            var messageToSend = GetMessagePensionAdded(customer.FirstName, brokerName, PensionName);

            try
            {
                await NotificationService.SendMessageNotification(messageToSend, new List<int> { customerId }, brokerId, updateAppAlert: false);
            }
            catch
            {

            }
        }
    }
}
