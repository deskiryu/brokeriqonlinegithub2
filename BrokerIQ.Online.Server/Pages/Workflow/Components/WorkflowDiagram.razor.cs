using System.Collections.Generic;
using System.Linq;
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

    protected void AddStep()
    {
        var newStep = new StepDto();
        if (Workflow.Steps.Any())
        {
            var last = Workflow.Steps.Last();

            last.NextStepId = newStep.Id;
        }

        Workflow.Steps.Add(newStep);
    }

    void RemoveStep(StepDto step)
    {
        Workflow.Steps.Remove(step);
        foreach (var s in Workflow.Steps)
        {
            if (s.NextStepId == step.Id)
            {
                s.NextStepId = null;
            }
        }
    }

    // bool CanAddStep(StepDto step)
    // {
    //     var selected = ActivityOptions.FirstOrDefault(a => a.Id == step.ActivityId);
    //     if (selected is null) return false;

    //     if (selected.Parameters.Count() != step.StepParameters.Count()) return false;

    //     return step.StepParameters.Any(s => string.IsNullOrWhiteSpace(s.Value));
    // }
}