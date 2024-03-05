using System.Threading.Tasks;

using AutoMapper;

using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services;

public class CustomerAppointmentService : ICustomerAppointmentService
{
    private readonly string Url = "CustomerAppointment";
    private readonly IRequestProviderService requestProviderService;
    private readonly IMapper mapper;
    private readonly IAccountService accountService;

    public CustomerAppointmentService(IRequestProviderService requestProviderService, IMapper mapper, IAccountService accountService)
    {
        this.requestProviderService = requestProviderService;
        this.mapper = mapper;
        this.accountService = accountService;
        this.accountService = accountService;
        this.requestProviderService = requestProviderService;
    }

    public async Task<CustomerAppointment> Create(CustomerAppointment appointment)
    {
        var user = await this.accountService.GetUser();
        this.requestProviderService.Token = user?.Token;
        var mapped = mapper.Map<CreateCustomerAppointmentDto>(appointment);

        var answer = await this.requestProviderService.Post<CreateCustomerAppointmentDto, CustomerAppointmentDto>(this.Url, mapped);
        return this.mapper.Map<CustomerAppointment>(answer);
    }
}
