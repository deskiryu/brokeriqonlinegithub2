using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Pages
{
    using Microsoft.AspNetCore.Components;
    using Models;
    using Services.Interface;

    public class MortgageDetailBase : ComponentBase
    {
        private int id;
        private string mortgageId;

        [Inject]
        public IMortgageService MortgageService { get; set; }

        public Mortgage Mortgage { get; set; }

        [Parameter]
        public string MortgageId
        {
            get => this.mortgageId;
            set
            {
                this.mortgageId = value;
                this.id = int.Parse(value);
            }
        }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                Mortgage = (await MortgageService.GetMortgage(this.id));
            }
            catch
            {

            }
        }
    }
}
