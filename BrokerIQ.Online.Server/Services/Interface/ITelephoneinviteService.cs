using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;

namespace BrokerIQ.Online.Services.Interface
{
    public interface ITelephoneInviteService
    {
        Task<IEnumerable<TelephoneInvite>> GetTelephoneInvites();

        Task<IEnumerable<TelephoneInvite>> GetTelephoneInvitesByBrokerId(int brokerId);

        Task<bool> AddTelephoneInvites(CreateTelephoneInviteDto createTelephoneInvite);

        Task<bool> DeleteTelephoneInvite(int id);

        Task<bool> UpdateTelephoneInvites(UpdateEmailInviteDto updateTelephoneInviteDto);

        Task<bool> SaveTelephoneNotes(int invitationId, string notes);
    }
}