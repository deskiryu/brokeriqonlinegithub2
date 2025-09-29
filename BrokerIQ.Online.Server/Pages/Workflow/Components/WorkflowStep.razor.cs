using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Dto.Dto.Workflows;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Extensions;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Workflow.Components;

public partial class WorkflowStep
{
    [Inject]
    IDialogService DialogService { get; set; }

    [Inject]
    public IVideoService VideoService { get; set; }

    [Parameter]
    public User User { get; set; }

    [Parameter]
    public StepDto Step { get; set; } = new();

    [Parameter]
    public IEnumerable<ActivityDto> Activities { get; set; } = Enumerable.Empty<ActivityDto>();

    [Parameter] public EventCallback OnChanged { get; set; }
    private Task HasChanged() => OnChanged.InvokeAsync();

    public IEnumerable<Models.Video> BrokerVideos { get; set; }

    private int SelectedActivityId
    {
        get => Step.ActivityId;
        set
        {
            if (Step.ActivityId == value) return;

            Step.ActivityId = value;
            Step.StepParameters = BuildCurrentParameters();

            if (!Activities.First(a => a.Id == Step.ActivityId).IsBranchingActivity)
            {
                Step.AlternateStepId = null;
            }

            InEditMode = Activities.First(a => a.Id == Step.ActivityId).Parameters.Any();

            HasChanged();
        }
    }

    private string ActivityLabel
    {
        get
        {
            var activity = Activities.FirstOrDefault(a => a.Id == SelectedActivityId);

            if (activity == null) return string.Empty;

            if (activity.IsEventActivity) return "When";

            if (activity.IsBranchingActivity) return "Check";

            return "Action";
        }
    }

    public bool InEditMode { get; set; }

    public string SelectedAction
    {
        get
        {
            var action = Activities.FirstOrDefault(a => a.Id == Step.ActivityId);
            return action == null ? string.Empty : action.Description;
        }
    }

    public MarkupString ParametersText
    {
        get
        {
            if (!Step.StepParameters.Any() || BrokerVideos == null) return new MarkupString();

            var result = "";
            foreach (var param in Step.StepParameters)
            {
                var activityParam = ActivityParameters.First(p => p.Order == param.Order);

                switch (activityParam.Type)
                {
                    case "text":
                        result += $"<div>{activityParam.Label}: \" {param.Value}\"</div>";
                        break;
                    case "template":
                        result += $"<div>{activityParam.Label}: \"{param.Value}\"</div>";
                        break;
                    case "timespan":
                        result += TimeSpan.TryParse(param.Value, out var timespan) ?
                             $"<div>{activityParam.Label}: {timespan.Humanize()}</div>" :
                             $"<div>{activityParam.Label}:</div>";
                        break;
                    case "videourl":
                        var video = BrokerVideos.FirstOrDefault(v => v.Id.ToString() == param.Value);
                        if (video != null) result += $"<div>{activityParam.Label}: {video.Name}</div>";
                        break;
                    default:
                        continue;
                }
            }

            return new MarkupString(result);
        }
    }

    private IEnumerable<ActivityParameterDto> ActivityParameters
    {
        get
        {
            var selected = Activities.FirstOrDefault(a => a.Id == Step.ActivityId);
            if (selected is null) return Array.Empty<ActivityParameterDto>();

            return selected.Parameters;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        BrokerVideos = (await VideoService.GetVideos(User.MasterBrokerId)).ToList();
        InEditMode = Step.ActivityId == 0;
    }

    async Task EditParameters()
    {
        var selectedActivity = Activities.First(a => a.Id == SelectedActivityId);
        var dialogParams = new DialogParameters
            {
                { "Activity", selectedActivity},
                { "Step", Step },
                { "User", User },
                { "BrokerVideos", BrokerVideos }

            };

        var options = new DialogOptions() { MaxWidth = MaxWidth.Medium };

        var dialogResult = await DialogService.Show<ParameterValueDialog>("Set Parameter(s)", dialogParams, options).Result;

        if (!dialogResult.Canceled)
        {
            Step = dialogResult.Data as StepDto;

            var required = selectedActivity.Parameters.Where(p => p.IsRequired);
            InEditMode = Step.StepParameters.Any(p => required.Any(r => r.Order == p.Order) && string.IsNullOrWhiteSpace(p.Value));

            await HasChanged();
        }
    }

    private StepParameterDto[] BuildCurrentParameters()
    {
        var currentParameters = Step.StepParameters.ToArray();
        StepParameterDto[] newParameters = ActivityParameters.Select(p => new StepParameterDto() { Order = p.Order, Value = string.Empty }).ToArray();

        for (int i = 0; i < newParameters.Length; i++)
        {
            if (i >= currentParameters.Length) break;

            newParameters[i].Value = currentParameters[i].Value;
        }

        return newParameters;
    }

    private void ResetStep()
    {
        InEditMode = true;
    }
}
