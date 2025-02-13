using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Services.Interface
{
    using Dto.Models;
    using Models;

    public interface IMortgageService
    {
        Task<Mortgage> GetMortgage(int id, bool eager = true);

        Task<Mortgage> UpdateMortgage(Mortgage ins);

        Task<Mortgage> AddMortgage(Mortgage ins);

        Task<bool> DeleteMortgage(int id);

        Task<Mortgage> AddMortgage(Mortgage ins, List<(string, byte[])> documents);

        Task<MortgageFromDocumentDto> GetFromFile(DocumentDto dto);
    }
}
