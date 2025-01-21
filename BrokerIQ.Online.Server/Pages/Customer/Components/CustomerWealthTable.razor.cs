using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;

namespace BrokerIQ.Online.Server.Pages.Customer.Components
{
    public partial class CustomerWealthTable : ComponentBase
	{
        [Inject]
        public IWealthService WealthService { get; set; }

        [Parameter]
        public Online.Models.User User { get; set; }

        [Parameter]
        public Online.Models.Customer Customer { get; set; }

        [Parameter]
        public IEnumerable<Online.Models.Broker> Brokers { get; set; }

        [Parameter]
        public IEnumerable<Online.Models.Wealth> Wealths { get; set; }

        public CustomerWealthTable()
        {
            Wealths = Array.Empty<Online.Models.Wealth>();
        }

        protected override async Task OnInitializedAsync()
        {
            Wealths = await WealthService.GetForCustomer(Customer.Id);
        }
    }
}

