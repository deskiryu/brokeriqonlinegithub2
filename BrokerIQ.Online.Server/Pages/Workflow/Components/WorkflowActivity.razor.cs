using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Dto.Dto.Workflows;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Workflow.Components;

public partial class WorkflowActivity
{
    [Inject]
    IDialogService DialogService { get; set; }

    [Parameter]
    public StepDto Step { get; set; } = new();

    [Parameter]
    public IEnumerable<ActivityDto> ActivityOptions { get; set; } = Enumerable.Empty<ActivityDto>();

    private IEnumerable<ActivityParameterDto> ActivityParameters
    {
        get
        {
            var selected = ActivityOptions.FirstOrDefault(a => a.Id == Step.ActivityId);
            if (selected is null) return Array.Empty<ActivityParameterDto>();

            return selected.Parameters;
        }
    }

    async Task EditParameters()
    {
        var dialogParams = new DialogParameters
            {
                { "ActivityParameters", ActivityParameters},
                { "StepParameters", Step.StepParameters }
            };

        var dialogResult = await DialogService.Show<ParameterValueDialog>("Set Parameter(s)", dialogParams).Result;

        if (!dialogResult.Canceled)
        {
            Step.StepParameters = dialogResult.Data as StepParameterDto[];
        }
    }
}
