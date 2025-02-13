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
        public int BrokerId { get; set; }

        [Parameter]
        public IEnumerable<Online.Models.Broker> Brokers { get; set; }

        [Parameter]
        public IEnumerable<Online.Models.Wealth> WealthRecords { get; set; }

        public CustomerWealthTable()
        {
            WealthRecords = Array.Empty<Online.Models.Wealth>();
        }

        protected override async Task OnInitializedAsync()
        {
            WealthRecords = await WealthService.GetForCustomer(Customer.Id);
        }
    }
}

