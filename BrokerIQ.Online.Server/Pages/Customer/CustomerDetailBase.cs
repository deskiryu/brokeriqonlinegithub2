using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Pages
{
    using Microsoft.AspNetCore.Components;
    using Models;
    using BrokerIQ.Dto.Enum;
    using BrokerIQ.Dto.Models;
    using BrokerIQ.Online.Services.Interface;
    using MudBlazor;
    using System.IO;
    using BrokerIQ.Online.Server.Shared;

    public class CustomerDetailBase : ComponentBase
    {
        [Inject]
        public ICustomerService CustomerService { get; set; }

        [Inject]
        public IBrokerService BrokerService { get; set; }

        [Inject]
        public IAccountService AccountService { get; set; }

        [Inject]
        public ICustomerDocumentService CustomerDocumentService { get; set; }

        [Inject]
        public IDialogService DialogService { get; set; }

        [Inject]
        public INotificationService NotificationService { get; set; }

        [Inject]
        public IAlertService AlertService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IMenuPlanService MenuPlanService { get; set; }

        [Inject]
        public INoteService NoteService { get; set; }

        [Inject]
        public IChatService ChatService { get; set; }

        public Customer Customer { get; set; }

        public CustomerDocumentDto CustomerProfilePicture { get; set; }

        public IEnumerable<Broker> Brokers { get; set; }

        public IEnumerable<Broker> CustomerBrokers { get; set; }

        public IEnumerable<CustomerDocumentDto> CustomerDocuments { get; set; }

        public IEnumerable<Note> Notes { get; set; }

        public Chat Chat { get; set; }

        [Parameter]
        public string CustomerId { get; set; }
        public bool IsAdmin { get; set; }

        protected string Message = string.Empty;
        protected string StatusClass = string.Empty;
        protected bool Saved;

        protected string allNotification;
        protected string selectedNotification;

        protected MemoryStream memoryStream = new MemoryStream();
        protected string imageFileName { get; set; }
        protected byte[] imageData { get; set; }

        public int BrokerListId = 0;


        protected override async Task OnInitializedAsync()
        {
            try
            {
                Customer = await CustomerService.GetCustomer(int.Parse(CustomerId));
                CustomerProfilePicture = await CustomerDocumentService.GetProfilePicture(int.Parse(CustomerId));
                CustomerDocuments = await CustomerDocumentService.Get(int.Parse(CustomerId));
                Notes = await NoteService.GetNotesByBrokerId(Customer.Id);
            }
            catch
            {
                StatusClass = "alert-danger";
                Message = "Something went wrong getting customer details";
                Saved = false;
            }


            var user = await AccountService.GetUser();
            IsAdmin = user.IsAdmin;
            if (IsAdmin)
            {
                try
                {
                    Brokers = await BrokerService.GetBrokers();
                }
                catch
                {
                    StatusClass = "alert-danger";
                    Message = "Something went wrong getting customer details";
                    Saved = false;
                }
            }

            if (!IsAdmin)
            {
                Chat = await ChatService.Get(Customer.Id);
            }
            else
            {
                if (Customer != null)
                {
                    if (Customer.ConnectedBrokers != null && Customer.ConnectedBrokers.Any())
                    {
                        CustomerBrokers = Brokers.Where(x => Customer.ConnectedBrokers.Contains(x.Id)).ToList();
                    }
                }
            }


        }

        protected async Task ChatBrokerChanged()
        {
            Chat = await ChatService.Get(Customer.Id, BrokerListId);
        }

        protected async Task SendNotificationToSelected()
        {
            await OpenNotificationDialog();
        }

        protected async Task OpenNotificationDialog(bool sendAll = false)
        {

            if (Customer.EmailConfirmed)
            {                
                var dialogParams = new DialogParameters();

                if (string.IsNullOrEmpty(selectedNotification))
                {
                    dialogParams.Add("Message", $"Please enter a notification to send.");
                    await DialogService.Show<AlertDialog>("Send Notification", dialogParams).Result;
                    return;
                }

                if (selectedNotification.Length > 299)
                {
                    dialogParams.Add("Message", $"Your notification is too long. It needs to be less than 300 letters.");
                    await DialogService.Show<AlertDialog>("Send Notification", dialogParams).Result;
                    return;
                }


                dialogParams.Add("Notification", selectedNotification);

                var targetsName = new List<string>();
                targetsName.Add(Customer.Name);

                //var longlist = string.Join(",", targets);

                dialogParams.Add("Users", targetsName);
                dialogParams.Add("areBrokers", false);
                var result = await DialogService.Show<ScrollableDialog>("Send Notification", dialogParams).Result;

                if (!result.Cancelled)
                {
                    var targetsId = new List<int>();
                    targetsId.Add(Customer.Id);
                    var user = await AccountService.GetUser();


                    var succeeded = false;
                    try
                    {
                        succeeded = await NotificationService.SendMessageNotification(selectedNotification, targetsId,user.MasterBrokerId);
                    }
                    catch
                    {

                    }

                    if (succeeded)
                    {
                        AlertService.Alert(new Models.AlertBIQ
                        {
                            AutoClose = true,
                            Message = "Notification Sent"
                        });
                    }
                    else
                    {
                        AlertService.Error("Notification sending failed");
                    };
                }
            }
            else
            {
                var dialogParams = new DialogParameters();
                dialogParams.Add("Message", $"A notification will NOT be sent to {Customer.Name} as they have not confirmed their email address.");
                var result = await DialogService.Show<AlertDialog>("Send Notification", dialogParams).Result;
            }
        }

        protected void NavigateToOverview()
        {
                NavigationManager.NavigateTo($"/clientlist");
        }

        protected async Task DeleteProfilePicture(Guid id)
        {
            var suceeded = false;
            try
            {
                suceeded = await CustomerDocumentService.DeleteProfilePicture(id);
            }
            catch
            {
                StatusClass = "alert-danger";
                Message = "Something went wrong getting insurance details";
                Saved = false;
            }

            if (suceeded)
            {
                StatusClass = "alert-success";
                Message = "Deleted successfully";
            }

            Saved = true;
        }

        protected async Task UploadProfilePicture(string filename, byte[] dataBytes)
        {
            if (Customer == null)
            {
                StatusClass = "alert-danger";
                Message = "No client found";
                Saved = true;
                return;
            }

            CustomerDocument sdoc = new CustomerDocument();
            sdoc.CustomerId = Customer.Id;
            sdoc.FileName = filename;
            sdoc.SupportingDocumentType = DocumentTypeEnum.PNG;
            sdoc.File = dataBytes;

            bool succeeded = false;
            try
            {
                succeeded = await CustomerDocumentService.UploadProfilePicture(sdoc);
            }
            catch
            {

            }

            if (succeeded)
            {
                StatusClass = "alert-success";
                Message = "Uploaded successfully";
            }
            else
            {
                StatusClass = "alert-danger";
                Message = "Something went wrong adding the profile picture. Please try again.";
            }

            Saved = true;
        }

        protected async Task DeleteCustomer()
        {
            var dialogParams = new DialogParameters();
            dialogParams.Add("Message", $"Are you absolutely sure you want to delete this client {Customer.Name}? This is a PERMANENT DELETE and cannot be undone.");
            var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Cancelled)
            {
                var deleted = await CustomerService.DeleteCustomer(Customer.Id);
                if (deleted)
                {
                    var responseParams = new DialogParameters();
                    responseParams.Add("Message", "Deleted successfully");
                    await DialogService.Show<AlertDialog>("Information", responseParams).Result;
                }
                else
                {
                    var responseParams = new DialogParameters();
                    responseParams.Add("Message", "The client did not delete.");
                    await DialogService.Show<AlertDialog>("Information", responseParams).Result;
                }
                NavigationManager.NavigateTo($"/clientlist");
            }
        }


        protected async Task NewNote()
        {
            bool succeeded = false;
            var result = await DialogService.Show<NoteEditDialog>("New Note").Result;
            if(!result.Cancelled)
            {
                var message = result.Data.ToString();

                try
                {
                    if (!string.IsNullOrEmpty(message))
                    {
                       succeeded = (await NoteService.SaveNote(message, Customer.Id)).Id>0;
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
                StatusClass = "alert-success";
                Message = "Note saved successfully";
            }
            else
            {
                StatusClass = "alert-danger";
                Message = "Something went wrong adding the note. Please try again.";
            }

            Saved = true;

        }

        protected async Task EditNote(int id)
        {                
            bool succeeded = false;
            var note = Notes.FirstOrDefault(x => x.Id == id);
            var dialogParams = new DialogParameters();
            dialogParams.Add("Message", note.Message);
            
            var result = await DialogService.Show<NoteEditDialog>("Edit Note", dialogParams).Result;
            if(!result.Cancelled)
            {
                var message = result.Data.ToString();
                try
                {
                    if (!string.IsNullOrEmpty(message))
                    {
                        succeeded = (await NoteService.UpdateNote(message, note.Id)).Id > 0;
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
                StatusClass = "alert-success";
                Message = "Note saved successfully";
            }
            else
            {
                StatusClass = "alert-danger";
                Message = "Something went wrong adding the note. Please try again.";
            }

            Saved = true;

        }

        protected async Task DeleteNote(int id)
        {
            var dialogParams = new DialogParameters();
            dialogParams.Add("Message", $"Are you sure you want to delete this note?");
            var result = await DialogService.Show<ConfirmCancelDialog>("Warning", dialogParams).Result;
            if (!result.Cancelled)
            {
                var deleted = await NoteService.Delete(id);
                if (deleted)
                {
                    var responseParams = new DialogParameters();
                    responseParams.Add("Message", "Deleted successfully");
                    await DialogService.Show<AlertDialog>("Information", responseParams).Result;
                }
                else
                {
                    var responseParams = new DialogParameters();
                    responseParams.Add("Message", "The note did not delete.");
                    await DialogService.Show<AlertDialog>("Information", responseParams).Result;
                }
                NavigationManager.NavigateTo($"/clientlist");
            }
            else
            {
                return;
            }
        }

        protected async Task NewChat()
        {
            bool succeeded = false;
            var result = await DialogService.Show<MessageSendDialog>("Send Message").Result;
            if (!result.Cancelled)
            {
                var message = result.Data.ToString();

                try
                {
                    if (!string.IsNullOrEmpty(message))
                    {
                        succeeded = (await ChatService.Send(message, Customer.Id));
                        var user = await AccountService.GetUser();
                        var notifMessage = $"You have a new chat message from your Broker";
                        succeeded |= await NotificationService.SendMessageNotification(notifMessage, new List<int> { Customer.Id }, user.MasterBrokerId, chat:true);
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
                StatusClass = "alert-success";
                Message = "Message sent successfully";
            }
            else
            {
                StatusClass = "alert-danger";
                Message = "Something went wrong sending the message. Please try again.";
            }

            Saved = true;

        }
    }
}
