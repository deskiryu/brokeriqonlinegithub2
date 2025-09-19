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
using System.Diagnostics;

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

    protected IEnumerable<TriggerDto> _triggers;

    protected IEnumerable<ActivityDto> _activities;

    protected string ActiveButtonLabel => Workflow.IsActive ? "Deactivate" : "Activate";

    protected Color ActiveButtonColour => Workflow.IsActive ? Color.Error : Color.Success;

    protected bool WorkflowIsNotValid => string.IsNullOrWhiteSpace(Workflow.Name) ||
            Workflow.TriggerId == 0 ||
            !Workflow.Steps.Any() ||
            Workflow.Steps.Any(s => s.ActivityId == 0 || s.StepParameters.Any(p => string.IsNullOrWhiteSpace(p.Value)));

    protected override async Task OnInitializedAsync()
    {
        _triggers = await WorkflowService.GetTriggersAsync();
        _activities = await WorkflowService.GetActivitiesAsync();

        if (!string.IsNullOrWhiteSpace(WorkflowId))
        {
            Workflow = await WorkflowService.GetWorkflowAsync(WorkflowId);
        }
    }

    protected async Task Save()
    {
        if (WorkflowIsNotValid)
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
            var activity = _activities.First(a => a.Id == step.ActivityId);

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
