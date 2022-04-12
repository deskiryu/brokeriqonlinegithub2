using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Pages
{
    using Microsoft.AspNetCore.Components;
    using Models;
    using Services.Interface;

    public class InsuranceDetailBase : ComponentBase
    {
        private int id;
        private string insuranceId;

        [Inject]
        public IInsuranceService InsuranceService { get; set; }

        public Insurance Insurance { get; set; }

        [Parameter]
        public string InsuranceId
        {
            get => this.insuranceId;
            set
            {
                this.insuranceId = value;
                this.id = int.Parse(value);
            }
        }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                Insurance = (await InsuranceService.GetInsurance(this.id));
            }
            catch
            {

            }
        }
    }
}
