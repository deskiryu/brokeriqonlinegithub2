using System;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Dto.Enum;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Pages.Customer.Components;

public partial class QuoteButton
{
    [Inject]
    public IDialogService DialogService { get; set; }

    [Parameter]
    public Online.Models.Broker Broker { get; set; }

    [Parameter]
    public Online.Models.Customer Customer { get; set; }

    [Parameter]
    public Func<Task> PostQuoteFunction  { get; set; }

    protected async Task GetInsuranceQuote()
    {
        var parameters = new DialogParameters
        {
            { "Customer", Customer }
        };

        var options = new DialogOptions() { MaxWidth = MaxWidth.Small, FullWidth = true };

        var result = await DialogService.Show<IncomeProtectionQuoteDialog>("Income Protection Quote", parameters, options).Result;

        if (!result.Canceled && PostQuoteFunction is not null)
        {
            await PostQuoteFunction();
        }
    }

    protected bool CanShowControl()
    {
        if (Broker == null || !Broker.HasActiveInsuranceQuoteSubscription) return false;

        if (Customer.HasNeeds) return true;

        return (Customer.Employment == EmploymentEnum.Employed || Customer.Employment == EmploymentEnum.SelfEmployed) &&
            !Customer.Insurances.Any(i => i.InsType == InsuranceEnum.Income && i.ExpiryDate > DateTime.UtcNow);
    }
}