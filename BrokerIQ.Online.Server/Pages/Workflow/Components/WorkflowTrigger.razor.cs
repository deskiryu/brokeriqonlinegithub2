using System.Collections.Generic;
using System.Linq;
using BrokerIQ.Dto.Dto.Workflows;
using Microsoft.AspNetCore.Components;

namespace BrokerIQ.Online.Server.Pages.Workflow.Components;

public partial class WorkflowTrigger
{
    [Parameter]
    public WorkflowDto Workflow { get; set; } = new();

    [Parameter]
    public IEnumerable<TriggerDto> TriggerOptions { get; set; } = Enumerable.Empty<TriggerDto>();
}
