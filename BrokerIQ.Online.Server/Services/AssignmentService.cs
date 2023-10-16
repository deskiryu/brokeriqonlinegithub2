using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Services.Base;
using BrokerIQ.Online.Server.Services.Interface;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Server.Services
{
    public class AssignmentService : BrokerIQService, IAssignmentService
    {
        private const string _assignmentUrl = "CustomerAssignment";

        private readonly IMapper _mapper;

        public AssignmentService(IAccountService accountService, IRequestProviderService requestProviderService, IMapper mapper) :
            base(accountService, requestProviderService)
        {
            _mapper = mapper;
        }

        public async Task<IEnumerable<Customer>> GetForEmployee(int employeeId)
        {
            var user = await accountService.GetUser();
            requestProviderService.Token = user?.Token;
            var answer = await requestProviderService.Get<IEnumerable<CustomerDto>>($"{_assignmentUrl}?employeeid={employeeId}");
            var mapped = _mapper.Map<IEnumerable<Customer>>(answer);

            return mapped;
        }

        public async Task<IEnumerable<Customer>> Assign(BrokerStaff staff, IEnumerable<int> customerIds)
        {
            throw new System.NotImplementedException();
        }

        public async Task<IEnumerable<Customer>> Unassign(BrokerStaff staff, IEnumerable<int> customerIds)
        {
            throw new System.NotImplementedException();
        }
    }
}