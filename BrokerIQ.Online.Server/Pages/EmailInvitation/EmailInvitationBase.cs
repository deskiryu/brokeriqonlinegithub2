using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;

using BrokerIQ.Online.Services.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BrokerIQ.Online.Server.Shared;
using Microsoft.JSInterop;
using BrokerIQ.Online.Server.Extensions;
using BrokerIQ.Online.Data;
using Microsoft.AspNetCore.Components.Web;

namespace BrokerIQ.Online.Pages
{
    public class EmailInvitationBase : ComponentBase
    {
        [Inject]
        public IEmailInviteService EmailInviteService { get; set; }

        [Inject]
        public ITelephoneInviteService TelephoneInviteService { get; set; }

        [Inject]
        public IEmailService EmailService { get; set; }

        [Inject]
        public IBrokerStaffService BrokerStaffService { get; set; }

        [Inject]
        public IBrokerService BrokerService { get; set; }

        [Inject]
        public IAlertService AlertService { get; set; }

        [Inject]
        public IAccountService AccountService { get; set; }

        [Inject]
        public IDialogService DialogService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IJSRuntime js { get; set; }

        public List<EmailInvite> EmailInvitesSent { get; set; }

        public List<EmailInvite> EmailInvitesSentBase { get; set; }

        public HashSet<EmailInvite> EmailInvitesSelected { get; set; }

        public List<TelephoneInvite> TelephoneInvitesSent { get; set; }

        public List<TelephoneInvite> TelephoneInvitesSentBase { get; set; }

        public HashSet<TelephoneInvite> TelephoneInvitesSelected { get; set; }

        public List<BrokerStaff> BrokerStaff { get; set; }

        public List<string> EmailTargets { get; set; }

        public string CustomerName { get; set; }

        public string TelephoneNumber { get; set; }

        public string Email { get; set; }

        public string SearchTerm { get; set; } = "";

        public string SearchTermPhone { get; set; } = "";

        public bool IsAdmin { get; set; }

        public int BrokerId { get; set; }

        public int BrokerIdPhone { get; set; }

        public int? BrokerStaffId { get; set; }

        public List<Broker> Brokers { get; set; }

        public Broker Broker { get; set; }

        //filter
        public List<EmailInvite> FilteredEmailInvites => EmailInvitesSent.Where(i => string.IsNullOrEmpty(i.EmailAddress) || i.EmailAddress.ToLower().Contains(SearchTerm.ToLower())).ToList();

        public List<TelephoneInvite> FilteredTelephoneInvites => TelephoneInvitesSent.Where(i => string.IsNullOrEmpty(CustomerName) || i.CustomerName.ToLower().Contains(SearchTermPhone.ToLower())).ToList();

        public bool ShowEmployee { get; set; }

        public bool ShowBroker { get; set; }

        protected string Message = string.Empty;
        protected string StatusClass = string.Empty;
        protected bool Saved;

        private const string EmailAddressRegex = "^[A-Z0-9._%+-]+@[A-Z0-9.-]+\\.[A-Z]{2,6}$";
        private const string TelephoneRegex = @"^\+(?:[0-9]●?){6,14}[0-9]$";

        [Required]
        public int BrokerListId = 0;

        [Required]
        public int BrokerListIdTelephone = 0;

        public string BrokerStaffFirstName { get; set; }

        protected string HoverClass;

        protected void OnDragEnter(DragEventArgs e) => HoverClass = "drag-file-hover";

