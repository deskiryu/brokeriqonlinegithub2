using System.Collections.Generic;

namespace BrokerIQ.Online.Pages
{
    using System.Threading.Tasks;
    using BrokerIQ.Online.Server.Models;
    using BrokerIQ.Online.Services.Interface;
    using Microsoft.AspNetCore.Components;

    public class TrainingVideoListBase : ComponentBase
    {
        [Inject]
        protected ITrainingVideoService TrainingVideoService { get; set; }

        protected List<TrainingVideo> Videos { get; set; } = new List<TrainingVideo>();

        protected override async Task OnInitializedAsync()
        {
            Videos = await TrainingVideoService.GetVideos();
        }
    }
}
