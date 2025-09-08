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

    protected IEnumerable<TriggerDto> _triggerOptions;

    protected IEnumerable<ActivityDto> _activityOptions;

    protected string ActiveButtonLabel => Workflow.IsActive ? "Deactivate" : "Activate";

    protected Color ActiveButtonColour => Workflow.IsActive ? Color.Error : Color.Success;

    protected bool WorkflowIsNotValid => string.IsNullOrWhiteSpace(Workflow.Name) ||
            Workflow.TriggerId == 0 ||
            !Workflow.Steps.Any() ||
            Workflow.Steps.Any(s => s.ActivityId == 0 || s.StepParameters.Any(p => string.IsNullOrWhiteSpace(p.Value)));

    protected override async Task OnInitializedAsync()
    {
        _triggerOptions = await WorkflowService.GetTriggersAsync();
        _activityOptions = await WorkflowService.GetActivitiesAsync();

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
