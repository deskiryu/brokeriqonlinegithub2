using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using BrokerIQ.Dto.Models;
using BrokerIQ.Dto.Response;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Models.Account;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Services
{
    public class AccountService : IAccountService
    {
        private IRequestProviderService _requestProviderService;
        private ILocalStorageService _localStorageService;
        private readonly CookieService _cookieService;
        private readonly IMapper _mapper;
        private string _userKey = "user";

        private string _audioRecordingKey = "audioRecording";

        private User _user;

        public AccountService(
            IRequestProviderService httpService,
            ILocalStorageService localStorageService,
            CookieService cookieService,
            IMapper mapper
        )
        {
            _requestProviderService = httpService;
            _localStorageService = localStorageService;
            _cookieService = cookieService;
            _mapper = mapper;
        }

        public async Task<User> GetUser()
        {
            _user = await _localStorageService.GetItem<User>(_userKey);
            return _user;
        }

        public async Task<string> GetAudioRecordingAsbase64()
        {
            return (await _localStorageService.GetItem<string>(_audioRecordingKey));
        }

        public async Task Initialize()
        {
            _user = await _localStorageService.GetItem<User>(_userKey);
        }

        public async Task<LoginResponseDto> Login(Login model)
        {
            var response = await _requestProviderService.FirstFactorPost("Auth/SignIn", model);

            _user = _mapper.Map<User>(response);
            await _localStorageService.SetItem(_userKey, _user);

            return response;
        }

        public async Task<LoginResponseDto> LoginTwoFactor(Login model)
        {
            var response = await _requestProviderService.SecondFactorPost("Auth/SignInTwofactor", model);

            _user = _mapper.Map<User>(response);
            await _localStorageService.SetItem(_userKey, _user);

            return response;
        }

        public async Task<bool> IsLoggedIn()
        {
            var token = await _cookieService.GetCookieAsync(CookieService.ACCESS_TOKEN_KEY);
            var expirationValue = await _cookieService.GetCookieAsync(CookieService.ACCESS_EXPIRATION_KEY);
            var wasParsed = DateTime.TryParse(WebUtility.UrlDecode(expirationValue), out DateTime expiration);

            return wasParsed && expiration > DateTime.UtcNow && !string.IsNullOrWhiteSpace(token);
        }

        public async Task<bool> RequestNewPassword(string emailAddress)
        {
            string newUrl = "Auth/RequestNewPassword";
            newUrl += $"?emailaddress={emailAddress}";

            var resetPassword = await _requestProviderService.Post<PasswordDto>(newUrl);
            return resetPassword.PasswordChanged;
        }

        public async Task Logout()
        {
            var token = await _cookieService.GetCookieAsync(CookieService.REFRESH_TOKEN_KEY);

            await _requestProviderService.Delete($"auth/revoke?token={WebUtility.UrlEncode(token)}");

            await _cookieService.DeleteCookieAsync(CookieService.ACCESS_TOKEN_KEY);
            await _cookieService.DeleteCookieAsync(CookieService.ACCESS_EXPIRATION_KEY);
            await _cookieService.DeleteCookieAsync(CookieService.REFRESH_TOKEN_KEY);

            _user = null;
            await _localStorageService.SetItem(_userKey, new User());
        }

        public async Task<BrokerDto> Register(CreateBrokerDto model)
        {
            return await _requestProviderService.Post<CreateBrokerDto, BrokerDto>("BrokerAuth/signup", model);
        }

        public async Task<BrokerStaffDto> RegisterStaff(CreateBrokerStaffDto model)
        {
            return await _requestProviderService.Post<CreateBrokerStaffDto, BrokerStaffDto>("BrokerAuth/signupstaff", model);
        }

        public async Task<bool> ToggleTwoFactor(string emailAddress, bool enable)
        {
            return await _requestProviderService.Post<bool>($"BrokerAuth/twofactorenable?emailaddress={emailAddress}&enable={enable}");
        }

        public async Task<IList<User>> GetAll()
        {
            return await _requestProviderService.Get<IList<User>>("/users");
        }

        public async Task<User> GetById(string id)
        {
            return await _requestProviderService.Get<User>($"/users/{id}");
        }

        public async Task Update(string id, EditUser model)
        {
            await _requestProviderService.Put<EditUser, bool>($"/users/{id}", model);

            // update stored user if the logged in user updated their own record
            if (id == _user.Id)
            {
                // update local storage
                _user.FirstName = model.FirstName;
                _user.LastName = model.LastName;
                _user.Username = model.Username;
                await _localStorageService.SetItem(_userKey, _user);
            }
        }

        public async Task Delete(string id)
        {
            await _requestProviderService.Delete($"/users/{id}");

            // auto logout if the logged in user deleted their own record
            if (id == _user.Id)
                await Logout();
        }

        public async Task<bool> ResendEmail(string customerEmail)
        {
            return await _requestProviderService.Post<bool>($"Auth/resend?EmailAddress={customerEmail}");
        }

        public async Task<bool> ResendEmailBroker(string brokerEmail)
        {
            return await _requestProviderService.Post<bool>($"BrokerAuth/resend?EmailAddress={brokerEmail}");
        }

    }
}