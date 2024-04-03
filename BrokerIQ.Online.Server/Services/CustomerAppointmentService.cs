using System;
using System.Collections.Generic;
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
        this.requestProviderService = requestProviderService;
    }

    public async Task<CalendlyUserDto> GetUser()
    {
        var user = await this.accountService.GetUser();
        this.requestProviderService.Token = user?.Token;

        return await this.requestProviderService.Get<CalendlyUserDto>($"{Url}/user");
    }

    public async Task<bool> IsUserConnected()
    {
        var user = await this.accountService.GetUser();
        this.requestProviderService.Token = user?.Token;

        return await this.requestProviderService.Get<bool>($"{Url}/isconnected");
    }

    public async Task<CustomerAppointment> Create(CustomerAppointment appointment)
    {
        var user = await this.accountService.GetUser();
        this.requestProviderService.Token = user?.Token;
        var mapped = mapper.Map<CreateCustomerAppointmentDto>(appointment);

        var answer = await this.requestProviderService.Post<CreateCustomerAppointmentDto, CustomerAppointmentDto>(this.Url, mapped);
        return this.mapper.Map<CustomerAppointment>(answer);
    }

    public async Task<IEnumerable<CustomerAppointment>> GetByBrokerID(int brokerID)
    {
        var user = await this.accountService.GetUser();
        this.requestProviderService.Token = user?.Token;

        var response = await this.requestProviderService.Get<IEnumerable<CustomerAppointmentDto>>($"{Url}/broker/{brokerID}");
        var mapped = mapper.Map <IEnumerable<CustomerAppointment>>(response);
        return mapped;
    }

    public async Task<bool> Delete(int id, int brokerId)
    {
        return await DeleteMultiple(new List<int>{ id }, brokerId);
    }

    public async Task<bool> DeleteMultiple(List<int> ids, int brokerId)
    {
        var user = await this.accountService.GetUser();
        this.requestProviderService.Token = user?.Token;
        var url = Url + $"/deleteMultiple/{brokerId}?";
        foreach (var id in ids)
        {
            url += $"ids={id}&";
        }
        url = url.TrimEnd('&');

        try
        {
            return await this.requestProviderService.Delete(url);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Delete: exception {ex.Message}");

            return false;
        }
    }
}
