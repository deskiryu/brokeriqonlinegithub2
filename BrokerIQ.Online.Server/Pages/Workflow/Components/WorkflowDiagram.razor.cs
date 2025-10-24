using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Dto.Dto.Workflows;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;

namespace BrokerIQ.Online.Server.Pages.Workflow.Components;

public partial class WorkflowDiagram
{
    [Inject]
    public IAccountService AccountService { get; set; }

    [Inject]
    public IVideoService VideoService { get; set; }

    [Parameter]
    public WorkflowDto Workflow { get; set; } = new();

    [Parameter]
    public IEnumerable<TriggerDto> Triggers { get; set; } = Enumerable.Empty<TriggerDto>();

    [Parameter]
    public IEnumerable<ActivityDto> Activities { get; set; } = Enumerable.Empty<ActivityDto>();

    [Parameter] public EventCallback OnChanged { get; set; }
    private Task HasChanged() => OnChanged.InvokeAsync();

    private User User { get; set; }

    public IEnumerable<Models.Video> BrokerVideos { get; set; }

    protected override async Task OnInitializedAsync()
    {
        User = await AccountService.GetUser();

        BrokerVideos = (await VideoService.GetVideos(User.MasterBrokerId)).ToList();
    }

    protected void AddInitialStep()
    {
        var newStep = new StepDto();

        Workflow.Steps.Add(newStep);

        HasChanged();
    }

    private void OnPathChange()
    {
        HasChanged();
    }
}