        protected void OnDragLeave(DragEventArgs e) => HoverClass = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await FillDetails();
            }
            catch
            {
                NavigationManager.NavigateTo($"account/logout");
            }
        }

        private async Task FillDetails()
        {
            EmailTargets = new List<string>();

            IsAdmin = false;
            var user = await this.AccountService.GetUser();
            ShowEmployee = false;
            ShowBroker = false;
            BrokerStaffId = null;

            if (user.IsBroker || user.IsBrokerStaff)
            {
                BrokerId = user.MasterBrokerId;
                if (user.IsBroker)
                {
                    BrokerStaff = (await BrokerStaffService.GetBrokerStaffbyBrokerId(user.MasterBrokerId)).ToList();
                    ShowEmployee = true;
                }
                else
                {
                    BrokerStaffId = Int32.Parse(user.Id);
                    var brokerStaff = await BrokerStaffService.GetBrokerStaff(BrokerStaffId.Value);
                    BrokerStaffFirstName = brokerStaff.FirstName;
                }

                EmailInvitesSentBase = (await EmailInviteService.GetEmailInvitesByBrokerId(user.MasterBrokerId)).ToList();
                TelephoneInvitesSentBase = (await TelephoneInviteService.GetTelephoneInvitesByBrokerId(user.MasterBrokerId)).ToList();
                FillBrokerStaff();
                EmailInvitesSent = EmailInvitesSentBase;
                TelephoneInvitesSent = TelephoneInvitesSentBase;
                Brokers = new List<Broker>();
                Broker = (await BrokerService.GetBroker(user.MasterBrokerId, eagerload: true));
            }
            else if (user.IsAdmin)
            {
                IsAdmin = true;
                ShowBroker = true;
                Brokers = (await BrokerService.GetBrokers()).ToList();
                BrokerStaff = (await BrokerStaffService.GetBrokerStaff()).ToList();
                EmailInvitesSentBase = (await EmailInviteService.GetEmailInvites()).ToList();
                TelephoneInvitesSentBase = (await TelephoneInviteService.GetTelephoneInvites()).ToList();
                FillBrokerStaff();
                EmailInvitesSent = EmailInvitesSentBase;
                TelephoneInvitesSent = TelephoneInvitesSentBase;
            }
            else
            {
                ShowEmployee = false;
            }
        }

        protected async Task AutoCompleteClickBroker()
        {
            if (BrokerId == 0)
            {
                EmailInvitesSent = EmailInvitesSentBase;
            }
            else if (BrokerId > 0)
            {
                EmailInvitesSent = EmailInvitesSentBase.Where(x => x.BrokerId == BrokerId).ToList();
            }
        }


        protected async Task AutoCompleteClickBrokerPhone()
        {
            if (BrokerIdPhone == 0)
            {
                TelephoneInvitesSent = TelephoneInvitesSentBase;
            }
            else if (BrokerIdPhone > 0)
            {
                TelephoneInvitesSent = TelephoneInvitesSent.Where(x => x.BrokerId == BrokerIdPhone).ToList();
            }
        }

        private void FillBrokerStaff()
        {
            foreach (var notif in EmailInvitesSentBase)
            {
                notif.BrokerStaffName = "-";
                notif.BrokerName = "-";
                if (notif.BrokerStaffId != null && BrokerStaff != null && BrokerStaff.Count > 0)
                {
                    var foundStaff = BrokerStaff.FirstOrDefault(x => x.Id == notif.BrokerStaffId);
                    if (foundStaff != null)
                    {
                        notif.BrokerStaffName = foundStaff.FirstName + " " + foundStaff.LastName;
                    }
                }
                if (IsAdmin)
                {
                    var foundBroker = Brokers.FirstOrDefault(x => x.Id == notif.BrokerId);
                    if (foundBroker != null)
                    {
                        notif.BrokerName = foundBroker.Name;
                    }
                }
            }

            foreach (var notif in TelephoneInvitesSentBase)
            {
                notif.BrokerStaffName = "-";
                notif.BrokerName = "-";
                if (notif.BrokerStaffId != null && BrokerStaff != null && BrokerStaff.Count > 0)
                {
                    var foundStaff = BrokerStaff.FirstOrDefault(x => x.Id == notif.BrokerStaffId);
                    if (foundStaff != null)
                    {
                        notif.BrokerStaffName = foundStaff.FirstName + " " + foundStaff.LastName;
                    }
                }
                if (IsAdmin)
                {
                    var foundBroker = Brokers.FirstOrDefault(x => x.Id == notif.BrokerId);
                    if (foundBroker != null)
                    {
                        notif.BrokerName = foundBroker.Name;
                    }
                }
            }
        }

        public void AddEmail()
        {
            EmailTargets.Add(Email);
        }


        public async Task DeleteSelectedInviteList()
        {
            var dialogParams = new DialogParameters();
            var longlistEmails = new List<(string, int)>();

            foreach (var item in EmailInvitesSelected)
            {
                if (item.Converted == false)
                {
                    longlistEmails.Add((item.EmailAddress, item.Id));
                }
            }

            dialogParams.Add("EmailInvitation", longlistEmails);
            dialogParams.Add("Heading", "Broker IQ will delete these connections ");
            dialogParams.Add("Delete", true);
            var response = await DialogService.Show<ScrollableEmailDialog>("Delete invitations", dialogParams).Result;
            if (!response.Cancelled)
            {
                bool succeeded = false;

                try
                {
                    foreach (var item in longlistEmails)
                    {
                        succeeded = await this.EmailInviteService.DeleteEmailInvite(item.Item2);
                    }
                }
                catch
                {
                    succeeded = false;
                }

                if (succeeded)
                {
                    StatusClass = "alert-success";
                    Message = "Connections deleted successfully";
                }
                else
                {
                    StatusClass = "alert-danger";
                    Message = "Some or all of the connections did not delete, check your invite list";
                }

                Saved = true;
            }
        }


        public async Task ShowSecondEmailList()
        {
            var dialogParams = new DialogParameters();
            var longlistEmails = new List<(string, int)>();

            foreach (var item in EmailInvitesSelected)
            {
                if (item.Converted == false)
                {
                    longlistEmails.Add((item.EmailAddress, item.InvitationCount));
                }
            }

            dialogParams.Add("EmailInvitation", longlistEmails);
            dialogParams.Add("Heading", "Broker IQ will send an invite email to these email addresses : ");
            dialogParams.Add("Delete", false);
            var response = await DialogService.Show<ScrollableEmailDialog>("Send Reminder Emails", dialogParams).Result;
            if (!response.Cancelled)
            {
                bool succeeded = false;
                try
                {
                    var emailsToSend = new CreateEmailDto
                    {
                        To = longlistEmails.Select(x => x.Item1).ToList(),
                        BrokerId = BrokerId,
                        InvitationCount = longlistEmails.Select(x => x.Item2).ToList(),
                        Subject = "Subject",
                        Content = "Content"
                    };

                    succeeded = await EmailService.SendInviteEmails(emailsToSend);
                }
                catch
                {

                }

                if (succeeded)
                {
                    StatusClass = "alert-success";
                    Message = "Invite Emails sent successfully";
                }
                else
                {
                    StatusClass = "alert-danger";
                    Message = "Some or all of the emails did not send, check your invite list";
                }

                Saved = true;
            }
        }

        public async Task LoadFiles(InputFileChangeEventArgs e)
        {

            try
            {
                var file = e.GetMultipleFiles(1).FirstOrDefault();
                if (file != null)
                {
                    var memoryStream = new MemoryStream();
                    await file.OpenReadStream(int.MaxValue).CopyToAsync(memoryStream);
                    using (var reader = new StreamReader(memoryStream))
                    {
                        memoryStream.Position = 0;
                        string line;

                        while ((line = reader.ReadLine()) != null)
                        {
                            var emailAddress = line;
                            EmailTargets.Add(emailAddress);
                        }
                    }
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

        public async Task SendInvites()
        {
            var dialogParams = new DialogParameters();


            bool validEmails = true;
            foreach (var item in EmailTargets)
            {
                validEmails &= Regex.IsMatch(item,
                                    EmailAddressRegex,
                                    RegexOptions.IgnoreCase,
                                    TimeSpan.FromMilliseconds(250));
            }

            if (!validEmails)
            {
                dialogParams.Add("Message", $"Please use valid email addresses");
                await DialogService.Show<AlertDialog>("Invite Connections", dialogParams).Result;
                EmailTargets.Clear();
            }
            else
            {
                var createEmail = new CreateEmailInviteDto
                {
                    BrokerId = BrokerId,
                    BrokerStaffId = BrokerStaffId,
                    EmailAddresses = EmailTargets
                };

                if (IsAdmin)
                {
                    if (BrokerListId <= 0)
                    {
                        dialogParams.Add("Message", $"Please choose a broker");
                        await DialogService.Show<AlertDialog>("Invite Connections", dialogParams).Result;
                        return;
                    }
                    createEmail.BrokerId = BrokerListId;
                    createEmail.BrokerStaffId = null;
                }

                dialogParams.Add("Customers", EmailTargets);
                dialogParams.Add("Heading", "The invitation connection with your brokerage will be made to ");
                var result = await DialogService.Show<ScrollableEmailDialog>("Make Connections", dialogParams).Result;

                if (!result.Cancelled)
                {
                    bool succeeded = false;
                    try
                    {
                        succeeded = await EmailInviteService.AddEmailInvites(createEmail);
                    }
                    catch
                    {

                    }

                    if (succeeded)
                    {
                        await RefreshInvitationsWithDialogMessage(succeeded, "Email connection made successfully");
                    }
                    else
                    {
                        await RefreshInvitationsWithDialogMessage(succeeded, "Some or all of the emails did not add, they may be associated with another broker, check your invite list");
                    }
                }
            }
        }

        public async Task DeleteSelectedInviteListTelephone()
        {
            var dialogParams = new DialogParameters();
            var longlistEmails = new List<(string, int)>();

            foreach (var item in TelephoneInvitesSelected)
            {
                if (item.Converted == false)
                {
                    longlistEmails.Add((item.CustomerName, item.Id));
                }
            }

            dialogParams.Add("EmailInvitation", longlistEmails);
            dialogParams.Add("Heading", "Broker IQ will delete these connections ");
            dialogParams.Add("Delete", true);
            var response = await DialogService.Show<ScrollableEmailDialog>("Delete invitations", dialogParams).Result;
            if (!response.Cancelled)
            {
                bool succeeded = false;

                try
                {
                    foreach (var item in longlistEmails)
                    {
                        succeeded = await this.EmailInviteService.DeleteEmailInvite(item.Item2);
                    }
                }
                catch
                {
                    succeeded = false;
                }

                if (succeeded)
                {
                    StatusClass = "alert-success";
                    Message = "Connections deleted successfully";
                }
                else
                {
                    StatusClass = "alert-danger";
                    Message = "Some or all of the connections did not delete, check your invite list";
                }

                Saved = true;
            }
        }


        public async Task SendTelephoneInvites()
        {
            var dialogParams = new DialogParameters();
            var NameTelephoneTargets = new List<(string, string)>() { (CustomerName, TelephoneNumber) };

            bool validTelephones = true;
            foreach (var item in NameTelephoneTargets)
            {
                validTelephones &= Regex.IsMatch(item.Item2,
                                    TelephoneRegex,
                                    RegexOptions.IgnoreCase,
                                    TimeSpan.FromMilliseconds(250));
            }

            if (!validTelephones)
            {
                dialogParams.Add("Message", $"Please use valid telephone numbers starting with country code and no spaces e.g +447812345678");
                await DialogService.Show<AlertDialog>("Invite Connections", dialogParams).Result;
                NameTelephoneTargets.Clear();
            }
            else
            {
                var brokerId = BrokerStaffId > 0 ? 0 : BrokerId;
                var createTelephone = new CreateTelephoneInviteDto
                {
                    BrokerId = brokerId,
                    BrokerStaffId = BrokerStaffId,
                    TelphoneNumbers = NameTelephoneTargets.Select(x => x.Item2).ToList(),
                    CustomerNames = NameTelephoneTargets.Select(x => x.Item1).ToList(),
                };

                if (IsAdmin)
                {
                    if (BrokerListIdTelephone <= 0)
                    {
                        dialogParams.Add("Message", $"Please choose a broker");
                        await DialogService.Show<AlertDialog>("Invite Connections", dialogParams).Result;
                        return;
                    }
                    createTelephone.BrokerId = BrokerListIdTelephone;
                    createTelephone.BrokerStaffId = null;
                    Broker = await this.BrokerService.GetBroker(BrokerListIdTelephone);
                }

                dialogParams.Add("Customers", NameTelephoneTargets.Select(x => x.Item1).ToList());
                dialogParams.Add("Heading", "The invitation connection with your brokerage will be made to ");
                dialogParams.Add("Delete", false);
                var result = await DialogService.Show<ScrollableEmailDialog>("Make Connections", dialogParams).Result;

                if (!result.Cancelled)
                {
                    bool succeeded = false;
                    try
                    {
                        succeeded = await TelephoneInviteService.AddTelephoneInvites(createTelephone);
                    }
                    catch
                    {

                    }

                    if (succeeded)
                    {
                        CustomerName = string.Empty;
                        TelephoneNumber = string.Empty;
                        await RefreshInvitationsWithDialogMessage(succeeded, "Telephone connection made successfully");
                    }
                    else
                    {
                        await RefreshInvitationsWithDialogMessage(succeeded, "Some or all of the Telephones did not add, they may be associated with another broker, check your invite list");
                    }
                }
            }
        }

        protected async Task WhatsApp(string telephoneNumber)
        {
            var dialogParams = new DialogParameters();
            bool succeeded = false;
            try
            {
                if (IsAdmin)
                {
                    if (BrokerListIdTelephone <= 0)
                    {
                        dialogParams.Add("Message", $"Please choose a broker");
                        await DialogService.Show<AlertDialog>("Invite Connections", dialogParams).Result;
                        return;
                    }
                    Broker = await this.BrokerService.GetBroker(BrokerListIdTelephone);
                }

                var appName = "BrokerIQ";
                var playstore = Urls.PlayStoreLink;
                var appStore = Urls.AppStoreLink;
                var brokerName = Broker.Name.Replace("&", "%26");
                var brokerFirstName = Broker.BrokerFirstName;

                if (Broker.BrokerIdentifier.IdentifierFound)
                {
                    appStore = Broker.BrokerIdentifier.AppStoreLink;
                    playstore = Broker.BrokerIdentifier.PlayStoreLink;
                    appName = Broker.BrokerIdentifier.AppName;
                }

                if (BrokerStaffId > 0)
                {
                    brokerFirstName = BrokerStaffFirstName;
                }

                string message = $"Hi its {brokerFirstName} from {brokerName}, we have a new app called {appName}. We will be using the app to communicate with you, collect information and share important updates about your case. %0a";
                message += $"Please download the app for your device.%0aiOS:%0a{appStore}%0aAndroid:%0a{playstore}";

                var url = $"https://wa.me/{telephoneNumber}/?text={message}";
                await Extensions.NavigateToNewTab(js, url);

            }
            catch
            {

            }
        }

        protected async Task EditNote(string note, int id)
        {
            bool succeeded = false;
            var dialogParams = new DialogParameters();
            dialogParams.Add("Message", note);

            var result = await DialogService.Show<NoteEditDialog>("Edit Note", dialogParams).Result;
            if (!result.Cancelled)
            {
                var message = result.Data.ToString();
                try
                {
                    if (!string.IsNullOrEmpty(message))
                    {
                        try
                        {
                            succeeded = (await TelephoneInviteService.SaveTelephoneNotes(id, message));
                        }
                        catch
                        {
                            await RefreshInvitationsWithDialogMessage(false, "Something went wrong updating the Note. Please try again.");
                        }
                    }
                }
                catch
                {

                }
            }
            else
            {
                return;
            }

            if (succeeded)
            {
                await RefreshInvitationsWithDialogMessage(succeeded, "Note updated successfully");
            }
            else
            {
                await RefreshInvitationsWithDialogMessage(succeeded, "Something went wrong updating the note. Please try again");
            }
        }

        protected async Task DeleteLink(int id)
        {
            bool succeeded = false;
            try
            {
                var dialogParams = new DialogParameters();
                dialogParams.Add("Message", "Are you sure you want to delete this connection?");
                var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
                if (!result.Cancelled)
                {
                    succeeded = await this.EmailInviteService.DeleteEmailInvite(id);
                }

            }
            catch
            {

            }

            if (succeeded)
            {
                await RefreshInvitationsWithDialogMessage(succeeded, "Connection deleted successfully");
            }
            else
            {
                await RefreshInvitationsWithDialogMessage(succeeded, "The invite connection did not delete, check your invite list");
            }
        }



        protected void NavigateToOverview()
        {
            NavigationManager.NavigateTo($"refresh");
        }

        /// <summary>
        /// Refreshes invitation information displayed on webpage if desired. Displays appropriate dialog message.
        /// </summary>
        /// <param name="success">Success of prior API call</param>
        /// <param name="message">Message to be displayed in dialog</param>
        private async Task RefreshInvitationsWithDialogMessage(bool success, string message)
        {
            if (success)
            {
                await FillDetails();
                StateHasChanged();
            }
            var responseParams = new DialogParameters();
            responseParams.Add("Message", message);
            await DialogService.Show<AlertDialog>("Information", responseParams).Result;
        }
    }
}
