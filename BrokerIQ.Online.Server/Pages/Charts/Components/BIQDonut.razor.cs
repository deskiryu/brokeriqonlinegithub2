using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Charts.Components
{
    public partial class BIQDonut : BIQDashboardComponent
    {
        [Inject]
        public IDialogService DialogService { get; set; }

        [Parameter]
        public string Height { get; set; } = "250px";

        [Parameter]
        public string[] Labels { get; set; }

        [Parameter]
        public double[] Data { get; set; }

        [Parameter]
        public string NoDataMessage { get; set; } = "No Data available";

        [Parameter]
        public string KPIValue { get; set; }

        [Parameter]
        public string KPILabel { get; set; }
    }
}