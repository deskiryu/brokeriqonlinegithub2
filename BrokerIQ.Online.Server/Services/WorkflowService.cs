using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.CreateDto;
using BrokerIQ.Dto.Dto.Workflows;
using BrokerIQ.Online.Server.Services.Base;
using BrokerIQ.Online.Server.Services.Interface;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Server.Services;

public class WorkflowService : BIQService, IWorkflowService
{

    private const string API_CONTROLLER = "Workflow";

    public WorkflowService(IAccountService accountService, IRequestProviderService requestProviderService)
        : base(accountService, requestProviderService)
    {
    }

    public async Task<IEnumerable<TriggerDto>> GetTriggersAsync()
    {
        try
        {
            return await _requestProviderService.Get<IEnumerable<TriggerDto>>($"{API_CONTROLLER}/trigger");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get: exception {ex.Message}");
        }

        return Array.Empty<TriggerDto>();
    }

    public async Task<IEnumerable<ActivityDto>> GetActivitiesAsync()
    {
        try
        {
            return await _requestProviderService.Get<IEnumerable<ActivityDto>>($"{API_CONTROLLER}/activity");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get: exception {ex.Message}");
        }

        return Array.Empty<ActivityDto>();
    }

    public async Task<WorkflowDto> SaveAsync(WorkflowDto workflowDto)
    {
        var brokerId = await GetCurrentBrokerId();

        var createDto = new CreateWorkflowDto()
        {
            BrokerId = brokerId,
            TriggerId = workflowDto.TriggerId,
            Name = workflowDto.Name,
            Steps = workflowDto.Steps,
        };

        return await this._requestProviderService.Post<CreateWorkflowDto, WorkflowDto>($"{API_CONTROLLER}/workflow", createDto);
    }

    public async Task<WorkflowDto> GetWorkflowAsync(string workflowId)
    {
        return await this._requestProviderService.Get<WorkflowDto>($"{API_CONTROLLER}/workflow/{workflowId}");
    }
}
