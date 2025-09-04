using System.Collections.Generic;
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

    protected IEnumerable<TriggerDto> _triggerOptions;

    protected IEnumerable<WorkflowDto> BrokerWorkflows { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _triggerOptions = await WorkflowService.GetTriggersAsync();

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
}