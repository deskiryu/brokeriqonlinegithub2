using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using BrokerIQ.Dto.Dto;
using BrokerIQ.Dto.Enum;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Services.Interface;
using BrokerIQ.Online.Services.Interface;

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
        public NavigationManager NavigationManager { get; set; }

        [Parameter]
        public User User { get; set; }

        [Parameter]
        public Online.Models.Broker Broker { get; set; }

        [Parameter]
        public IEnumerable<Online.Models.Broker> Brokers { get; set; }

        [Parameter]
        public Online.Models.Customer Customer { get; set; }

        protected CustomerDocumentDto CustomerProfilePicture { get; set; }

        protected OccupationDto Occupation { get; set; }

        protected override async Task OnInitializedAsync()
        {
            CustomerProfilePicture = await CustomerDocumentService.GetProfilePicture(Customer.Id);

            Occupation = await OccupationService.GetById(Customer.OccupationId);
        }

        protected string GetNeedsContent()
        {
            if (!Customer.HasNeeds) return string.Empty;

            var currentDate = DateTime.UtcNow;

            var hasIncomeProtection = Customer.Insurances.Any(i => i.InsType == InsuranceEnum.Income && i.ExpiryDate > currentDate);
            var hasLifeAndIlness = Customer.Insurances.Any(i => i.InsType == InsuranceEnum.Illness && i.ExpiryDate > currentDate);

            if (Customer.Employment == EmploymentEnum.SelfEmployed)
            {
                if (!hasIncomeProtection && !hasLifeAndIlness) return "Customer is self employed, but has neither Income Protection nor Life and Ilness cover.";
                if (!hasIncomeProtection) return "Customer is self employed, but does not have Income Protection cover.";
                if (!hasLifeAndIlness) return "Customer is self employed, but does not have Life and Ilness cover.";
            }

            if (Customer.Employment == EmploymentEnum.Employed)
            {
                return "Customer is employed, but does not have Life and Ilness cover.";
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
            if (!result.Cancelled)
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
            if (!result.Cancelled)
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

    }
}