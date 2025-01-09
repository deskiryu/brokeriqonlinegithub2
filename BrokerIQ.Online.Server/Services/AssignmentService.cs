using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BrokerIQ.Dto.Dto;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Services.Base;
using BrokerIQ.Online.Server.Services.Interface;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Server.Services
{
    public class AssignmentService : BIQService, IAssignmentService
    {
        private const string _assignmentUrl = "CustomerAssignment";

        private readonly IMapper _mapper;

        public AssignmentService(IAccountService accountService, IRequestProviderService requestProviderService, CookieService cookieService, IMapper mapper)
            : base(accountService, requestProviderService, cookieService)
        {
            _mapper = mapper;
        }

        public async Task<IEnumerable<Customer>> GetForEmployee(int employeeId)
        {
            var answer = await _requestProviderService.Get<IEnumerable<CustomerDto>>($"{_assignmentUrl}?employeeid={employeeId}");
            var mapped = _mapper.Map<IEnumerable<Customer>>(answer);

            return mapped;
        }

        public async Task<IEnumerable<Customer>> Assign(BrokerStaff staff, IEnumerable<int> customerIds)
        {
            var dto = new CustomerAssignmentDto() { EmployeeId = staff.Id, CustomerIds = customerIds };

            var answer = await _requestProviderService.Post<CustomerAssignmentDto, IEnumerable<CustomerDto>>($"{_assignmentUrl}/assign", dto);
            var mapped = _mapper.Map<IEnumerable<Customer>>(answer);

            return mapped;
        }

        public async Task<IEnumerable<Customer>> Unassign(BrokerStaff staff, IEnumerable<int> customerIds)
        {
            var dto = new CustomerAssignmentDto() { EmployeeId = staff.Id, CustomerIds = customerIds };

            var answer = await _requestProviderService.Post<CustomerAssignmentDto, IEnumerable<CustomerDto>>($"{_assignmentUrl}/unassign", dto);
            var mapped = _mapper.Map<IEnumerable<Customer>>(answer);

            return mapped;
        }
    }
}