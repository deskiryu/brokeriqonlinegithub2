using System.Collections.Generic;
using System.Linq;
using BrokerIQ.Dto.Dto.Workflows;
using Microsoft.AspNetCore.Components;

namespace BrokerIQ.Online.Server.Pages.Workflow.Components;

public partial class WorkflowDiagram
{

    [Parameter]
    public WorkflowDto Workflow { get; set; } = new();

    [Parameter]
    public IEnumerable<ActivityDto> ActivityOptions { get; set; } = Enumerable.Empty<ActivityDto>();

    // [Parameter]
    // public List<string> ConditionOptions { get; set; } = new();

    [Parameter]
    public IEnumerable<TriggerDto> TriggerOptions { get; set; } = Enumerable.Empty<TriggerDto>();

    // string GetStepLabel(StepModel step) => $"{Workflow.Steps.IndexOf(step) + 1}: {step.ActivityType}";

    protected void AddStep()
    {
        var newStep = new StepDto();
        if (Workflow.Steps.Any())
        {
            var last = Workflow.Steps.Last();

            last.NextStepId = newStep.Id;
        }

        Workflow.Steps.Add(newStep);
    }

    void RemoveStep(StepDto step)
    {
        Workflow.Steps.Remove(step);
        foreach (var s in Workflow.Steps)
        {
            if (s.NextStepId == step.Id)
            {
                s.NextStepId = null;
            }
        }
    }

    // async Task UploadFiles(InputFileChangeEventArgs e, StepModel step, bool isElse = false)
    // {
    //     foreach (var file in e.GetMultipleFiles())
    //     {
    //         var buffer = new byte[file.Size];
    //         await file.OpenReadStream(10_000_000).ReadAsync(buffer);
    //         var attachment = new StepAttachmentModel
    //         {
    //             FileName = file.Name,
    //             ContentType = file.ContentType,
    //             Data = buffer
    //         };
    //         if (isElse)
    //         {
    //             step.ElseAttachments.Add(attachment);
    //         }
    //         else
    //         {
    //             step.Attachments.Add(attachment);
    //         }
    //     }
    // }
    // void RemoveFile(StepModel step, StepAttachmentModel file, bool isElse = false)
    // {
    //     if (isElse)
    //     {
    //         step.ElseAttachments.Remove(file);
    //     }
    //     else
    //     {
    //         step.Attachments.Remove(file);
    //     }
    // }
}
