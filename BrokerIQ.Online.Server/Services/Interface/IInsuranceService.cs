using System.Collections.Generic;
using System.Threading.Tasks;

using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;

namespace BrokerIQ.Online.Services.Interface
{
    public interface IInsuranceService
    {
        Task<Insurance> GetInsurance(int id, bool eager = true);

        Task<Insurance> UpdateInsurance(Insurance ins);

        Task<Insurance> AddInsurance(Insurance ins);

        Task<bool> DeleteInsurance(int id);

        Task<Insurance> AddInsurance(Insurance ins, List<(string, byte[])> documents);

        Task<InsuranceFromDocumentDto> GetFromFile(DocumentDto dto);
    }
}
