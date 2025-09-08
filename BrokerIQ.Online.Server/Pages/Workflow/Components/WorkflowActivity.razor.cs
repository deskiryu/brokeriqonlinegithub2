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

    [Parameter] public EventCallback OnChanged { get; set; }
    private Task HasChanged() => OnChanged.InvokeAsync();

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
                { "StepParameters", BuildCurrentParameters() }
            };

        var options = new DialogOptions() { MaxWidth = MaxWidth.Medium, FullWidth = true };

        var dialogResult = await DialogService.Show<ParameterValueDialog>("Set Parameter(s)", dialogParams, options).Result;

        if (!dialogResult.Canceled)
        {
            Step.StepParameters = dialogResult.Data as StepParameterDto[];

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
