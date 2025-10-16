using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Dto.Dto.Workflows;
using BrokerIQ.Online.Models;
using Microsoft.AspNetCore.Components;

namespace BrokerIQ.Online.Server.Pages.Workflow.Components;

public partial class WorkflowPath : ComponentBase
{
    [Parameter]
    public User User { get; set; }

    [Parameter]
    public ICollection<StepDto> Steps { get; set; }

    [Parameter]
    public StepDto InitialStep { get; set; }

    [Parameter]
    public IEnumerable<TriggerDto> Triggers { get; set; } = Enumerable.Empty<TriggerDto>();

    [Parameter]
    public IEnumerable<ActivityDto> Activities { get; set; } = Enumerable.Empty<ActivityDto>();

    [Parameter] public EventCallback OnChanged { get; set; }
    private Task HasChanged() => OnChanged.InvokeAsync();

    public IEnumerable<ActivityDto> BranchingActivities => Activities.Where(a => a.IsBranchingActivity).ToArray();

    public ActivityDto FinishActivity => Activities.First(a => a.Id == -1);

    protected bool UsesBranchingActivity(StepDto step)
    {
        return BranchingActivities.Any(a => a.Id == step.ActivityId);
    }

    protected void AddNextStepFor(StepDto previous)
    {
        if (Steps.Any() && previous == null) return;

        var newStep = new StepDto();
        previous.NextStepId = newStep.Id;

        Steps.Add(newStep);

        HasChanged();
    }

    protected void AddAlternateStepFor(StepDto previous)
    {
        if (Steps.Any() && previous == null) return;

        var newStep = new StepDto();
        previous.AlternateStepId = newStep.Id;

        Steps.Add(newStep);

        HasChanged();
    }

    void RemoveStep(StepDto step)
    {
        var stepToRemoveIds = new List<string>() { step.Id };

        var idCount = stepToRemoveIds.Count;

        do
        {
            var additionalIds = Steps.Where(s => stepToRemoveIds.Contains(s.Id))
                .Select(s => s.NextStepId).ToList();

            additionalIds.AddRange(Steps.Where(s => stepToRemoveIds.Contains(s.Id))
                .Select(s => s.AlternateStepId));

            additionalIds = additionalIds.Distinct().ToList();

            stepToRemoveIds.AddRange(additionalIds.Where(s => !string.IsNullOrWhiteSpace(s)));
        } while (idCount < stepToRemoveIds.Count);

        var toRemove = Steps.Where(s => stepToRemoveIds.Contains(s.Id)).ToArray();

        foreach (var stepToRemove in toRemove)
        {
            Steps.Remove(stepToRemove);
        }

        var previousStep = Steps.FirstOrDefault(s => s.NextStepId == step.Id);
        if (previousStep != null) previousStep.NextStepId = null;

        previousStep = Steps.FirstOrDefault(s => s.AlternateStepId == step.Id);
        if (previousStep != null) previousStep.AlternateStepId = null;

        HasChanged();
    }

    private void OnStepChange()
    {
        HasChanged();
    }
}
