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
    public class WealthTypeService : BIQService, IWealthTypeService
    {
        private const string API_CONTROLLER = "WealthType";

        public WealthTypeService(IAccountService accountService, IRequestProviderService requestProviderService, CookieService cookieService)
            : base(accountService, requestProviderService, cookieService)
        {
        }

        public async Task<IEnumerable<WealthTypeDto>> GetAllForBroker(int brokerId)
        {
            try
            {
                return await _requestProviderService.Get<IEnumerable<WealthTypeDto>>($"{API_CONTROLLER}?brokerid={brokerId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get: exception {ex.Message}");
            }

            return Array.Empty<WealthTypeDto>();
        }

        public async Task<bool> Create(CreateWealthTypeDto wealthType)
        {
            wealthType.BrokerId = await GetCurrentBrokerId();

            bool response = false;
            try
            {
                response = await _requestProviderService.Post<CreateWealthTypeDto, bool>(API_CONTROLLER, wealthType);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreateOrCreate: exception {ex.Message}");
            }
            return response;
        }

        public async Task<bool> Update(UpdateWealthTypeDto wealthType)
        {
            wealthType.BrokerId = await GetCurrentBrokerId();

            bool response = false;
            try
            {
                response = await _requestProviderService.Put<UpdateWealthTypeDto, bool>(API_CONTROLLER, wealthType);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateOrCreate: exception {ex.Message}");
            }
            return response;
        }

        public async Task<bool> Delete(WealthTypeDto wealthType)
        {
            try
            {
                return await _requestProviderService.Delete(API_CONTROLLER, wealthType.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete: exception {ex.Message}");
            }

            return false;
        }
    }
}