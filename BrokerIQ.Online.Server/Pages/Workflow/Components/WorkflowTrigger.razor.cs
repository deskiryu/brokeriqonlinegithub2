using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Dto.Dto.Workflows;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Workflow.Components;

public partial class WorkflowTrigger
{
    [Parameter]
    public WorkflowDto Workflow { get; set; }

    [Parameter]
    public IEnumerable<TriggerDto> Triggers { get; set; } = Enumerable.Empty<TriggerDto>();

    [Parameter] public EventCallback OnChanged { get; set; }
    private Task HasChanged() => OnChanged.InvokeAsync();

    private MudMenu triggerMenu;

    private string SelectedTriggerKey
    {
        get => Workflow.TriggerKey;
        set
        {
            Workflow.TriggerKey = value;

            HasChanged();
        }
    }

    private string TriggerLabel
    {
        get
        {
            var trigger = Triggers.FirstOrDefault(a => a.Key == SelectedTriggerKey);

            if (trigger == null) return string.Empty;

            return "Trigger";
        }
    }

    public TriggerDto SelectedTrigger => string.IsNullOrWhiteSpace(Workflow.TriggerKey) ? null : Triggers.FirstOrDefault(t => t.Key == Workflow.TriggerKey);

    protected void SetTrigger(string triggerKey)
    {
        SelectedTriggerKey = triggerKey;

        triggerMenu.CloseMenu();
    }
}
