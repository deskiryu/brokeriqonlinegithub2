using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;

using BrokerIQ.Dto.Models;
using BrokerIQ.Dto.UpdateDto;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services
{
    public class DocumentVaultTypeService : IDocumentVaultTypeService
    {
        private const string API_CONTROLLER = "DocumentVaultType";

        private readonly IMapper mapper;
        private readonly IRequestProviderService requestProviderService;
        private readonly IAccountService accountService;

        public DocumentVaultTypeService(IMapper mapper, IRequestProviderService requestProviderService, IAccountService accountService)
        {
            this.mapper = mapper;
            this.requestProviderService = requestProviderService;
            this.accountService = accountService;
        }

        public async Task<IEnumerable<DocumentVaultTypeDto>> GetAllForCurrentBroker()
        {
            var brokerId = await GetCurrentBrokerId();
            try
            {
                return await requestProviderService.Get<IEnumerable<DocumentVaultTypeDto>>($"{API_CONTROLLER}?brokerid={brokerId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get: exception {ex.Message}");
            }

            return Array.Empty<DocumentVaultTypeDto>();
        }

        private async Task<int> GetCurrentBrokerId()
        {
            var user = await accountService.GetUser();
            requestProviderService.Token = user?.Token;

            return user.MasterBrokerId;
        }

        public async Task<bool> Create(CreateDocumentVaultTypeDto vaultType)
        {
            vaultType.BrokerId = await GetCurrentBrokerId();

            bool response = false;
            try
            {
                response = await requestProviderService.Post<CreateDocumentVaultTypeDto, bool>(API_CONTROLLER, vaultType);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreateOrCreate: exception {ex.Message}");
            }
            return response;
        }

        public async Task<bool> Update(UpdateDocumentVaultTypeDto vaultType)
        {
            vaultType.BrokerId = await GetCurrentBrokerId();

            bool response = false;
            try
            {
                response = await requestProviderService.Put<UpdateDocumentVaultTypeDto, bool>(API_CONTROLLER, vaultType);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateOrCreate: exception {ex.Message}");
            }
            return response;
        }

        public async Task<bool> Delete(DocumentVaultTypeDto vaultType)
        {
            try
            {
                return await requestProviderService.Delete(API_CONTROLLER, vaultType.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete: exception {ex.Message}");
            }

            return false;
        }
    }
}