using System.Threading.Tasks;

using BrokerIQ.Online.Models;

namespace BrokerIQ.Online.Services.Interface;

public interface ICustomerAppointmentService
{
    Task<CustomerAppointment> Create(CustomerAppointment appointment);
}
