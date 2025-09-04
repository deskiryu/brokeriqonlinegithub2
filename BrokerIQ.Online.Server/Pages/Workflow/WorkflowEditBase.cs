using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using BrokerIQ.Dto.Dto.Workflows;
using BrokerIQ.Online.Server.Services.Interface;

namespace BrokerIQ.Online.Server.Pages.Workflow;

public class WorkflowEditBase : ComponentBase
{
    [Inject]
    public IWorkflowService WorkflowService { get; set; }

    [Parameter]
    public string WorkflowId { get; set; }

    protected WorkflowDto Workflow = new();

    protected IEnumerable<TriggerDto> _triggerOptions;

    protected IEnumerable<ActivityDto> _activityOptions;

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
        if (WorkflowIsValid())
        {
            Workflow = await WorkflowService.SaveAsync(Workflow);
        }
    }

    private bool WorkflowIsValid()
    {
        return true;
    }
}
