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

    private MudMenu activityMenu;

    public IEnumerable<Models.Video> BrokerVideos { get; set; }

    public IEnumerable<ActivityDto> Actions => Activities.Where(a => !a.IsBranchingActivity).ToArray();

    public IEnumerable<ActivityDto> BranchingActions => Activities.Where(a => a.IsBranchingActivity).ToArray();

    private int SelectedActivityId
    {
        get => Step.ActivityId;
        set
        {
            if (Step.ActivityId == value) return;

            Step.ActivityId = value;
            Step.StepParameters = BuildCurrentParameters();

            if (BranchingActions.FirstOrDefault(a => a.Id == Step.ActivityId) == null)
            {
                Step.AlternateStepId = null;
            }

            HasChanged();
        }
    }

    private string ActivityLabel
    {
        get
        {
            var activity = Activities.FirstOrDefault(a => a.Id == SelectedActivityId);

            if (activity == null) return string.Empty;

            if(activity.Id == -1) return "End";

            if (activity.IsBranchingActivity) return "Check";

            if (!activity.Parameters.Any()) return "Action";

            return "Edit";
        }
    }

    private Color ActivityColor
    {
        get
        {
            var activity = Activities.FirstOrDefault(a => a.Id == SelectedActivityId);

            if (activity == null) return Color.Default;

            if (activity.Parameters.Any()) return Color.Primary;

            return Color.Default;
        }

    }

    public ActivityDto SelectedActivity => Activities.FirstOrDefault(a => a.Id == Step.ActivityId);

    public string BuildParameterText(StepParameterDto param)
    {
        const int textMaxLength = 30;

        var activityParam = ActivityParameters.First(p => p.Order == param.Order);

        switch (activityParam.Type)
        {
            case "text":
                var endText = param.Value.Length <= textMaxLength ? string.Empty : " ...";
                return $"{param.Value[..(param.Value.Length > textMaxLength ? textMaxLength : param.Value.Length)]}{endText}";
            case "template":
                endText = param.Value.Length <= textMaxLength ? string.Empty : " ...";
                return $"{param.Value[..(param.Value.Length > textMaxLength ? textMaxLength : param.Value.Length)]}{endText}";
            case "videourl":
                var video = BrokerVideos.FirstOrDefault(v => v.Id.ToString() == param.Value);
                if (video != null) return $" With video {video.Name}";
                break;
            case "timespan":
                return TimeSpan.TryParse(param.Value, out var timespan) ?
                     $"Put workflow on hold for {timespan.Humanize()}" :
                     "Put workflow on hold for a time";
            default:
                break;
        }

        return string.Empty;
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
    }

    protected async void SetActivity(int activityId)
    {
        SelectedActivityId = activityId;

        activityMenu.CloseMenu();

        await EditParameters();
    }

    async Task EditParameters()
    {
        if (!SelectedActivity.Parameters.Any()) return;

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
}
