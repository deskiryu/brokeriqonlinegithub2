using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Dto.Entities;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Server.Components;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Customer.Components
{
    public partial class CustomerGoalTable
    {
        [Inject]
        public IDialogService DialogService { get; set; }

        [Inject]
        public IGoalService GoalService { get; set; }

        [Inject]
        public ICustomerGoalService CustomerGoalService { get; set; }

        [Parameter]
        public Online.Models.Broker Broker { get; set; }

        [Parameter]
        public Online.Models.Customer Customer { get; set; }

        protected IEnumerable<GoalDto> Goals { get; set; } = Array.Empty<GoalDto>();

        protected IEnumerable<CustomerGoalDto> CustomerGoals { get; set; } = Array.Empty<CustomerGoalDto>();

        protected override async Task OnParametersSetAsync()
        {
            await LoadGoalData();

            await base.OnParametersSetAsync();
        }

        private async Task LoadGoalData()
        {
            Goals = await GoalService.GetAllForBroker(Broker.Id);

            CustomerGoals = await CustomerGoalService.Get(Customer.Id);
        }

        protected string GoalDescriptionFor(CustomerGoalDto customerGoal)
        {
            var brokerGoal = Goals.First(g => g.Id == customerGoal.GoalId);
            var goalText = brokerGoal.Description;
            if (brokerGoal.RequiresAdditionalInfo)
            {
                goalText += " : " + customerGoal.AdditionalInfo;
            }

            return goalText;
        }

        private async Task RemoveCustomerGoal(CustomerGoalDto customerGoal)
        {
            var parameters = new DialogParameters
            {
                { "ContentText", "Do you really want to delete this customer goal? This process cannot be undone." },
                { "ButtonText", "Delete" },
                { "Color", Color.Error }
            };

            var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };

            var result = await DialogService.Show<ConfirmationDialog>("Delete", parameters, dialogOptions).Result;

            if (!result.Canceled)
            {
                await CustomerGoalService.Delete(customerGoal.Id);
            }

            await LoadGoalData();
        }

        private async Task EditCustomerGoal(CustomerGoalDto customerGoal)
        {
            var operation = customerGoal.Id == 0 ? "Create" : "Edit";
            var title = $"{operation} {customerGoal}";
            var parameters = new DialogParameters
            {
                { "CustomerGoal", customerGoal },
                { "Goals", Goals }
            };

            var options = new DialogOptions() { MaxWidth = MaxWidth.Small, FullWidth = true };

            var result = await DialogService.Show<CustomerGoalDialog>(title, parameters, options).Result;

            if (!result.Canceled)
            {
                CustomerGoalDto updated = result.Data as CustomerGoalDto;

                var wasSuccessfull = false;
                if (customerGoal.Id == 0)
                {
                    wasSuccessfull = await CustomerGoalService.Create(new CreateCustomerGoalDto()
                    {
                        CustomerId = Customer.Id,
                        GoalId = updated.GoalId,
                        StartDate = updated.StartDate,
                        NextReviewDate = updated.NextReviewDate,
                        EndDate = updated.EndDate,
                        AdditionalInfo = updated.AdditionalInfo
                    });
                }
                else
                {
                    wasSuccessfull = await CustomerGoalService.Update(new UpdateCustomerGoalDto()
                    {
                        Id = updated.Id,
                        CustomerId = Customer.Id,
                        GoalId = updated.GoalId,
                        StartDate = updated.StartDate,
                        NextReviewDate = updated.NextReviewDate,
                        EndDate = updated.EndDate,
                        AdditionalInfo = updated.AdditionalInfo
                    });
                }

                // TODO : Re introduce these when a fix for the parsing error has been found
                // if (wasSuccessfull)
                // {
                //     Snackbar.Add("Reminder option was removed.", Severity.Success);
                // }
                // else
                // {
                //     Snackbar.Add("Reminder options update failed. Please try again.", Severity.Error);
                // } 
            }

            await LoadGoalData();
        }
    }
}

