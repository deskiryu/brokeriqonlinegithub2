using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Charts.Components
{
    public partial class BIQChart : BIQDashboardComponent
    {
        [Inject]
        public IDialogService DialogService { get; set; }

        [Parameter]
        public ChartType ChartType { get; set; }

        [Parameter]
        public string[] XAxisLabels { get; set; }

        [Parameter]
        public List<ChartSeries> ChartSeries { get; set; }

        [Parameter]
        public Position LegendPosition { get; set; }

        [Parameter]
        public string NoDataMessage { get; set; } = "No Data available";

        protected double MaxValue = 5;        

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            MaxValue = 0;

            if (ChartSeries is null) return;

            foreach (var series in ChartSeries)
            {
                if (series.Data.Length == 0) continue;

                var max = series.Data.Max();

                if (max > MaxValue) MaxValue = max;
            }
        }
    }
}