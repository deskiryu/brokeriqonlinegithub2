using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using AutoMapper;

using BrokerIQ.Dto.Dto.Workflows;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Services.Interface;

using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Workflow.Components;

public partial class ParameterValueDialog : ComponentBase
{
    [Inject]
    public IMapper Mapper { get; set; }

    [Inject]
    public IAccountService AccountService { get; set; }

    [Inject]
    public IBrokerService BrokerService { get; set; }

    [Inject]
    public IBrokerDefinedMessageService DefinedMessageService { get; set; }

    [CascadingParameter]
    MudDialogInstance MudDialog { get; set; }

    [Parameter]
    public IEnumerable<ActivityParameterDto> ActivityParameters { get; set; }

    [Parameter]
    public ICollection<StepParameterDto> StepParameters { get; set; }

    public Online.Models.Broker Broker { get; set; }

    public Online.Models.Customer SampleCustomer { get; set; }

    public Online.Models.Customer SampleConnection { get; set; }

    public BrokerDefinedMessage Template { get; set; }

    public string Message { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var template = ActivityParameters.FirstOrDefault(p => p.Type == "template");
        if (template is not null)
        {
            Message = StepParameters.First(p => p.Order == template.Order).Value;
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

        var user = await AccountService.GetUser();
        Broker = await BrokerService.GetBroker(user.MasterBrokerId);
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private void Confirm()
    {
        var template = ActivityParameters.FirstOrDefault(p => p.Type == "template");
        if (template is not null && !string.IsNullOrWhiteSpace(Message))
        {
            StepParameters.First(p => p.Order == template.Order).Value = Message;
        }

        MudDialog.Close(StepParameters);
    }
}