using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Dto.Workflows;

namespace BrokerIQ.Online.Server.Services.Interface;

public interface IWorkflowService
{
    Task<IEnumerable<TriggerDto>> GetTriggersAsync();

    Task<IEnumerable<ActivityDto>> GetActivitiesAsync();

    Task<WorkflowDto> SaveAsync(WorkflowDto dto);

    Task<WorkflowDto> GetWorkflowAsync(string workflowId);
}
