using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Models;
using BrokerIQ.Dto.UpdateDto;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Components;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Settings.Components
{
    public partial class GoalTable : ComponentBase
    {
        [Inject]
        private IDialogService DialogService { get; set; }

        [Inject]
        private ISnackbar Snackbar { get; set; }

        [Inject]
        public IGoalService GoalService { get; set; }

        [Parameter]
        public User User { get; set; }

        [Parameter]

        public Online.Models.Broker Broker { get; set; }

        private ICollection<GoalDto> Goals { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Goals = new List<GoalDto>(await GoalService.GetAllForBroker(Broker.Id));
        }

        private async Task RemoveGoal(GoalDto Goal)
        {
            var parameters = new DialogParameters
            {
                { "ContentText", "Do you really want to delete this goal? This process cannot be undone." },
                { "ButtonText", "Delete" },
                { "Color", Color.Error }
            };

            var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };

            var result = await DialogService.Show<ConfirmationDialog>("Delete", parameters, dialogOptions).Result;

            if (!result.Cancelled)
            {
                await GoalService.Delete(Goal);
            }

            await ReloadGoals();
        }

        private async Task ReloadGoals()
        {
            Goals = new List<GoalDto>(await GoalService.GetAllForBroker(Broker.Id));

            StateHasChanged();
        }

        private async Task EditGoal(GoalDto Goal)
        {
            var operation = Goal.Id == 0 ? "Create" : "Edit";
            var title = $"{operation} {Goal.Description}";
            var parameters = new DialogParameters
            {
                { "Goal", Goal }
            };

            var options = new DialogOptions() { MaxWidth = MaxWidth.Small, FullWidth = true };

            var result = await DialogService.Show<GoalDialog>(title, parameters, options).Result;

            if (!result.Canceled)
            {
                GoalDto updated = result.Data as GoalDto;

                bool wasSuccessfull;

                if (Goal.Id == 0)
                {
                    wasSuccessfull = await GoalService.Create(new CreateGoalDto()
                    {
                        BrokerId = updated.BrokerId,
                        Description = updated.Description,
                        RequiresAdditionalInfo = updated.RequiresAdditionalInfo
                    });
                }
                else
                {
                    wasSuccessfull = await GoalService.Update(new UpdateGoalDto()
                    {
                        Id = updated.Id,
                        BrokerId = updated.BrokerId,
                        Description = updated.Description,
                        RequiresAdditionalInfo = updated.RequiresAdditionalInfo
                    });
                }

                var message = wasSuccessfull ? "Goal was saved." : "Goal save failed. Please try again.";
                Snackbar.Add(message, wasSuccessfull ? Severity.Success : Severity.Error);
            }

            await ReloadGoals();
        }
    }
}