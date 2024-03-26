using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;

namespace BrokerIQ.Online.Services.Interface;

public interface ICustomerAppointmentService
{
    Task<CalendlyUserDto> GetUser();

    Task<bool> IsUserConnected();

    Task<CustomerAppointment> Create(CustomerAppointment appointment);

    Task<IEnumerable<CustomerAppointmentDto>> GetByBrokerID(int brokerID);

    Task<bool> Delete(int id);
}
