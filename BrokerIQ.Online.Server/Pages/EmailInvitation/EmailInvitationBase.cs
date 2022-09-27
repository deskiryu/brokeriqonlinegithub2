
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Models;

using BrokerIQ.Online.Services.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BrokerIQ.Online.Server.Shared;

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

        public List<EmailInvite> EmailInvitesSent { get; set; }

        public List<EmailInvite> EmailInvitesSentBase { get; set; }

        public List<TelephoneInvite> TelephoneInvitesSent { get; set; }

        public List<TelephoneInvite> TelephoneInvitesSentBase { get; set; }

        public List<BrokerStaff> BrokerStaff { get; set; }

        public List<string> EmailTargets { get; set; }

        public string CustomerName { get; set; }

        public string TelephoneNumber { get; set; }


        public string Email { get; set; }
        public string DragEnterStyle { get; set; }

        public string SearchTerm { get; set; } = "";

        public string SearchTermPhone { get; set; } = "";

        public bool IsAdmin { get; set; }

        public int BrokerId { get; set; }

        public int BrokerIdPhone { get; set; }

        public int? BrokerStaffId { get; set; }

        public List<Broker> Brokers { get; set; }

        //filter
        public List<EmailInvite> FilteredEmailInvites => EmailInvitesSent.Where(i => i.EmailAddress.ToLower().Contains(SearchTerm.ToLower())).ToList();

        public List<TelephoneInvite> FilteredTelephoneInvites => TelephoneInvitesSent.Where(i => i.CustomerName.ToLower().Contains(SearchTermPhone.ToLower())).ToList();

        public bool ShowEmployee { get; set; }

        public bool ShowBroker { get; set; }

        protected string Message = string.Empty;
        protected string StatusClass = string.Empty;
        protected bool Saved;

        private const string EmailAddressRegex = "^[A-Z0-9._%+-]+@[A-Z0-9.-]+\\.[A-Z]{2,6}$";
        private const string TelephoneRegex = @"^\+(?:[0-9]●?){6,14}[0-9]$";

        [Required]
        public int BrokerListId = 0;

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
                }

                EmailInvitesSentBase = (await EmailInviteService.GetEmailInvitesByBrokerId(user.MasterBrokerId)).ToList();
                TelephoneInvitesSentBase = (await TelephoneInviteService.GetTelephoneInvitesByBrokerId(user.MasterBrokerId)).ToList();
                FillBrokerStaff();
                EmailInvitesSent = EmailInvitesSentBase;
                TelephoneInvitesSent = TelephoneInvitesSentBase;
                Brokers = new List<Broker>();
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

        public async Task ShowSecondEmailList()
        {
            var dialogParams = new DialogParameters();
            var longlistEmails = new List<(string,int)>();

            foreach (var item in EmailInvitesSent)
            {
               if(item.Selected == true)
                {
                    longlistEmails.Add((item.EmailAddress,item.InvitationCount));   
                }
            }

            dialogParams.Add("EmailInvitation", longlistEmails);
            dialogParams.Add("Heading", "Broker IQ will send an invite email to these email addresses : ");
            var response = await DialogService.Show<ScrollableEmailDialog>("Send Reminder Emails", dialogParams).Result;
            if(!response.Cancelled)
            {
                bool succeeded = false;
                try
                {
                    var emailsToSend = new CreateEmailDto
                    {
                        To = longlistEmails.Select(x => x.Item1).ToList(),
                        BrokerId = BrokerId,
                        InvitationCount = longlistEmails.Select(x => x.Item2).ToList(),
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

            if (!validEmails){
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

        public async Task SendTelephoneInvites()
        {
            var dialogParams = new DialogParameters();
            var NameTelephoneTargets = new List<(string, string)>() { ( CustomerName, TelephoneNumber) };

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
                dialogParams.Add("Message", $"Please use valid telephone numbers starting with country code e.g +44");
                await DialogService.Show<AlertDialog>("Invite Connections", dialogParams).Result;
                NameTelephoneTargets.Clear();
            }
            else
            {
                var createTelephone = new CreateTelephoneInviteDto
                {
                    BrokerId = BrokerId,
                    BrokerStaffId = BrokerStaffId,
                    TelphoneNumbers = NameTelephoneTargets.Select(x => x.Item2).ToList(),
                    CustomerNames = NameTelephoneTargets.Select(x => x.Item1).ToList(),
                };

                if (IsAdmin)
                {
                    if (BrokerListId <= 0)
                    {
                        dialogParams.Add("Message", $"Please choose a broker");
                        await DialogService.Show<AlertDialog>("Invite Connections", dialogParams).Result;
                        return;
                    }
                    createTelephone.BrokerId = BrokerListId;
                    createTelephone.BrokerStaffId = null;
                }

                dialogParams.Add("Customers", NameTelephoneTargets.Select(x => x.Item1).ToList());
                dialogParams.Add("Heading", "The invitation connection with your brokerage will be made to ");
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
