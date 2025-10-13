using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Dto.Dto.Workflows;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Components;
using BrokerIQ.Online.Server.Services.Interface;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Workflow;

public class WorkflowsBase : ComponentBase
{
    [Inject]
    public ISnackbar Snackbar { get; set; }

    [Inject]
    private IDialogService DialogService { get; set; }

    [Inject]
    public IWorkflowService WorkflowService { get; set; }

    [Parameter]
    public User User { get; set; }

    [Parameter]
    public Online.Models.Broker Broker { get; set; }

    protected IEnumerable<WorkflowDto> BrokerWorkflows { get; set; }

    protected IEnumerable<ActivityDto> Triggers { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Triggers = (await WorkflowService.GetActivitiesAsync()).Where(a => a.IsEventActivity).ToArray();
        BrokerWorkflows = await WorkflowService.GetWorkflowsAsync();
    }

    protected async Task RemoveWorkflow(WorkflowDto wf)
    {
        var parameters = new DialogParameters
        {
            { "ContentText", "Do you really want to delete this workflow? This process cannot be undone." },
            { "ButtonText", "Delete" },
            { "Color", Color.Error }
        };

        var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };

        var result = await DialogService.Show<ConfirmationDialog>("Delete", parameters, dialogOptions).Result;

        if (!result.Canceled)
        {
            await WorkflowService.DeleteWorkflowAsync(wf.Id);

            Snackbar.Add("Workflow was deleted", Severity.Success);

            BrokerWorkflows = await WorkflowService.GetWorkflowsAsync();
        }
    }

    protected string GetTriggerName(WorkflowDto wf)
    {
        if (!Triggers.Any() || !wf.Steps.Any()) return string.Empty;

        return Triggers.First(t => t.Id == wf.Steps.First().ActivityId).Name;
    }
}