using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using AutoMapper;

using BrokerIQ.Dto.Dto.Workflows;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Services.Interface;

using MudBlazor;
using System.Collections.Generic;

namespace BrokerIQ.Online.Server.Pages.Workflow.Components;

public partial class ParameterValueDialog : ComponentBase
{
    [Inject]
    public IMapper Mapper { get; set; }

    [Inject]
    public IBrokerService BrokerService { get; set; }

    [Inject]
    public IBrokerDefinedMessageService DefinedMessageService { get; set; }

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

    private Online.Models.Broker Broker { get; set; }

    private Online.Models.Customer SampleCustomer { get; set; }

    private Online.Models.Customer SampleConnection { get; set; }

    private BrokerDefinedMessage Template { get; set; }

    public string Message { get; set; }

    //private IList<IBrowserFile> _files = new List<IBrowserFile>();

    protected override async Task OnInitializedAsync()
    {
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

    // private void UploadFiles(IBrowserFile file)
    // {
    //     _files.Add(file);
    //     //TODO upload the files to the server
    // }

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
}