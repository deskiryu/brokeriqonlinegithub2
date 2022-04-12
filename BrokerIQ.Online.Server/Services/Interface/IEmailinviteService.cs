using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;

namespace BrokerIQ.Online.Services.Interface
{
    public interface IEmailInviteService
    {
        Task<IEnumerable<EmailInvite>> GetEmailInvites();

        Task<IEnumerable<EmailInvite>> GetEmailInvitesByBrokerId(int brokerId);

        Task<bool> AddEmailInvites(CreateEmailInviteDto createEmailInvite);

        Task<bool> DeleteEmailInvite(int id);

        Task<bool> UpdateEmailInvites(UpdateEmailInviteDto updateEmailInviteDto);
    }
}