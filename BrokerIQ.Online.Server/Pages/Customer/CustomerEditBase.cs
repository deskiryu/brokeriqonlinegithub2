using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Dto;
using BrokerIQ.Dto.Enum;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.AppSettings;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Extensions;
using BrokerIQ.Online.Server.Services.Interface;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace BrokerIQ.Online.Pages
{
    public class CustomerEditBase : ComponentBase
    {
        protected const int MIN_OCCUPATION_CHARS = 2;

        private int id;
        private int customerId;
        private string strCustomerId;

        [Inject]
        public ICustomerService CustomerService { get; set; }

        [Inject]
        public IInsuranceDocumentService SupportingDocumentService { get; set; }

        [Inject]
        public IOccupationService OccupationService { get; set; }

        [Inject]
        public IWealthTypeService WealthTypeService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IOptions<ReviewItAPIDetails> api { get; set; }

        [Inject]
        public IBrokerService BrokerService { get; set; }

        public Broker Broker { get; set; }

        public Customer Customer { get; set; }

        public CustomerCategoryEnum[] CustomerCategoriesByRelevance;

        public IEnumerable<WealthTypeDto> WealthTypes;

        public int SelectedCustomerCategory { get { return (int)Customer.CustomerCategory; } set { Customer.CustomerCategory = (CustomerCategoryEnum)value; } }

        public int SelectedGender { get { return (int)Customer.Gender; } set { Customer.Gender = (GenderEnum)value; } }

        public int SelectedEmploymentStatus { get { return (int)Customer.Employment; } set { Customer.Employment = (EmploymentEnum)value; } }

        public int SelectedResidentialStatus { get { return (int)Customer.ResidentialStatus; } set { Customer.ResidentialStatus = (ResidentialStatusEnum)value; } }

        protected string Message = string.Empty;

        protected string StatusClass = string.Empty;

        protected bool Saved;

        protected OccupationDto SelectedOccupation { get; set; }

        [Parameter]
        public string CustomerId { get; set; }

        public CustomerEditBase()
        {
            Customer = new Customer();
        }

        protected override async Task OnInitializedAsync()
        {
            CustomerCategoriesByRelevance = ExtensionClass.GetAllCustomerCategories();

            id = Int32.Parse(CustomerId);

            if (id > 0)
            {
                Customer = await CustomerService.GetCustomer(id);

                SelectedOccupation = await OccupationService.GetById(Customer.OccupationId);

                Broker = await BrokerService.GetBroker(Customer.ChosenBrokerId);

                WealthTypes = await WealthTypeService.GetAllForBroker(Broker.Id);

                if (Broker.BrokerIdentifier.InsuranceOnly)
                {
                    CustomerCategoriesByRelevance = ExtensionClass.GetFilteredCustomerCategories(new int[] { 0, 2 });
                }
            }
        }

        protected void HandleInvalidSubmit()
        {
            StatusClass = "alert-danger";
            Message = "There are some validation errors. Please try again.";
            Saved = false;
        }

        protected async Task HandleValidSubmit()
        {
            StatusClass = "alert-success";
            Message = "Customer updated successfully.";
            try
            {
                if (api.Value.ShowProtection)
                {
                    Customer.OccupationId = SelectedOccupation?.Id ?? 0;
                }

                await CustomerService.UpdateCustomer(Customer);
            }
            catch
            {
                StatusClass = "alert-danger";
                Message = "Something went wrong updating the Customer. Please try again.";
            }
            finally
            {
                Saved = true;
            }
        }

        protected void NavigateToOverview()
        {
            NavigationManager.NavigateTo($"/clientdetail/{Customer.TargetCustomerId}");
        }

        protected async Task<IEnumerable<OccupationDto>> SearchOccupations(string partial)
        {
            return await OccupationService.Search(partial);
        }
    }
}
