using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Dto.Dto.Workflows;
using Microsoft.AspNetCore.Components;

namespace BrokerIQ.Online.Server.Pages.Workflow.Components;

public partial class WorkflowDiagram
{
    [Parameter]
    public WorkflowDto Workflow { get; set; } = new();

    [Parameter]
    public IEnumerable<ActivityDto> ActivityOptions { get; set; } = Enumerable.Empty<ActivityDto>();

    [Parameter]
    public IEnumerable<TriggerDto> TriggerOptions { get; set; } = Enumerable.Empty<TriggerDto>();

    [Parameter] public EventCallback OnChanged { get; set; }
    private Task HasChanged() => OnChanged.InvokeAsync();

    public IEnumerable<ActivityDto> CheckActivityOptions => ActivityOptions.Where(a => a.IsCheckActivity).ToArray();

    public IEnumerable<ActivityDto> NonCheckActivityOptions => ActivityOptions.Where(a => !a.IsCheckActivity).ToArray();

    public ActivityDto FinishActivity => ActivityOptions.First(a => a.Id == -1);

    protected void AddStep()
    {
        var newStep = new StepDto();

        if (Workflow.Steps.Any())
        {
            var last = Workflow.Steps.Last();

            if (last.IsBranchOption)
            {
                var lastBranch = Workflow.Steps.Last(s => GetCheckActivitiesIds().Contains(s.ActivityId));
                var otherOption = Workflow.Steps.First(a => a.Id == lastBranch.NextStepId);
                otherOption.NextStepId = newStep.Id;
            }

            last.NextStepId = newStep.Id;
        }

        Workflow.Steps.Add(newStep);

        HasChanged();
    }

    void RemoveStep(StepDto step)
    {
        var stepsToRemoveIds = new List<string>();

        if (GetCheckActivitiesIds().Contains(step.ActivityId))
        {
            if (step.NextStepId is not null) stepsToRemoveIds.Add(step.NextStepId);
            if (step.AlternateStepId is not null) stepsToRemoveIds.Add(step.AlternateStepId);
        }

        stepsToRemoveIds.Add(step.Id);
        var stepsToRemove = Workflow.Steps.Where(s => stepsToRemoveIds.Contains(s.Id)).ToArray();

        foreach (var item in stepsToRemove)
        {
            Workflow.Steps.Remove(item);
        }

        foreach (var s in Workflow.Steps)
        {
            if (stepsToRemoveIds.Contains(s.NextStepId))
            {
                s.NextStepId = null;
            }

            if (stepsToRemoveIds.Contains(s.AlternateStepId))
            {
                s.AlternateStepId = null;
            }
        }

        HasChanged();
    }

    private void ActivityChanged()
    {
        FixNonBranchingSteps();

        FixBranchingSteps();

        HasChanged();
    }

    private void FixNonBranchingSteps()
    {
        var steps = Workflow.Steps.Where(s => GetNonCheckActivitiesIds().Contains(s.ActivityId)).ToArray();

        foreach (var step in steps)
        {
            // remove alternate step from regular steps
            if (!string.IsNullOrWhiteSpace(step.AlternateStepId))
            {
                var toRemove = Workflow.Steps.FirstOrDefault(s => s.Id == step.AlternateStepId);
                if (toRemove is not null) Workflow.Steps.Remove(toRemove);

                step.AlternateStepId = null;
            }
        }
    }

    private int[] GetNonCheckActivitiesIds()
    {
        return NonCheckActivityOptions.Select(a => a.Id).ToArray();
    }

    private int[] GetCheckActivitiesIds()
    {
        return CheckActivityOptions.Select(a => a.Id).ToArray();
    }

    private void FixBranchingSteps()
    {
        var steps = Workflow.Steps.Where(s => GetCheckActivitiesIds().Contains(s.ActivityId)).ToArray();

        foreach (var step in steps)
        {
            // branching steps will always need to have both steps

            if (string.IsNullOrWhiteSpace(step.NextStepId))
            {
                var newStep = new StepDto() { IsBranchOption = true };
                Workflow.Steps.Add(newStep);

                step.NextStepId = newStep.Id;
            }

            if (string.IsNullOrWhiteSpace(step.AlternateStepId))
            {
                var newStep = new StepDto() { IsBranchOption = true };
                Workflow.Steps.Add(newStep);

                step.AlternateStepId = newStep.Id;
            }
        }
    }
}