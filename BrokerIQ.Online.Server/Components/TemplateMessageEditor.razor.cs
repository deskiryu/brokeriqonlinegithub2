using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Services.Interface;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Components;

public partial class TemplateMessageEditor : ComponentBase
{
    [Inject]
    public IMapper Mapper { get; set; }

    [Inject]
    public IBrokerDefinedMessageService BrokerDefinedMessageService { get; set; }

    [Parameter]
    public Broker Broker { get; set; }

    [Parameter]
    public Customer Customer { get; set; }

    [Parameter]
    public Customer Connection { get; set; }

    [Parameter]
    public BrokerDefinedMessage DefinedMessage { get; set; }

    [Parameter]
    public EventCallback<BrokerDefinedMessage> DefinedMessageChanged { get; set; }

    [Parameter]
    public string MessagePreview { get; set; }

    [Parameter]
    public EventCallback<string> MessagePreviewChanged { get; set; }

    public List<BrokerDefinedMessage> MergedMessages = new();

    public DateTime? SelectedTemplateDateReplacement { get; set; }

    public TimeSpan? SelectedTemplateTimeReplacement { get; set; }

    public bool ShowInsertDate { get; set; }

    public bool ShowInsertTime { get; set; }

    public bool ShowTemplatePdf { get; set; }

    public string TemplatePdfName { get; set; }

    public MudAutocomplete<BrokerDefinedMessage> TemplateAutoComplete { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await PopulateBrokerDefinedMessages();
    }

    private async Task PopulateBrokerDefinedMessages()
    {
        MergedMessages = new List<BrokerDefinedMessage>();
        var templates = Mapper.Map<List<BrokerDefinedMessage>>(await BrokerDefinedMessageService.GetAllForCurrentBroker());

        foreach (var template in templates)
        {
            // if (!string.IsNullOrWhiteSpace(template.Message))
            // {
            //     // display broker defined message
            //     template.Message = template.Message
            //         .Replace("INSERT_CLIENT_NAME", Connection != null ? $"{Customer.FirstName} and {Connection.FirstName}" : Customer.FirstName)
            //         .Replace("INSERT_BROKER_NAME", $"{Broker?.BrokerFirstName} {Broker?.BrokerLastName}")
            //         .Replace("INSERT_COMPANY_NAME", Broker?.Name);
            // }

            if (!template.WelcomeChat)
            {
                MergedMessages.Add(template);
            }
        }
    }

    protected void OnComboValueChanged(string itemResponse)
    {
        SelectedTemplateTimeReplacement = null;
        SelectedTemplateDateReplacement = null;
        MessagePreview = string.Empty;

        if (string.IsNullOrEmpty(itemResponse))
        {
            return;
        }

        DefinedMessage = MergedMessages.FirstOrDefault(mm => mm.Prompt.ToLower().Contains(itemResponse.ToLower()));
        if (DefinedMessage != null)
        {
            MessagePreview = DefinedMessage.Message;
            // ShowInsertDate = DefinedMessage.Message.Contains("INSERT_DATE");
            // ShowInsertTime = DefinedMessage.Message.Contains("INSERT_TIME");
            ShowTemplatePdf = !string.IsNullOrEmpty(DefinedMessage.FileName);
            TemplatePdfName = !string.IsNullOrEmpty(DefinedMessage.FileName) ? DefinedMessage.FileName : "";
        }

        DefinedMessageChanged.InvokeAsync(DefinedMessage);
        MessagePreviewChanged.InvokeAsync(MessagePreview);
    }

    protected async Task<IEnumerable<BrokerDefinedMessage>> OnTemplateFilter(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return MergedMessages;

        return MergedMessages.Where(mm => mm.Prompt.ToLower().Contains(value.ToLower())).ToArray();
    }

    // protected void InsertDateToTemplateMessage(DateTime? dateIn)
    // {
    //     SelectedTemplateDateReplacement = dateIn;
    //     UpdatePreviewWithTimes();
    // }

    // protected void InsertTimeToTemplateMessage(TimeSpan? timeIn)
    // {
    //     SelectedTemplateTimeReplacement = timeIn;
    //     UpdatePreviewWithTimes();
    // }

    // private void UpdatePreviewWithTimes()
    // {
    //     if (DefinedMessage == null)
    //     {
    //         return;
    //     }

    //     MessagePreview = DefinedMessage.Message;
    //     if (DefinedMessage.Message.Contains("INSERT_DATE") && SelectedTemplateDateReplacement.HasValue)
    //     {
    //         MessagePreview = MessagePreview.Replace("INSERT_DATE", SelectedTemplateDateReplacement.Value.ToBiqDateString());
    //     }

    //     if (DefinedMessage.Message.Contains("INSERT_TIME") && SelectedTemplateTimeReplacement.HasValue)
    //     {
    //         MessagePreview = MessagePreview.Replace("INSERT_TIME", SelectedTemplateTimeReplacement.Value.ToBiqTimeString());
    //     }

    //     MessagePreviewChanged.InvokeAsync(MessagePreview);
    // }

    protected async Task ClearAutoComplete()
    {
        await TemplateAutoComplete.Clear();
        SelectedTemplateDateReplacement = null;
        SelectedTemplateTimeReplacement = null;
        ShowTemplatePdf = false;
        TemplatePdfName = string.Empty;
        ShowInsertDate = false;
        ShowInsertTime = false;

        DefinedMessage = new BrokerDefinedMessage();
        await DefinedMessageChanged.InvokeAsync(DefinedMessage);

        MessagePreview = string.Empty;
        await MessagePreviewChanged.InvokeAsync(MessagePreview);
    }
    private void OnPreviewMessageBlur(Microsoft.AspNetCore.Components.Web.FocusEventArgs args)
    {
        MessagePreviewChanged.InvokeAsync(MessagePreview);
    }
}