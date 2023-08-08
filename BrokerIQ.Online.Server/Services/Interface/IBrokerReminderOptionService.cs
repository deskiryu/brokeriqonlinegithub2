using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Dto;

namespace BrokerIQ.Online.Services.Interface
{
    public interface IBrokerReminderOptionService
    {
        Task<IEnumerable<ReminderOptionDto>> GetAllForCurrentBroker();

        Task<bool> UpdateOrCreate(IEnumerable<ReminderOptionDto> definedMessages);

        Task<bool> Delete(ReminderOptionDto toDelete);
    }
}