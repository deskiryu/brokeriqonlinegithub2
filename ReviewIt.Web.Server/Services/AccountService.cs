using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReviewIt.Web.Services
{
    using System;
    using AutoMapper;
    using BrokerIQ.Dto.Models;
    using BrokerIQ.Dto.Response;
    using ReviewIt.Web.Models;
    using ReviewIt.Web.Models.Account;
    using ReviewIt.Web.Server.Helper;
    using ReviewIt.Web.Services.Interface;
    using ReviewIt.Web.Services.Abstract;

    public class AccountService : IAccountService
    {
        private IRequestProviderService _requestProviderService;
        private NavigationManager _navigationManager;
        private ILocalStorageService _localStorageService;
        private readonly IMapper _mapper;
        private string _userKey = "user";

        private string _audioRecordingKey = "audioRecording";

        private User _user;

        public AccountService(
            IRequestProviderService httpService,
            NavigationManager navigationManager,
            ILocalStorageService localStorageService,
            IMapper mapper
        ) {
            _requestProviderService = httpService;
            _navigationManager = navigationManager;
            _localStorageService = localStorageService;
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
            var loginDto = _mapper.Map<LoginDto>(model);
            var response = await _requestProviderService.Post<Login, LoginResponseDto>("Auth/SignIn", model);
            _user = _mapper.Map<User>(response);
            await _localStorageService.SetItem(_userKey, _user);
            return response;
        }

        public async Task<bool> IsLoggedIn()
        {
            var user = await GetUser();
            var loggedin = false;
            if(user !=null && !string.IsNullOrEmpty(user.Id) && !string.IsNullOrEmpty(user.Token))
            {
                loggedin = true;
            }
            return loggedin;
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
            _user = null;
            await _localStorageService.SetItem(_userKey,new User());
            _navigationManager.NavigateTo("account/login");
        }

        public async Task<BrokerDto> Register(CreateBrokerDto model)
        {
            return await _requestProviderService.Post<CreateBrokerDto, BrokerDto>("BrokerAuth/signup", model);
        }

        public async Task<BrokerStaffDto> RegisterStaff(CreateBrokerStaffDto model)
        {
            return await _requestProviderService.Post<CreateBrokerStaffDto, BrokerStaffDto>("BrokerAuth/signupstaff", model);
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

    }
}