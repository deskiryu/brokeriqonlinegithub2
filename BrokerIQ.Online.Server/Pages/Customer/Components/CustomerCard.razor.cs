using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Dto.Dto;
using BrokerIQ.Dto.Enum;
using BrokerIQ.Dto.Import;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Services.Interface;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Customer.Components
{
    public partial class CustomerCard : ComponentBase
    {
        [Inject]
        public ICustomerService CustomerService { get; set; }

        [Inject]
        public IAccountService AccountService { get; set; }

        [Inject]
        public ICustomerDocumentService CustomerDocumentService { get; set; }

        [Inject]
        public IOccupationService OccupationService { get; set; }

        [Inject]
        public IDialogService DialogService { get; set; }

        [Inject]
        public IEmailService EmailService { get; set; }

        [Inject]
        public IWealthTypeService WealthService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public ISnackbar Snackbar { get; set; }

        [Parameter]
        public User User { get; set; }

        [Parameter]
        public Online.Models.Broker Broker { get; set; }

        [Parameter]
        public IEnumerable<Online.Models.Broker> Brokers { get; set; }

        [Parameter]
        public Online.Models.Customer Customer { get; set; }

        [Parameter]
        public Action OnCustomerConnectionChange { get; set; }

        [Parameter]
        public bool IsAlreadyConnected { get; set; }

        protected OccupationDto Occupation { get; set; }

        protected CsvImportCustomerDto ImportDetails { get; set; }

        protected WealthTypeDto WealthType { get; set; }

        protected bool HasConnection { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Occupation = await OccupationService.GetById(Customer.OccupationId);

            ImportDetails = await CustomerService.GetImportDetails(Customer.Id);

            WealthType = (await WealthService.GetAllForBroker(Broker.Id)).FirstOrDefault(w => w.Id == Customer.WealthTypeId);
        }

        protected string GetNeedsContent()
        {
            if (!Customer.HasNeeds) return string.Empty;

            var currentDate = DateTime.UtcNow;

            var hasIncomeProtection = Customer.Insurances.Any(i => i.InsType == InsuranceEnum.Income && i.ExpiryDate > currentDate);
            var hasLifeAndIllness = Customer.Insurances.Any(i => i.InsType == InsuranceEnum.Illness && i.ExpiryDate > currentDate);

            if (Customer.Employment == EmploymentEnum.SelfEmployed)
            {
                if (!hasIncomeProtection && !hasLifeAndIllness) return "Customer is self employed, but has neither Income Protection nor Life and Illness cover.";
                if (!hasIncomeProtection) return "Customer is self employed, but does not have Income Protection cover.";
                if (!hasLifeAndIllness) return "Customer is self employed, but does not have Life and Illness cover.";
            }

            if (Customer.Employment == EmploymentEnum.Employed)
            {
                return "Customer is employed, but does not have Life and Illness cover.";
            }

            return string.Empty;
        }

        protected async Task DeleteCustomer()
        {
            var dialogParams = new DialogParameters
            {
                { "Message", $"Are you absolutely sure you want to delete this client {Customer.Name}? This is a PERMANENT DELETE and cannot be undone." }
            };

            var result = await DialogService.Show<Shared.ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Canceled)
            {
                var deleted = await CustomerService.DeleteCustomer(Customer.Id);
                if (deleted)
                {
                    var responseParams = new DialogParameters();
                    responseParams.Add("Message", "Deleted successfully");
                    await DialogService.Show<Shared.AlertDialog>("Information", responseParams).Result;
                }
                else
                {
                    var responseParams = new DialogParameters();
                    responseParams.Add("Message", "The client did not delete.");
                    await DialogService.Show<Shared.AlertDialog>("Information", responseParams).Result;
                }
                NavigationManager.NavigateTo($"/clientlist");
            }
        }

        protected async Task ResendEmailCustomer()
        {
            var dialogParams = new DialogParameters();
            dialogParams.Add("Message", $"A verify email will be sent to {Customer.Name}. Continue? ");
            var result = await DialogService.Show<Shared.ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Canceled)
            {
                var resent = await AccountService.ResendEmail(Customer.EmailAddress);
                if (resent)
                {
                    var responseParams = new DialogParameters();
                    responseParams.Add("Message", "Resent successfully");
                    await DialogService.Show<Shared.AlertDialog>("Information", responseParams).Result;
                }
                else
                {
                    var responseParams = new DialogParameters();
                    responseParams.Add("Message", "The resend email failed.");
                    await DialogService.Show<Shared.AlertDialog>("Information", responseParams).Result;
                }
                NavigationManager.NavigateTo($"/clientlist");
            }
        }

        protected async Task Disconnect()
        {
            var dialogParams = new DialogParameters
            {
                { "Message", $"Are you sure you want to disconnect this client?" }
            };

            var result = await DialogService.Show<Shared.ConfirmCancelDialog>("Confirmation", dialogParams).Result;

            if (result.Canceled) return;

            await CustomerService.Disconnect(Customer.TargetCustomerId);

            OnCustomerConnectionChange();
        }

        protected async Task SendAppInvite()
        {
            var emailSent = await CustomerService.SendAppInvite(Customer.Id);

            if (emailSent)
            {
                ImportDetails = await CustomerService.GetImportDetails(Customer.Id);

                Snackbar.Add("Mobile App invitation has been sent", Severity.Success);

                StateHasChanged();

                return;
            }

            Snackbar.Add("Unable to send mobile app invite", Severity.Error);
        }

        protected async Task ConnectToCustomer()
        {
            var dialogParams = new DialogParameters
            {
                { "Customer", Customer},
            };

            var dialogOptions = new DialogOptions()
            {
                MaxWidth = MaxWidth.Small,
                FullWidth = true
            };

            var result = await DialogService.Show<CustomerConnectionDialog>("Connect to customer", dialogParams, dialogOptions).Result;

            if (result.Canceled) return;

            await CustomerService.Connect(Customer.ChosenBrokerId, Customer.Id, (int)result.Data);

            OnCustomerConnectionChange();
        }
    }
}