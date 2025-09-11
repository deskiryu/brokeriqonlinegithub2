using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Server.Services.Base;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services
{
    public class BrokerConsentDocumentService : BIQService, IBrokerConsentDocumentService
    {
        private const string API_CONTROLLER = "ConsentDocument";

        private readonly IMapper mapper;

        public BrokerConsentDocumentService(IAccountService accountService, IRequestProviderService requestProviderService, IMapper mapper)
            : base(accountService, requestProviderService)
        {
            this.mapper = mapper;
        }

        public async Task<IEnumerable<BrokerConsentDocumentDto>> GetAllForCurrentBroker()
        {
            var brokerId = await GetCurrentBrokerId();

            try
            {
                var docs = await _requestProviderService.Get<IEnumerable<BrokerConsentDocumentDto>>($"{API_CONTROLLER}/{brokerId}");

                return mapper.Map<IEnumerable<BrokerConsentDocumentDto>>(docs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get: exception {ex.Message}");
            }

            return Array.Empty<BrokerConsentDocumentDto>();
        }

        public async Task<IEnumerable<BrokerConsentDocumentDto>> GetForBroker(int brokerId)
        {
            try
            {
                var docs = await _requestProviderService.Get<IEnumerable<BrokerConsentDocumentDto>>($"{API_CONTROLLER}/{brokerId}");

                return mapper.Map<IEnumerable<BrokerConsentDocumentDto>>(docs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get: exception {ex.Message}");
            }

            return Array.Empty<BrokerConsentDocumentDto>();
        }

        public async Task<bool> Create(CreateBrokerConsentDocumentDto document)
        {
            try
            {
                // Ensure broker id is set if the DTO supports it
                try
                {
                    var brokerId = await GetCurrentBrokerId();
                    var prop = document?.GetType().GetProperty("BrokerId");
                    prop?.SetValue(document, brokerId);
                }
                catch { }

                var response = await _requestProviderService.Post<CreateBrokerConsentDocumentDto, BrokerConsentDocumentDto>(API_CONTROLLER, document);
                return response.Id > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Create: exception {ex.Message}");
                return false;
            }
        }

        public async Task<bool> Update(BrokerConsentDocumentDto document)
        {
            try
            {
                var response = await _requestProviderService.Put<BrokerConsentDocumentDto, BrokerConsentDocumentDto>(API_CONTROLLER, document);
                return response.Id > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update: exception {ex.Message}");
                return false;
            }
        }

        public async Task<bool> Delete(BrokerConsentDocumentDto document)
        {
            try
            {
                return await _requestProviderService.Delete($"{API_CONTROLLER}/{document.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete: exception {ex.Message}");
                return false;
            }
        }
    }
}
