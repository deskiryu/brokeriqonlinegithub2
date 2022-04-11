namespace ReviewIt.Web.Services.Interface
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BrokerIQ.Dto.Models;
    using BrokerIQ.Dto.Response;
    using ReviewIt.Web.Models;
    using ReviewIt.Web.Models.Account;

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
    }
}
