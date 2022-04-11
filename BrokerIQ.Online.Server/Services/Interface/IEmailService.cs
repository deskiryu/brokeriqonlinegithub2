using System.Threading.Tasks;
using BrokerIQ.Dto.Models;

namespace BrokerIQ.Online.Services.Interface
{
    public interface IEmailService
    {
        Task<bool> SendEmail(CreateEmailDto email);

        Task<bool> SendInviteEmails(CreateEmailDto email);
    }
}