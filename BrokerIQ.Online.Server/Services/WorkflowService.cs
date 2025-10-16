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

        if (workflowDto.Id != Guid.Empty)
        {
            return await _requestProviderService.Put<WorkflowDto, WorkflowDto>($"{API_CONTROLLER}/{workflowDto.Id}", workflowDto);
        }

        var createDto = new CreateWorkflowDto()
        {
            BrokerId = brokerId,
            Name = workflowDto.Name,
            Steps = workflowDto.Steps,
            IsActive = workflowDto.IsActive,
            TriggerKey = workflowDto.TriggerKey,
        };

        return await _requestProviderService.Post<CreateWorkflowDto, WorkflowDto>($"{API_CONTROLLER}", createDto);
    }

    public async Task<IEnumerable<WorkflowDto>> GetWorkflowsAsync()
    {
        return await _requestProviderService.Get<IEnumerable<WorkflowDto>>($"{API_CONTROLLER}");
    }

    public async Task<WorkflowDto> GetWorkflowAsync(string workflowId)
    {
        return await _requestProviderService.Get<WorkflowDto>($"{API_CONTROLLER}/{workflowId}");
    }

    public async Task DeleteWorkflowAsync(Guid workflowId)
    {
        await _requestProviderService.Delete($"{API_CONTROLLER}/{workflowId}");
    }
}
