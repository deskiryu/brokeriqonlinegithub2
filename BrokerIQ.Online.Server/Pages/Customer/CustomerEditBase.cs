namespace BrokerIQ.Online.Pages
{
    using System;
    using System.Threading.Tasks;
    using BrokerIQ.Dto.Enum;
    using BrokerIQ.Online.Server.Extensions;
    using Microsoft.AspNetCore.Components;
    using Models;
    using Services.Interface;

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
        public NavigationManager NavigationManager { get; set; }

        public Customer Customer { get; set; }

        public CustomerCategoryEnum[] CustomerCategoriesByRelevance;

        public int SelectedCustomerCategory { get { return (int)Customer.CustomerCategory; } set { Customer.CustomerCategory = (CustomerCategoryEnum)value; } }

        protected string Message = string.Empty;
        protected string StatusClass = string.Empty;
        protected bool Saved;

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
    }
}
