using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BrokerIQ.Dto.Dto;
using BrokerIQ.Dto.Enum;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Server.Models;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services
{
    public class BrokerReminderOptionService : IBrokerReminderOptionService
    {
        private const string API_CONTROLLER = "BrokerReminderOption";

        private readonly IMapper mapper;
        private readonly IRequestProviderService requestProviderService;
        private readonly IAccountService accountService;

        public BrokerReminderOptionService(IMapper mapper, IRequestProviderService requestProviderService, IAccountService accountService)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
            this.accountService = accountService;
        }

        public async Task<IEnumerable<ReminderOptionDto>> GetAllForCurrentBroker()
        {
            var brokerId = await GetCurrentBrokerId();
            try
            {
                var messagesDto = await requestProviderService.Get<BrokerReminderOptionDto>($"{API_CONTROLLER}/{brokerId}");
                var options = mapper.Map<BrokerReminderOption>(messagesDto);
                return BuildAllOptionsFrom(brokerId, options.BrokerReminderOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get: exception {ex.Message}");
            }

            return Array.Empty<ReminderOptionDto>();
        }

        private async Task<int> GetCurrentBrokerId()
        {
            var user = await accountService.GetUser();
            requestProviderService.Token = user?.Token;

            return user.MasterBrokerId;
        }

        private static IEnumerable<ReminderOptionDto> BuildAllOptionsFrom(int brokerId, IEnumerable<ReminderOptionDto> options)
        {
            var allOptions = new List<ReminderOptionDto>();

            foreach (var target in Enum.GetValues(typeof(ReminderTargetEnum)))
            {
                foreach (var type in Enum.GetValues(typeof(ReminderTypeEnum)))
                {
                    var option = options.FirstOrDefault(x => x.ReminderTargetId == (int)target && x.ReminderTypeId == (int)type);

                    if (option != null)
                    {
                        allOptions.Add(option);
                        continue;
                    }

                    allOptions.Add(new ReminderOptionDto()
                    {
                        ReminderTargetId = (int)target,
                        ReminderTypeId = (int)type
                    });
                }
            }

            return allOptions;
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
                response = await requestProviderService.Post<CreateBrokerReminderOptionDto, bool>(API_CONTROLLER, brokerReminderOption);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateOrCreate: exception {ex.Message}");
            }
            return response;
        }
    }
}