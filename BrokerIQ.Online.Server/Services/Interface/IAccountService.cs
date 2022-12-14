namespace BrokerIQ.Online.Services.Interface
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BrokerIQ.Dto.Models;
    using BrokerIQ.Dto.Response;
    using BrokerIQ.Online.Models;
    using BrokerIQ.Online.Models.Account;

    public interface IAccountService
    {
        Task<User> GetUser();
        Task Initialize();
        Task<LoginResponseDto> Login(Login model);
        Task<bool> IsLoggedIn();
        Task Logout();
        Task<bool> RequestNewPassword(string emailAddress);
        Task<BrokerDto> Register(CreateBrokerDto model);
        Task<BrokerStaffDto> RegisterStaff(CreateBrokerStaffDto model);
        Task<IList<User>> GetAll();
        Task<User> GetById(string id);
        Task Update(string id, EditUser model);
        Task Delete(string id);
        Task<bool> ResendEmail(string customerEmail);

        Task<bool> ResendEmailBroker(string brokerEmail);
        Task<LoginResponseDto> LoginTwoFactor(Login model);
    }
}
