using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Online.Server.Components;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Customer.Components
{
    public partial class CustomerConnectionDialog : ComponentBase
    {
        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Inject]
        public ISnackbar Snackbar { get; set; }

        [Inject]
        public ICustomerService CustomerService { get; set; }

        [Inject]
        private IDialogService DialogService { get; set; }

        [Parameter]
        public Online.Models.Customer Customer { get; set; }

        public Online.Models.Customer SelectedCustomer { get; set; }

        private bool IsBusy { get; set; }

        private async Task<IEnumerable<Online.Models.Customer>> Search(string value)
        {
            // if text is null or empty, show complete list
            if (string.IsNullOrWhiteSpace(value) || value.Length < 3)
                return Array.Empty<Online.Models.Customer>();

            var possibleMatches = await CustomerService.Search(Customer.ChosenBrokerId, value);
            possibleMatches = possibleMatches.Where(c => c.Id != Customer.Id);

            return possibleMatches;
        }

        readonly Converter<Online.Models.Customer> CustomerConverter = new Converter<Online.Models.Customer>
        {
            SetFunc = customer => customer is null? string.Empty : $"{customer.FirstName} {customer.LastName} ({customer.EmailAddress})",
            GetFunc = text => null,
        };

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private async Task ConnectToCustomer()
        {
            var parameters = new DialogParameters
            {
                { "ContentText", $"Do you really want to connect to this customer?" },
                { "AdditionalText", $"ALL THE DATA FOR {SelectedCustomer.FirstName} {SelectedCustomer.LastName} WILL BECOME UNAVAILABLE!" },
                { "ButtonText", "Connect" },
                { "Color", Color.Error }
            };

            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };

            var result = await DialogService.Show<ConfirmationDialog>("WARNING", parameters, options).Result;

            if (result.Canceled) return;

            MudDialog.Close(DialogResult.Ok(SelectedCustomer.Id));
        }
    }
}