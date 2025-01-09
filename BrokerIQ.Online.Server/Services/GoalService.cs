using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using BrokerIQ.Dto.Models;
using BrokerIQ.Dto.UpdateDto;
using BrokerIQ.Online.Server.Services.Base;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services
{
    public class GoalService : BIQService, IGoalService
    {
        private const string API_CONTROLLER = "Goal";

        public GoalService(IAccountService accountService, IRequestProviderService requestProviderService, CookieService cookieService)
            : base(accountService, requestProviderService, cookieService)
        {
        }

        public async Task<IEnumerable<GoalDto>> GetAllForBroker(int brokerId)
        {
            try
            {
                return await _requestProviderService.Get<IEnumerable<GoalDto>>($"{API_CONTROLLER}?brokerid={brokerId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get: exception {ex.Message}");
            }

            return Array.Empty<GoalDto>();
        }

        public async Task<bool> Create(CreateGoalDto Goal)
        {
            Goal.BrokerId = await GetCurrentBrokerId();

            bool response = false;
            try
            {
                response = await _requestProviderService.Post<CreateGoalDto, bool>(API_CONTROLLER, Goal);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Create: exception {ex.Message}");
            }
            return response;
        }

        public async Task<bool> Update(UpdateGoalDto Goal)
        {
            Goal.BrokerId = await GetCurrentBrokerId();

            bool response = false;
            try
            {
                response = await _requestProviderService.Put<UpdateGoalDto, bool>(API_CONTROLLER, Goal);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update: exception {ex.Message}");
            }
            return response;
        }

        public async Task<bool> Delete(GoalDto Goal)
        {
            try
            {
                return await _requestProviderService.Delete(API_CONTROLLER, Goal.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete: exception {ex.Message}");
            }

            return false;
        }
    }
}