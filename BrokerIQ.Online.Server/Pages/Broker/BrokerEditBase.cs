using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Extensions;
using BrokerIQ.Online.Server.Models;
using BrokerIQ.Online.Server.Shared;
using BrokerIQ.Online.Services.Interface;

using MudBlazor;
using BrokerIQ.Dto.Models;
using BrokerIQ.Dto.Enum;
using BrokerIQ.Online.Models.Account;

namespace BrokerIQ.Online.Pages
{
    public class BrokerEditBase : ComponentBase
    {
        [Inject]
        public IBrokerService BrokerService { get; set; }

        [Inject]
        public IBrokerIdentifierService BrokerIdentifierService { get; set; }

        [Inject]
        public IBrokerSubscriptionService BrokerSubscriptionService { get; set; }

        [Inject]
        public IAccountService AccountService { get; set; }

        [Inject]
        public ILogoService LogoService { get; set; }

        [Inject]
        public IInsuranceDocumentService SupportingDocumentService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IDialogService DialogService { get; set; }

        [Parameter]
        public string BrokerId { get; set; }

        public User CurrentUser { get; set; }

        public Broker Broker { get; set; }

        public UpdatePassword MyUpdatePassword { get; set; }

        protected string Message = string.Empty;

        protected string StatusClass = string.Empty;

        protected bool Saved;

        protected bool isShowOld;
        protected InputType PasswordInputOld = InputType.Password;
        protected string PasswordInputIconOld = Icons.Material.Filled.VisibilityOff;

        protected bool isShowNew;
        protected InputType PasswordInputNew = InputType.Password;
        protected string PasswordInputIconNew = Icons.Material.Filled.VisibilityOff;

        protected bool isShowNewConfirm;
        protected InputType PasswordInputNewConfirm = InputType.Password;
        protected string PasswordInputIconNewConfirm = Icons.Material.Filled.VisibilityOff;

        public string IdentifierTabLabel
        {
            get
            {
                return Broker.Subscriptions.Any(s => s.SubscriptionServiceId == (int)SubscriptionServiceEnum.WhiteLabel) ? "White Label" : string.Empty;
            }
        }

        public bool IsCreatingWhiteLabel { get; set; } = false;

        public BrokerEditBase()
        {
            Broker = new Broker
            {
                BrokerIdentifier = new BrokerIdentifier
                {
                    IdentifierFound = false
                }
            };
        }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                CurrentUser = await AccountService.GetUser();

                var id = CurrentUser.IsAdmin || CurrentUser.IsMinorAdmin ? Int32.Parse(BrokerId) : CurrentUser.MasterBrokerId;

