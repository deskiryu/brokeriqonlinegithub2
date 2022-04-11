using AutoMapper;
using Microsoft.Extensions.Options;
using ReviewIt.Web.AppSettings;
using ReviewIt.Web.Services.Abstract;
using ReviewIt.Web.Services.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReviewIt.Web.Services
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
