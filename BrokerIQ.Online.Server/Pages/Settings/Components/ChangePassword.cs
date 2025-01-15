using System.Threading.Tasks;
using BrokerIQ.Dto.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using BrokerIQ.Online.Models.Account;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Services.Interface;

namespace BrokerIQ.Online.Server.Pages.Settings.Components
{
    public partial class ChangePassword : ComponentBase
    {
        [Inject]
        public IDialogService DialogService { get; set; }

        [Inject]
        public IAccountService AccountService { get; set; }

        [Inject]
        private ISnackbar Snackbar { get; set; }

        [Parameter]
        public string EmailAddress { get; set; }

        public UpdatePassword MyUpdatePassword { get; set; }

        protected bool isShowOld;
        protected InputType PasswordInputOld = InputType.Password;
        protected string PasswordInputIconOld = Icons.Material.Filled.VisibilityOff;

        protected bool isShowNew;
        protected InputType PasswordInputNew = InputType.Password;
        protected string PasswordInputIconNew = Icons.Material.Filled.VisibilityOff;

        protected bool isShowNewConfirm;
        protected InputType PasswordInputNewConfirm = InputType.Password;
        protected string PasswordInputIconNewConfirm = Icons.Material.Filled.VisibilityOff;

        protected override async Task OnInitializedAsync()
        {
            MyUpdatePassword = new UpdatePassword();
        }

        protected void HandleInvalidPasswordChange()
        {
            Snackbar.Add("There are some validation errors. Please try again.", Severity.Error);
        }

        protected async Task HandleValidPasswordChange()
        {
            Snackbar.Add("Password updated successfully.", Severity.Success);

            var dto = new UpdatePasswordDto()
            {
                EmailAddress = EmailAddress,
                OldPassword = MyUpdatePassword.ExistingPassword,
                NewPassword = MyUpdatePassword.NewPassword
            };

            try
            {
                await AccountService.ChangePassword(dto);
            }
            catch
            {
                Snackbar.Add("Something went wrong updating the Password. Please try again.", Severity.Error);
            }
        }

        protected void ShowPasswordOld()
        {
            if (isShowOld) { isShowOld = false; PasswordInputIconOld = Icons.Material.Filled.VisibilityOff; PasswordInputOld = InputType.Password; }
            else { isShowOld = true; PasswordInputIconOld = Icons.Material.Filled.Visibility; PasswordInputOld = InputType.Text; }
        }

        protected void ShowPasswordNew()
        {
            if (isShowNew) { isShowNew = false; PasswordInputIconNew = Icons.Material.Filled.VisibilityOff; PasswordInputNew = InputType.Password; }
            else { isShowNew = true; PasswordInputIconNew = Icons.Material.Filled.Visibility; PasswordInputNew = InputType.Text; }
        }

        protected void ShowPasswordNewConfirm()
        {
            if (isShowNewConfirm) { isShowNewConfirm = false; PasswordInputIconNewConfirm = Icons.Material.Filled.VisibilityOff; PasswordInputNewConfirm = InputType.Password; }
            else { isShowNewConfirm = true; PasswordInputIconNewConfirm = Icons.Material.Filled.Visibility; PasswordInputNewConfirm = InputType.Text; }
        }
    }
}