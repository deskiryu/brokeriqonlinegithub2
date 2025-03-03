using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;
using BrokerIQ.Dto.Dto;
using BrokerIQ.Dto.Enum;
using BrokerIQ.Online.Server.Services.Base;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services
{
    public class CustomerWarningService : BIQService, ICustomerWarningService
    {
        private readonly string CustomerWarningUrl = "CustomerWarning";
        private readonly IMapper mapper;

        public CustomerWarningService(IRequestProviderService requestProviderService, IMapper mapper, IAccountService accountService)
            : base(accountService, requestProviderService)

        {
            this.mapper = mapper;
        }

        public async Task<IEnumerable<CustomerWarningDto>> GetForCustomer(int id)
        {
            var answer = await this._requestProviderService.Get<IEnumerable<CustomerWarningDto>>($"{this.CustomerWarningUrl}/customer/{id}");
            return this.mapper.Map<IEnumerable<CustomerWarningDto>>(answer);
        }
    }
}
