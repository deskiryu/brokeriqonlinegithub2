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

    public CustomerAppointmentService(IRequestProviderService requestProviderService, IMapper mapper)
    {
        this.requestProviderService = requestProviderService;
        this.mapper = mapper;
        this.requestProviderService = requestProviderService;
    }

    public async Task<CalendlyUserDto> GetUser()
    {
        return await this.requestProviderService.Get<CalendlyUserDto>($"{Url}/user");
    }

    public async Task<bool> IsUserConnected()
    {
        return await this.requestProviderService.Get<bool>($"{Url}/isconnected");
    }

    public async Task<CustomerAppointment> Create(CustomerAppointment appointment)
    {
        var mapped = mapper.Map<CreateCustomerAppointmentDto>(appointment);

        var answer = await this.requestProviderService.Post<CreateCustomerAppointmentDto, CustomerAppointmentDto>(this.Url, mapped);
        return this.mapper.Map<CustomerAppointment>(answer);
    }

    public async Task<IEnumerable<CustomerAppointment>> GetByBrokerID(int brokerID)
    {
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
