using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Dto.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Customer.Components
{
    public partial class CustomerGoalDialog
    {
        [Inject]
        public IDialogService DialogService { get; set; }

        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public CustomerGoalDto CustomerGoal { get; set; }

        [Parameter]
        public IEnumerable<GoalDto> Goals { get; set; }

        MudForm form;

        protected GoalDto SelectedGoal { get; set; }

        protected DateTime? StartDate { get; set; }

        protected DateTime? NextReviewDate { get; set; }

        protected DateTime? EndDate { get; set; }

        protected string AdditionalInfoClass => SelectedGoal is not null && SelectedGoal.RequiresAdditionalInfo ? string.Empty : "d-none";

        protected override Task OnParametersSetAsync()
        {
            SelectedGoal = Goals.FirstOrDefault(g => g.Id == CustomerGoal.GoalId);
            StartDate = CustomerGoal.StartDate;
            NextReviewDate = CustomerGoal.NextReviewDate;
            EndDate = CustomerGoal.EndDate;

            return base.OnParametersSetAsync();
        }

        async Task Submit()
        {
            await form.Validate();

            if (form.IsValid)
            {
                CustomerGoal.GoalId = SelectedGoal.Id;
                CustomerGoal.StartDate = StartDate.Value;
                CustomerGoal.NextReviewDate = NextReviewDate.Value;
                CustomerGoal.EndDate = EndDate.Value;

                MudDialog.Close(DialogResult.Ok(CustomerGoal));
            }
        }

        void Cancel() => MudDialog.Cancel();
    }
}