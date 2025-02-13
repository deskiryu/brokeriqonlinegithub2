using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BrokerIQ.Dto.Dto;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Server.Models;
using BrokerIQ.Online.Server.Services.Base;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services
{
    public class BrokerReminderOptionService : BIQService,  IBrokerReminderOptionService
    {
        private const string API_CONTROLLER = "BrokerReminderOption";

        private readonly IMapper mapper;

        public BrokerReminderOptionService(IMapper mapper, IRequestProviderService requestProviderService, IAccountService accountService)
            :base(accountService, requestProviderService)
        {
            this.mapper = mapper;
        }

        public async Task<IEnumerable<ReminderOptionDto>> GetAllForCurrentBroker()
        {
            var brokerId = await GetCurrentBrokerId();
            try
            {
                var messagesDto = await _requestProviderService.Get<BrokerReminderOptionDto>($"{API_CONTROLLER}/{brokerId}");
                var options = mapper.Map<BrokerReminderOption>(messagesDto);

                return options.BrokerReminderOptions = options.BrokerReminderOptions.OrderBy(o => o.ReminderTypeId)
                                                                                    .ThenByDescending(o => o.NotificationPeriod)
                                                                                    .ThenBy(o => o.ReminderTargetId)
                                                                                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get: exception {ex.Message}");
            }

            return Array.Empty<ReminderOptionDto>();
        }

        public async Task<bool> UpdateOrCreate(IEnumerable<ReminderOptionDto> reminderOptions)
        {
            var brokerId = await GetCurrentBrokerId();

            CreateBrokerReminderOptionDto brokerReminderOption = new CreateBrokerReminderOptionDto
            {
                BrokerId = brokerId,
                BrokerReminderOptions = reminderOptions.ToList()
            };

            bool response = false;
            try
            {
                response = await _requestProviderService.Post<CreateBrokerReminderOptionDto, bool>(API_CONTROLLER, brokerReminderOption);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateOrCreate: exception {ex.Message}");
            }
            return response;
        }

        public async Task<bool> Delete(ReminderOptionDto reminderOption)
        {
            try
            {
                return await _requestProviderService.Delete(API_CONTROLLER, reminderOption.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete: exception {ex.Message}");
            }

            return false;
        }
    }
}