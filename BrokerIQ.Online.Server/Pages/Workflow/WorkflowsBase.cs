using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Dto.Dto.Workflows;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Components;
using BrokerIQ.Online.Server.Services.Interface;
using BrokerIQ.Online.Services.Interface;
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
    public IAccountService AccountService { get; set; }

    [Inject]
    public IBrokerService BrokerService { get; set; }

    [Inject]
    public IWorkflowService WorkflowService { get; set; }

    [Parameter]
    public User User { get; set; }

    public int CurrentBrokerId { get; set; }

    public IEnumerable<Online.Models.Broker> Brokers { get; set; }

    public Online.Models.Broker Broker { get; set; }

    protected IEnumerable<WorkflowDto> BrokerWorkflows { get; set; }

    protected IEnumerable<TriggerDto> Triggers { get; set; }

    protected string NewWorkflowUrl => $"/workflow/new/{CurrentBrokerId}";

    protected override async Task OnInitializedAsync()
    {
        User = await AccountService.GetUser();
        CurrentBrokerId = User.MasterBrokerId;

        Broker = await BrokerService.GetBroker(User.MasterBrokerId);

        Triggers = (await WorkflowService.GetTriggersAsync()).ToArray();

        if (User.IsAdmin || User.IsMinorAdmin)
        {
            Brokers = await BrokerService.GetBrokers();
        }
        else
        {
            Broker = await BrokerService.GetBroker(CurrentBrokerId, true);

            BrokerWorkflows = await WorkflowService.GetWorkflowsAsync(CurrentBrokerId);
        }
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

            BrokerWorkflows = await WorkflowService.GetWorkflowsAsync(Broker.Id);
        }
    }

    protected string GetTriggerName(WorkflowDto wf)
    {
        if (!Triggers.Any() || !wf.Steps.Any()) return string.Empty;

        return Triggers.First(t => t.Key == wf.TriggerKey).Name;
    }

    public async Task OnBrokerChanged(int brokerId)
    {
        CurrentBrokerId = brokerId;

        Broker = brokerId == 0 ? null : await BrokerService.GetBroker(CurrentBrokerId, true);

        BrokerWorkflows = await WorkflowService.GetWorkflowsAsync(CurrentBrokerId);
    }
}