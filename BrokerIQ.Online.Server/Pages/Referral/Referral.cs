
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
using Microsoft.JSInterop;
using BrokerIQ.Online.Server.Extensions;
using BrokerIQ.Online.Data;
using BrokerIQ.Online.Services;
using BrokerIQ.Online.Server.Services;
using Microsoft.IdentityModel.Tokens;

namespace BrokerIQ.Online.Pages
{
    public class ClientReferralBase : ComponentBase
    {

        [Inject]
        public ITelephoneInviteService TelephoneInviteService { get; set; }

        [Inject]
        public IClientReferralService ClientReferralService { get; set; }

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

        public List<ClientReferral> ClientReferralsSent { get; set; }

        public List<ClientReferral> ClientReferralsSentBase { get; set; }

        public HashSet<ClientReferral> ClientReferralsSelected { get; set; }

        public List<BrokerStaff> BrokerStaff { get; set; }

        public string CustomerName { get; set; }

        public string TelephoneNumber { get; set; }

        [Inject]
        protected ICustomerService CustomerService { get; set; }


        public string Email { get; set; }
        public string DragEnterStyle { get; set; }

        public string SearchTerm { get; set; } = "";

        public bool IsAdmin { get; set; }

        public int BrokerId { get; set; }

        public int? BrokerStaffId { get; set; }

        public List<Broker> Brokers { get; set; }

        public Broker Broker { get; set; }

        protected List<Customer> Customers { get; set; }

        //filter
        public List<ClientReferral> FilteredClientReferrals => ClientReferralsSent.Where(i => i.ReferralName.IsNullOrEmpty() ||  i.ReferralName.ToLower().Contains(SearchTerm.ToLower())).ToList();

        public bool ShowEmployee { get; set; }

        public bool ShowBroker { get; set; }

        protected string Message = string.Empty;
        protected string StatusClass = string.Empty;
        protected bool Saved;

        private const string EmailAddressRegex = "^[A-Z0-9._%+-]+@[A-Z0-9.-]+\\.[A-Z]{2,6}$";
        private const string TelephoneRegex = @"^\+(?:[0-9]●?){6,14}[0-9]$";

        [Required]
        public int BrokerListId = 0;

        public string BrokerStaffFirstName { get; set; }

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

            IsAdmin = false;
            var user = await this.AccountService.GetUser();
            ShowEmployee = false;
            ShowBroker = false;
            BrokerStaffId = null;

            var queryCust = await CustomerService.GetAllCustomers();
            Customers = queryCust.ToList();

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

                ClientReferralsSentBase = (await ClientReferralService.GetReferralsByBrokerId(user.MasterBrokerId)).ToList();
                FillBrokerStaff();
                FillCustomer();
                ClientReferralsSent = ClientReferralsSentBase;
                Brokers = new List<Broker>();
                Broker = (await BrokerService.GetBroker(user.MasterBrokerId, eagerload: true));
            }
            else if (user.IsAdmin)
            {
                IsAdmin = true;
                ShowBroker = true;
                Brokers = (await BrokerService.GetBrokers()).ToList();
                BrokerStaff = (await BrokerStaffService.GetBrokerStaff()).ToList();
                ClientReferralsSentBase = (await ClientReferralService.GetReferralsByBrokerId(0)).ToList();
                FillBrokerStaff();
                FillCustomer();
                ClientReferralsSent = ClientReferralsSentBase;
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
                ClientReferralsSent = ClientReferralsSentBase;
            }
            else if (BrokerId > 0)
            {
                ClientReferralsSent = ClientReferralsSentBase.Where(x => x.BrokerId == BrokerId).ToList();
            }
        }

        private void FillBrokerStaff()
        {
            foreach (var notif in ClientReferralsSentBase)
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

        private void FillCustomer()
        {
            foreach (var notif in ClientReferralsSentBase)
            {
                notif.CustomerName = "-";

                if (notif.CustomerId  > 0)
                {
                    var foundCust = Customers.FirstOrDefault(x => x.Id == notif.CustomerId);
                    if (foundCust != null)
                    {
                        notif.CustomerName = foundCust.Name;
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
                dialogParams.Add("Message", "Are you sure you want to delete this referral?");
                var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
                if (!result.Cancelled)
                {
                    succeeded = await this.ClientReferralService.Delete(id, BrokerId);
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

        protected async Task EditNote(string note, ClientReferral clientReferral)
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
                            clientReferral.ReferralNote = message;
                            var returned = await ClientReferralService.Update(clientReferral);
                            succeeded = returned != null;
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

        protected async Task UpdateCR(ClientReferral clientReferral)
        {
            bool succeeded = false;

            try
            {
                var returned = await ClientReferralService.Update(clientReferral);
                succeeded = returned != null;
            }
            catch
            {
                await RefreshInvitationsWithDialogMessage(false, "Something went wrong updating the referral. Please try again.");
            }
   

            if (succeeded)
            {
                await RefreshInvitationsWithDialogMessage(succeeded, "Referral updated successfully");
            }
            else
            {
                await RefreshInvitationsWithDialogMessage(succeeded, "Something went wrong updating the referral. Please try again");
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
