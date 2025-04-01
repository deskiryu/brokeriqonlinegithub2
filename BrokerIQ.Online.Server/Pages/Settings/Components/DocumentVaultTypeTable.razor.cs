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
    public partial class DocumentVaultTypeTable : ComponentBase
    {
        [Inject]
        private IDialogService DialogService { get; set; }

        [Inject]
        private ISnackbar Snackbar { get; set; }

        [Inject]
        public IDocumentVaultTypeService DocumentVaultTypeService { get; set; }

        [Parameter]
        public User User { get; set; }

        [Parameter]
        public Online.Models.Broker Broker { get; set; }

        private ICollection<DocumentVaultTypeDto> DocumentVaultTypes { get; set; }

        protected override async Task OnInitializedAsync()
        {
            DocumentVaultTypes = new List<DocumentVaultTypeDto>(await DocumentVaultTypeService.GetAllForBroker(Broker.Id));
        }

        private async Task RemoveDocumentVaultType(DocumentVaultTypeDto vaultType)
        {
            var parameters = new DialogParameters
            {
                { "ContentText", "Do you really want to delete these Document Vault type? This process cannot be undone." },
                { "ButtonText", "Delete" },
                { "Color", Color.Error }
            };

            var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };

            var result = await DialogService.Show<ConfirmationDialog>("Delete", parameters, dialogOptions).Result;

            if (!result.Cancelled)
            {
                await DocumentVaultTypeService.Delete(vaultType);
            }

            await ReloadDocumentVaultTypes();
        }

        private async Task ReloadDocumentVaultTypes()
        {
            DocumentVaultTypes = new List<DocumentVaultTypeDto>(await DocumentVaultTypeService.GetAllForBroker(Broker.Id));

            StateHasChanged();
        }

        private async Task EditDocumentVaultType(DocumentVaultTypeDto vaultType)
        {
            var operation = vaultType.Id == 0 ? "Create" : "Edit";
            var title = $"{operation} {vaultType.Name} vault type";
            var parameters = new DialogParameters
            {
                { "VaultType", vaultType }
            };

            var options = new DialogOptions() { MaxWidth = MaxWidth.Small, FullWidth = true };

            var result = await DialogService.Show<DocumentVaultTypeDialog>(title, parameters, options).Result;

            if (!result.Canceled)
            {
                DocumentVaultTypeDto updated = result.Data as DocumentVaultTypeDto;

                bool wasSuccessfull;

                if (vaultType.Id == 0)
                {
                    wasSuccessfull = await DocumentVaultTypeService.Create(new CreateDocumentVaultTypeDto()
                    {
                        BrokerId = updated.BrokerId,
                        Name = updated.Name,
                        AdditionalDetail = updated.AdditionalDetail
                    });
                }
                else
                {
                    wasSuccessfull = await DocumentVaultTypeService.Update(new UpdateDocumentVaultTypeDto()
                    {
                        Id = updated.Id,
                        BrokerId = updated.BrokerId,
                        Name = updated.Name,
                        AdditionalDetail = updated.AdditionalDetail
                    });
                }

                var message = wasSuccessfull ? "Docuvault type was saved." : "Docuvault type save failed. Please try again.";
                Snackbar.Add(message, wasSuccessfull ? Severity.Success : Severity.Error);
            }

            await ReloadDocumentVaultTypes();
        }
    }
}