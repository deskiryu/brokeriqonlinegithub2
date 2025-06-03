using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Charts.Components
{
    public partial class BIQPeriodChange : ComponentBase
    {
        [Parameter]
        public double? ChangePercent { get; set; }

        private string TrendingIcon { get; set; }
        private string ChangeStyle { get; set; }

        private string FormattedChangePercent { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            if (ChangePercent is null)
            {
                TrendingIcon = string.Empty;
                ChangeStyle = string.Empty;
                FormattedChangePercent = string.Empty;
                return;
            }

            FormattedChangePercent = $"{ChangePercent:P2}";

            if (ChangePercent == 0)
            {
                TrendingIcon = Icons.Material.Filled.TrendingFlat;
                ChangeStyle = $"color :{Colors.Grey.Darken4}";
            }
            else if (ChangePercent > 0)
            {
                TrendingIcon = Icons.Material.Filled.TrendingUp;
                ChangeStyle = $"color :{Colors.Green.Darken4}";
                FormattedChangePercent = $"+{ChangePercent:P2}";
            }
            else
            {
                TrendingIcon = Icons.Material.Filled.TrendingDown;
                ChangeStyle = $"color :{Colors.Red.Darken4}";
            }
        }
    }
}

