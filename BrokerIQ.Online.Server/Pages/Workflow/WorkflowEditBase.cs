using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Dto.Workflows;
using BrokerIQ.Online.Server.Services.Interface;
using Microsoft.AspNetCore.Components;
using Microsoft.VisualBasic;

namespace BrokerIQ.Online.Server.Pages.Workflow;

public class WorkflowEditBase : ComponentBase
{
    [Inject]
    public IWorkflowService WorkflowService { get; set; }

    protected WorkflowDto Workflow = new();

    protected string? _json;

    protected IEnumerable<TriggerDto> _triggerOptions;

    protected IEnumerable<ActivityDto> _activityOptions;
    //private readonly List<string> _conditionOptions = new() { "NoCondition", "PolicyActive", "LoanApproved" };

    protected override async Task OnInitializedAsync()
    {
        _triggerOptions = await WorkflowService.GetTriggersAsync();
        _activityOptions = await WorkflowService.GetActivitiesAsync();
    }

    // void Generate()
    // {
    //     _json = WorkflowService.GenerateJson(_workflow);
    // }

    protected async Task Save()
    {
        Workflow = await WorkflowService.Save(Workflow);
    }

    // void Load()
    // {
    //     var loaded = WorkflowService.Load(_workflow.Name);
    //     if (loaded is not null)
    //     {
    //         _workflow = loaded;
    //     }
    // }
}
