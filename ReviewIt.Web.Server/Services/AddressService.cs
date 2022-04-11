using AutoMapper;
using Microsoft.Extensions.Options;
using BrokerIQ.Online.AppSettings;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Services
{
    public class AddressService : IAddressService
    {
        private readonly IRequestProviderService requestProviderService;
        private readonly IMapper mapper;
        private readonly ReviewItAPIDetails api;

        public AddressService(IRequestProviderService requestProviderService, IMapper mapper, IOptions<ReviewItAPIDetails> api)
        {
            this.requestProviderService = requestProviderService;
            this.mapper = mapper;
            this.api = api.Value;
        }

        public async Task<List<Models.Address>> GetAddressesFromPostcode(string postcode, string countryCode, string customerEmailAddress="")
        {
            string newUrl = $"broker/postcode/web/{countryCode}/{postcode}?WS={api.WS}";
            var returned = await requestProviderService.Get<List<Models.Address>>(newUrl);
            return returned;

        }
    }
}
