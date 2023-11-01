using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Online.Server.Services.Interface;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Customer.Components
{
    public partial class AssignmentDialog : ComponentBase
    {
        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public int BrokerId { get; set; }

        [Parameter]
        public IEnumerable<int> SelectedCustomerIds { get; set; }

        [Inject]
        public ISnackbar Snackbar { get; set; }

        [Inject]
        private IBrokerStaffService _brokerStaffService { get; set; }

        [Inject]
        private IAssignmentService _assignmentService { get; set; }

        protected MudSelect<Online.Models.BrokerStaff> StaffSelectCtl;

        public IEnumerable<Online.Models.BrokerStaff> AssignableStaff { get; set; }

        public Online.Models.BrokerStaff SelectedStaff { get; set; }

        protected override async Task OnInitializedAsync()
        {
            AssignableStaff = Array.Empty<Online.Models.BrokerStaff>();

            AssignableStaff = (await _brokerStaffService.GetBrokerStaffbyBrokerId(BrokerId))
                .Where(s => s.StaffTypeId == Dto.Enum.StaffTypeEnum.Admin || s.StaffTypeId == Dto.Enum.StaffTypeEnum.Advisor)
                .ToArray();
        }

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        protected bool NoStaffSelected
        {
            get
            {
                return StaffSelectCtl != null && !StaffSelectCtl.SelectedValues.Any();
            }
        }

        private async Task AssignToEmployee()
        {
            try
            {
                await _assignmentService.Assign(SelectedStaff, SelectedCustomerIds);

                Snackbar.Add("Assignment was successfull", Severity.Success);
            }
            catch
            {
                Snackbar.Add("Unable to assign. Please try again", Severity.Error);
            }

            MudDialog.Close();
        }
    }
}