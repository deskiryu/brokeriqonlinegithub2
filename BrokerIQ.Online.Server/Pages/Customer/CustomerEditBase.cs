using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Dto;
using BrokerIQ.Dto.Enum;
using BrokerIQ.Online.Server.Extensions;
using Microsoft.AspNetCore.Components;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Services.Interface;
using BrokerIQ.Online.Server.Services.Interface;
using BrokerIQ.Online.Server;
using static System.Runtime.InteropServices.JavaScript.JSType;
using BrokerIQ.Online.AppSettings;
using Microsoft.Extensions.Options;

namespace BrokerIQ.Online.Pages
{
    public class CustomerEditBase : ComponentBase
    {
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
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IOptions<ReviewItAPIDetails> api { get; set; }

        public Customer Customer { get; set; }

        public CustomerCategoryEnum[] CustomerCategoriesByRelevance;

        public int SelectedCustomerCategory { get { return (int)Customer.CustomerCategory; } set { Customer.CustomerCategory = (CustomerCategoryEnum)value; } }

        public int SelectedGender { get { return (int)Customer.Gender; } set { Customer.Gender = (GenderEnum)value; } }

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
            CustomerCategoriesByRelevance = Extensions.BuildCustomerCategoriesByRelevance();

            id = Int32.Parse(CustomerId);

            if (id > 0)
            {
                Customer = await CustomerService.GetCustomer(id);

                SelectedOccupation = await OccupationService.GetById(Customer.OccupationId);
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
            Message = "Broker updated successfully.";
            try
            {
                if (api.Value.ShowProtection)
                {
                    Customer.OccupationId = SelectedOccupation.Id;
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
            NavigationManager.NavigateTo($"/clientdetail/{CustomerId}");
        }

        protected async Task<IEnumerable<OccupationDto>> SearchOccupations(string partial)
        {
            return await OccupationService.Search(partial);
        }
    }
}
