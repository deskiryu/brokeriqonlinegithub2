using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ReviewIt.Web.Services.Interface
{
    public interface IAddressService
    {
        Task<List<Models.Address>> GetAddressesFromPostcode(string postcode, string countryCode, string customerEmailAddress = "");
    }
}
