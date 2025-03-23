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
    public class DocumentVaultTypeService : BIQService, IDocumentVaultTypeService
    {
        private const string API_CONTROLLER = "DocumentVaultType";

        public DocumentVaultTypeService(IRequestProviderService requestProviderService, IAccountService accountService)
                : base(accountService, requestProviderService)
        {
        }

        public async Task<IEnumerable<DocumentVaultTypeDto>> GetAllForBroker(int brokerId)
        {
            try
            {
                return await _requestProviderService.Get<IEnumerable<DocumentVaultTypeDto>>($"{API_CONTROLLER}?brokerid={brokerId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get: exception {ex.Message}");
            }

            return Array.Empty<DocumentVaultTypeDto>();
        }

        public async Task<bool> Create(CreateDocumentVaultTypeDto vaultType)
        {
            vaultType.BrokerId = await GetCurrentBrokerId();

            try
            {
                await _requestProviderService.Post<CreateDocumentVaultTypeDto, DocumentVaultTypeDto>(API_CONTROLLER, vaultType);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreateOrCreate: exception {ex.Message}");
            }

            return false;
        }

        public async Task<bool> Update(UpdateDocumentVaultTypeDto vaultType)
        {
            vaultType.BrokerId = await GetCurrentBrokerId();

            try
            {
                await _requestProviderService.Put<UpdateDocumentVaultTypeDto, DocumentVaultTypeDto>(API_CONTROLLER, vaultType);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateOrCreate: exception {ex.Message}");
            }

            return false;
        }

        public async Task<bool> Delete(DocumentVaultTypeDto vaultType)
        {
            try
            {
                return await _requestProviderService.Delete(API_CONTROLLER, vaultType.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete: exception {ex.Message}");
            }

            return false;
        }
    }
}