using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Dto;
using BrokerIQ.Dto.CreateDto;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Server.Components;
using BrokerIQ.Online.Server.Models;
using BrokerIQ.Online.Server.Shared;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Customer.Components
{
    public partial class DocumentRequirementCard : ComponentBase
    {
        [Parameter]
        public Online.Models.Broker Broker { get; set; }

        [Parameter]
        public Online.Models.Customer Customer { get; set; }

        [Parameter]
        public Action OnRequirementsChange { get; set; }

        [Inject]
        public IDialogService DialogService { get; set; }

        [Inject]
        public IDocumentVaultTypeService DocumentVaultTypeService { get; set; }

        [Inject]
        public IDocumentsRequirementService DocumentsRequirementService { get; set; }

        public IEnumerable<DocumentVaultTypeDto> DocumentTypeValues = Array.Empty<DocumentVaultTypeDto>();

        public DocumentsRequirement DocumentsRequirement { get; set; }

        public Dictionary<int, int> RequestedDocuments = new Dictionary<int, int>();

        protected override async Task OnInitializedAsync()
        {
            await SetupDocumentRequirements();
        }

        private async Task SetupDocumentRequirements()
        {
            DocumentTypeValues = await DocumentVaultTypeService.GetAllForBroker(Broker.Id);

            DocumentsRequirement = await DocumentsRequirementService.Get(Customer.Id);

            InitialiseRequestedDocuments();
        }

        private void InitialiseRequestedDocuments()
        {
            RequestedDocuments.Clear();
            foreach (var type in DocumentTypeValues)
            {
                RequestedDocuments.Add(type.Id, 0);
            }
        }

        protected async Task SubmitDocumentRequirements()
        {
            if (!RequestedDocuments.Any(rd => rd.Value > 0))
            {
                var alertParams = new DialogParameters
                {
                    { "Message", "No document requirements have been set." }
                };

                await DialogService.Show<AlertDialog>("Information", alertParams).Result;

                return;
            }

            List<CreateDocumentsCheckDto> documentsRequiredList = new List<CreateDocumentsCheckDto>();

            string requirementsString = string.Empty;
            foreach (var req in RequestedDocuments)
            {
                if (req.Value < 1) continue;

                CreateDocumentsCheckDto requirement = new CreateDocumentsCheckDto
                {
                    DocuVaultType = req.Key,
                    RequiredCount = req.Value
                };
                documentsRequiredList.Add(requirement);

                requirementsString += $"{DocumentTypeValues.FirstOrDefault(t => t.Id == req.Key).Name}: {req.Value}\n";
            }

            var dialogParams = new DialogParameters
                {
                    { "ContentText", $"Are you sure you want to set the document requirements as the following?" },
                    { "AdditionalText", requirementsString},
                    { "ButtonText", "Confirm"}
                };
            var result = await DialogService.Show<ConfirmationDialog>("Warning", dialogParams).Result;

            if (result.Canceled) return;

            if (DocumentsRequirement != null)
            {
                var updatedChecks = new List<DocumentsCheckDto>();
                foreach (var check in documentsRequiredList)
                {
                    updatedChecks.Add(new DocumentsCheckDto()
                    {
                        DocumentsRequirementId = DocumentsRequirement.Id,
                        DocuVaultType = check.DocuVaultType,
                        RequiredCount = check.RequiredCount,
                    });
                }

                DocumentsRequirement = await DocumentsRequirementService.Update(DocumentsRequirement.Id, updatedChecks);
            }
            else
            {
                DocumentsRequirement = await DocumentsRequirementService.Create(Customer.Id, documentsRequiredList);
            }

            OnRequirementsChange();

            StateHasChanged();

        }

        protected void EditDocumentRequirements()
        {
            foreach (var document in DocumentsRequirement.DocumentChecks)
            {
                RequestedDocuments[document.DocuVaultType] = document.RequiredCount;
            }

            DocumentsRequirement = null;

            StateHasChanged();
        }

        protected async Task DeleteDocumentRequirements()
        {
            var dialogParams = new DialogParameters
            {
                { "Message", $"Are you sure you want to delete the document requirements currently set?" }
            };
            var result = await DialogService.Show<Server.Shared.ConfirmCancelDialog>("Warning", dialogParams).Result;

            if (!result.Canceled)
            {
                await DocumentsRequirementService.Delete(DocumentsRequirement.Id);

                DocumentsRequirement = null;

                InitialiseRequestedDocuments();

                StateHasChanged();
            }
        }
    }
}