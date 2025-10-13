using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using BrokerIQ.Dto.Dto.Workflows;
using BrokerIQ.Online.Server.Services.Interface;
using MudBlazor;
using BrokerIQ.Online.Server.Shared;
using System;
using BrokerIQ.Online.Server.Components;
using System.Linq;

namespace BrokerIQ.Online.Server.Pages.Workflow;

public class WorkflowEditBase : ComponentBase
{
    [Inject]
    ISnackbar Snackbar { get; set; }

    [Inject]
    private IDialogService DialogService { get; set; }

    [Inject]
    public IWorkflowService WorkflowService { get; set; }

    [Parameter]
    public string WorkflowId { get; set; }

    protected WorkflowDto Workflow = new();

    protected IEnumerable<TriggerDto> Triggers { get; set; }

    protected IEnumerable<ActivityDto> Activities { get; set; }

    protected string ActiveButtonLabel => Workflow.IsActive ? "Deactivate" : "Activate";

    protected Color ActiveButtonColour => Workflow.IsActive ? Color.Error : Color.Success;

    protected bool DisableSaveButton
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Workflow.TriggerKey)) return true;

            if (string.IsNullOrWhiteSpace(Workflow.Name) || Workflow.Steps.Count < 1) return true;

            foreach (var step in Workflow.Steps)
            {
                var stepActivity = Activities.FirstOrDefault(a => a.Id == step.ActivityId);

                if (stepActivity == null) return true;

                var requiredOrder = stepActivity.Parameters.Where(p => p.IsRequired).Select(p => p.Order).ToList();

                if (step.StepParameters.Any(p => requiredOrder.Contains(p.Order) && string.IsNullOrWhiteSpace(p.Value))) return true;
            }

            return false;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        Triggers = await WorkflowService.GetTriggersAsync();

        Activities = await WorkflowService.GetActivitiesAsync();

        if (!string.IsNullOrWhiteSpace(WorkflowId))
        {
            Workflow = await WorkflowService.GetWorkflowAsync(WorkflowId);
        }
    }

    protected async Task Save()
    {
        if (DisableSaveButton)
        {
            return;
        }

        if (Workflow.Id == Guid.Empty)
        {
            var parameters = new DialogParameters() {
                { "ContentText", "Would you like this workflow to be active?" },
                { "CancelText", "No"},
                { "ButtonText", "Yes"}
            };
            var result = await DialogService.Show<ConfirmationDialog>("Active Status", parameters).Result;

            Workflow.IsActive = !result.Canceled;
        }

        if (Workflow.IsActive)
        {
            CleanLooseEnds();
        }

        try
        {
            Workflow = await WorkflowService.SaveAsync(Workflow);

            Snackbar.Add("Workflow saved successfully.", Severity.Success);
        }
        catch
        {
            Snackbar.Add("Unable to save workflow.", Severity.Error);
        }
    }

    private void CleanLooseEnds()
    {
        var unfinishedSteps = Workflow.Steps
            .Where(s => s.ActivityId != -1 && (string.IsNullOrWhiteSpace(s.NextStepId) || string.IsNullOrWhiteSpace(s.AlternateStepId)))
            .ToList();

        foreach (var step in unfinishedSteps)
        {
            var activity = Activities.First(a => a.Id == step.ActivityId);

            if (string.IsNullOrWhiteSpace(step.NextStepId))
            {
                var newStep = new StepDto() { ActivityId = -1 };
                step.NextStepId = newStep.Id;

                Workflow.Steps.Add(newStep);
            }

            if (activity.IsBranchingActivity && string.IsNullOrWhiteSpace(step.AlternateStepId))
            {
                var newStep = new StepDto() { ActivityId = -1 };
                step.AlternateStepId = newStep.Id;

                Workflow.Steps.Add(newStep);
            }
        }
    }

    protected async Task SetActiveStatus()
    {
        var action = Workflow.IsActive ? "deactivate" : "activate";
        var parameters = new DialogParameters() {
            { "Message", $"Are you sure you want to {action} this workflow?" }
        };
        var result = await DialogService.Show<ConfirmCancelDialog>("Active Status", parameters).Result;

        if (result.Canceled) return;

        Workflow.IsActive = !Workflow.IsActive;
        await Save();
    }

    protected void HandleChildChanged()
    {
        StateHasChanged();
    }
}
