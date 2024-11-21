using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerIQ.Dto.Models;
using BrokerIQ.Dto.UpdateDto;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Server.Components;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Settings.Components
{
    public partial class WealthTypeTable : ComponentBase
    {
        [Inject]
        private IDialogService DialogService { get; set; }

        [Inject]
        private ISnackbar Snackbar { get; set; }

        [Inject]
        public IWealthTypeService WealthTypeService { get; set; }

        [Parameter]
        public User User { get; set; }

        [Parameter]

        public Online.Models.Broker Broker { get; set; }

        private ICollection<WealthTypeDto> WealthTypes { get; set; }

        protected override async Task OnInitializedAsync()
        {
            WealthTypes = new List<WealthTypeDto>(await WealthTypeService.GetAllForBroker(Broker.Id));
        }

        private async Task RemoveWealthType(WealthTypeDto wealthType)
        {
            var parameters = new DialogParameters
            {
                { "ContentText", "Do you really want to delete this Wealth type? This process cannot be undone." },
                { "ButtonText", "Delete" },
                { "Color", Color.Error }
            };

            var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };

            var result = await DialogService.Show<ConfirmationDialog>("Delete", parameters, dialogOptions).Result;

            if (!result.Cancelled)
            {
                await WealthTypeService.Delete(wealthType);
            }

            await ReloadWealthTypes();
        }

        private async Task ReloadWealthTypes()
        {
            WealthTypes = new List<WealthTypeDto>(await WealthTypeService.GetAllForBroker(Broker.Id));

            StateHasChanged();
        }

        private async Task EditWealthType(WealthTypeDto wealthType)
        {
            var operation = wealthType.Id == 0 ? "Create" : "Edit";
            var title = $"{operation} {wealthType.Name} wealth type";
            var parameters = new DialogParameters
            {
                { "WealthType", wealthType }
            };

            var options = new DialogOptions() { MaxWidth = MaxWidth.Small, FullWidth = true };

            var result = await DialogService.Show<WealthTypeDialog>(title, parameters, options).Result;

            if (!result.Cancelled)
            {
                WealthTypeDto updated = result.Data as WealthTypeDto;

                var wasSuccessfull = false;
                if (wealthType.Id == 0)
                {
                    wasSuccessfull = await WealthTypeService.Create(new CreateWealthTypeDto()
                    {
                        BrokerId = updated.BrokerId,
                        Name = updated.Name
                    });
                }
                else
                {
                    wasSuccessfull = await WealthTypeService.Update(new UpdateWealthTypeDto()
                    {
                        Id = updated.Id,
                        BrokerId = updated.BrokerId,
                        Name = updated.Name
                    });
                }

                // TODO : Re introduce these when a fix for the parsing error has been found
                // if (wasSuccessfull)
                // {
                //     Snackbar.Add("Reminder option was removed.", Severity.Success);
                // }
                // else
                // {
                //     Snackbar.Add("Reminder options update failed. Please try again.", Severity.Error);
                // } 
            }

            await ReloadWealthTypes();
        }
    }
}