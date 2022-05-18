namespace BrokerIQ.Online.Pages
{
    using System;
    using System.Threading.Tasks;
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
            id = Int32.Parse(CustomerId);
            if (id > 0)
            {
                Customer = (await CustomerService.GetCustomer(id));
            }
        }

        protected void HandleInvalidSubmit()
        {
            StatusClass = "alert-danger";
            Message = "There are some validation errors. Please try again.";
        }

        protected async Task HandleValidSubmit()
        {
            await CustomerService.UpdateCustomer(Customer);
            StatusClass = "alert-success";
            Message = "Customer updated successfully.";
            Saved = true;
        }

        protected void NavigateToOverview()
        {
            NavigationManager.NavigateTo($"/clientdetail/{CustomerId}");
        }
    }
}