                if (id > 0)
                {
                    Broker = await BrokerService.GetBroker(id, true);
                    Broker.Subscriptions = (await BrokerSubscriptionService.GetAllForBroker(id)).ToList();
                }
                MyUpdatePassword = new UpdatePassword();
            }
            catch
            {
                NavigationManager.NavigateTo($"account/logout");
            }
        }

        protected void HandleInvalidSubmit()
        {
            StatusClass = "alert-danger";
            Message = "There are some validation errors. Please try again.";
        }

        protected async Task HandleValidSubmit()
        {

            StatusClass = "alert-success";
            Message = "Broker updated successfully.";
            try
            {
                await BrokerService.UpdateBroker(Broker);
            }
            catch
            {
                StatusClass = "alert-danger";
                Message = "Something went wrong updating the Broker. Please try again.";

            }
            finally
            {
                Saved = true;
            }
        }

        protected void HandleInvalidSubmitIdentifier()
        {
            StatusClass = "alert-danger";
            Message = "There are some validation errors. Please try again.";
        }

        protected async Task HandleValidSubmitIdentifier()
        {
            StatusClass = "alert-success";
            Message = "Broker identifier updated successfully.";
            var dialogParams = new DialogParameters
            {
                { "Message", $"Are you absolutely sure you want to change settings for {Broker.Name}? This changes can affect app, email and notifications!!!!" }
            };
            var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Canceled)
            {
                try
                {
                    if (IsCreatingWhiteLabel)
                    {
                        await BrokerIdentifierService.AddBrokerIdentifier(GetCreateBrokerIdentifierFrom(Broker.BrokerIdentifier));
                        IsCreatingWhiteLabel = false;
                    }
                    else
                    {
                        await BrokerIdentifierService.UpdateBrokerIdentifier(Broker.BrokerIdentifier);
                    }
                }
                catch
                {
                    StatusClass = "alert-danger";
                    Message = "Something went wrong updating the Broker Identifier. Please try again.";
                }
                finally
                {
                    Saved = true;
                }
            }
        }

        private CreateBrokerIdentifierDto GetCreateBrokerIdentifierFrom(BrokerIdentifier brokerIdentifier)
        {
            return new CreateBrokerIdentifierDto()
            {
                BrokerId = Broker.Id,
                BundleIdentifier = brokerIdentifier.BundleIdentifier,
                BackgroundColour = brokerIdentifier.BackgroundColour,
                TextColour = brokerIdentifier.TextColour,
                HttpLink = brokerIdentifier.HttpLink,
                HttpAddress = brokerIdentifier.HttpAddress,
                AppName = brokerIdentifier.AppName,
                Invert = brokerIdentifier.Invert,
                LinkColour = brokerIdentifier.LinkColour,
                AppStoreLink = brokerIdentifier.AppStoreLink,
                PlayStoreLink = brokerIdentifier.PlayStoreLink,
                FromEmailName = brokerIdentifier.FromEmailName,
                FromEmailAddress = brokerIdentifier.FromEmailAddress,
                WelcomeVideoUrl = brokerIdentifier.WelcomeVideoUrl,
                HubClientConnectString = brokerIdentifier.HubClientConnectString,
                HubClientName = brokerIdentifier.HubClientName,
                FirebaseKey = brokerIdentifier.FirebaseKey,
                FirebaseClient = brokerIdentifier.FirebaseClient,
                FirebaseProjectId = brokerIdentifier.FirebaseProjectId,
                EmailTagLine = brokerIdentifier.EmailTagLine,
                InsuranceOnly = brokerIdentifier.InsuranceOnly
            };
        }

        protected async void NavigateToOverview()
        {
            if (CurrentUser.IsAdmin || CurrentUser.IsMinorAdmin)
            {
                NavigationManager.NavigateTo($"/brokerlist");
            }
            else
            {
                Saved = false;
                StateHasChanged();
            }

        }

        public async Task LoadFiles(InputFileChangeEventArgs e)
        {

            try
            {
                var file = e.GetMultipleFiles(1).FirstOrDefault();
                var ext = Path.GetExtension(file.Name);
                if (ext != ".jpeg" && ext != ".jpg")
                {
                    throw new Exception("Jpeg files only");
                }
                if (file != null)
                {
                    var memoryStream = new MemoryStream();
                    await file.OpenReadStream(int.MaxValue).CopyToAsync(memoryStream);
                    Broker.LogoImage = memoryStream.ToArray();
                    var fileName = Broker.Id.ToString() + ".jpeg";
                    memoryStream.Position = 0;
                    await LogoService.UploadLogo(fileName, memoryStream, Broker.Id);
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                StateHasChanged();
            }
        }

        protected async Task DeleteBroker()
        {
            var dialogParams = new DialogParameters();
            dialogParams.Add("Message", $"Are you absolutely sure you want to delete this broker {Broker.Name}? This is a PERMANENT DELETE and cannot be undone. All links to clients will be lost. Ensure this broker has no insurances or mortgages with clients.");
            var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Canceled)
            {
                var deleted = await BrokerService.DeleteBroker(Broker.Id);
                if (deleted)
                {
                    var responseParams = new DialogParameters();
                    responseParams.Add("Message", "Deleted successfully");
                    await DialogService.Show<AlertDialog>("Information", responseParams).Result;
                }
                else
                {
                    var responseParams = new DialogParameters();
                    responseParams.Add("Message", "The broker did not delete.");
                    await DialogService.Show<AlertDialog>("Information", responseParams).Result;
                }
                NavigationManager.NavigateTo($"/brokerlist");
            }
        }

        protected async Task SetUseBrokerPhoneNumber()
        {
            if (!Broker.TwoFactorUseBrokerPhoneNumber)
            {
                Broker.TwoFactorPhoneNumber = Broker.TelephoneNumber.GetFormattedPhoneNumber();
            }
            else
            {
                Broker.TwoFactorPhoneNumber = "";
            }
        }

        protected async Task SetTwoFactorEnabled()
        {
            bool success = await AccountService.ToggleTwoFactor(Broker.EmailAddress, !Broker.TwoFactorEnabled);
            if (success)
            {
                var responseParams = new DialogParameters();
                responseParams.Add("Message", "Two factor set/cleared successfully");
                await DialogService.Show<AlertDialog>("Information", responseParams).Result;
            }
            else
            {
                var responseParams = new DialogParameters();
                responseParams.Add("Message", "The two factor was not changed.");
                await DialogService.Show<AlertDialog>("Information", responseParams).Result;
            }
            NavigationManager.NavigateTo($"/brokerlist");
        }

        protected async Task ResendEmailBroker()
        {
            var dialogParams = new DialogParameters();
            dialogParams.Add("Message", $"A verify email will be sent to {Broker.EmailAddress}. Continue? ");
            var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Canceled)
            {
                var resent = await AccountService.ResendEmailBroker(Broker.EmailAddress);
                if (resent)
                {
                    var responseParams = new DialogParameters();
                    responseParams.Add("Message", "Resent successfully");
                    await DialogService.Show<AlertDialog>("Information", responseParams).Result;
                }
                else
                {
                    var responseParams = new DialogParameters();
                    responseParams.Add("Message", "The resend email failed.");
                    await DialogService.Show<AlertDialog>("Information", responseParams).Result;
                }
                NavigationManager.NavigateTo($"/brokerlist");
            }
        }

        protected void HandleInvalidPasswordChange()
        {
            StatusClass = "alert-danger";
            Message = "There are some validation errors. Please try again.";
        }

        protected async Task HandleValidPasswordChange()
        {

            StatusClass = "alert-success";
            Message = "Password updated successfully.";
            var dto = new UpdatePasswordDto()
            {
                EmailAddress = Broker.EmailAddress,
                OldPassword = MyUpdatePassword.ExistingPassword,
                NewPassword = MyUpdatePassword.NewPassword
            };

            try
            {
                await AccountService.ChangePassword(dto);
            }
            catch
            {
                StatusClass = "alert-danger";
                Message = "Something went wrong updating the Password. Please try again.";

            }
            finally
            {
                Saved = true;
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
