using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;

namespace BrokerIQ.Online.Server.Pages.Customer.Components
{
    public partial class CustomerPensionTable : ComponentBase
	{
        [Inject]
        public IPensionService PensionService { get; set; }

        [Parameter]
        public Online.Models.User User { get; set; }

        [Parameter]
        public Online.Models.Customer Customer { get; set; }

        [Parameter]
        public IEnumerable<Online.Models.Broker> Brokers { get; set; }

        [Parameter]
        public IEnumerable<Online.Models.Pension> Pensions { get; set; }

        public CustomerPensionTable()
        {
            Pensions = Array.Empty<Online.Models.Pension>();
        }

        protected override async Task OnInitializedAsync()
        {
            Pensions = await PensionService.GetForCustomer(Customer.Id);
        }
    }
}

