using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Dto;
using BrokerIQ.Dto.Enum;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;

namespace BrokerIQ.Online.Server.Pages.Settings.Components
{
    public partial class ReminderOptionTable
    {
        [Inject]
        public IBrokerReminderOptionService BrokerReminderOptionService { get; set; }

        [Parameter]
        public User User { get; set; }

        [Parameter]
        public Online.Models.Broker Broker { get; set; }

        private IEnumerable<ReminderOptionDto> ReminderOptions { get; set; }

        protected override async Task OnInitializedAsync()
        {
            ReminderOptions = await BrokerReminderOptionService.GetAllForCurrentBroker();
        }

        protected string GetReminderTargetName(int targetId){
            return ((ReminderTargetEnum)targetId).ToString();
        }
    }
}