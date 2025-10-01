using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using AutoMapper;

using BrokerIQ.Dto.Dto.Workflows;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Services.Interface;

using MudBlazor;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components.Forms;
using System.IO;
using Microsoft.Extensions.Options;
using BrokerIQ.Online.Server.AppSettings;
using BrokerIQ.Online.Server.Shared;

namespace BrokerIQ.Online.Server.Pages.Workflow.Components;

public partial class ParameterValueDialog : ComponentBase
{
    [Inject]
    public IMapper Mapper { get; set; }

    [Inject]
    public IBrokerService BrokerService { get; set; }

    [Inject]
    public IBrokerDefinedMessageService DefinedMessageService { get; set; }

    [Inject]
    public IOptions<FileUploadSettings> FileUploadSettingsOption { get; set; }

    [Inject]
    public IDialogService DialogService { get; set; }

    [CascadingParameter]
    MudDialogInstance MudDialog { get; set; }

    [Parameter]
    public ActivityDto Activity { get; set; }

    [Parameter]
    public StepDto Step { get; set; }

    [Parameter]
    public User User { get; set; }

    [Parameter]
    public IEnumerable<Models.Video> BrokerVideos { get; set; }

    private FileUploadSettings fileUploadSettings { get; set; }

    private Online.Models.Broker Broker { get; set; }

    private Online.Models.Customer SampleCustomer { get; set; }

    private Online.Models.Customer SampleConnection { get; set; }

    public string Message { get; set; }

    private IBrowserFile _attachment;

    protected override async Task OnInitializedAsync()
    {
        fileUploadSettings = this.FileUploadSettingsOption.Value;

        var template = Activity.Parameters.FirstOrDefault(p => p.Type == "template");
        if (template is not null)
        {
            Message = Step.StepParameters.First(p => p.Order == template.Order).Value;
        }

        SampleCustomer = new Online.Models.Customer()
        {
            FirstName = "John",
            LastName = "Smith"
        };

        SampleConnection = new Online.Models.Customer()
        {
            FirstName = "Jane",
            LastName = "Smith"
        };

        Broker = await BrokerService.GetBroker(User.MasterBrokerId);
    }

    private async Task UploadFile(IBrowserFile file)
    {
        if (file.Size > this.fileUploadSettings.MaxFileSize)
        {
            var dialogParams = new DialogParameters
            {
                { "Message", $"File is too large, limit is {fileUploadSettings.MaxFileSize / 1024}MB" }
            };
            await DialogService.Show<AlertDialog>("File size ", dialogParams).Result;

            return;
        }

        _attachment = file;

        Step.AttachmentDto = new StepAttachmentDto()
        {
            FileName = _attachment.Name,
            ContentType = _attachment.ContentType
        };

        using var stream = file.OpenReadStream(file.Size);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        Step.AttachmentDto.Data = memoryStream.ToArray();
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private void Confirm()
    {
        var template = Activity.Parameters.FirstOrDefault(p => p.Type == "template");
        if (template is not null && !string.IsNullOrWhiteSpace(Message))
        {
            Step.StepParameters.First(p => p.Order == template.Order).Value = Message;
        }

        MudDialog.Close(Step);
    }

    private void OnTemplateChange(BrokerDefinedMessage template)
    {
        if (template.File == null) return;

        Step.AttachmentDto = new StepAttachmentDto()
        {
            FileName = template.FileName,
            ContentType = "application/pdf",
            Data = template.File
        };
    }

    private void RemoveAttachment(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
    {
        _attachment = null;
        Step.AttachmentDto = null;
    }
}