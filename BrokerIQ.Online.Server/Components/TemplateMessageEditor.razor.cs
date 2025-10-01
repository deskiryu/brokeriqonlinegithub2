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

        MergedMessages.AddRange(templates.Where(t => !t.WelcomeChat));
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

    private static bool MessageContainsDateMarkers(BrokerDefinedMessage message)
    {
        return message.Message.Contains("INSERT_DATE") || message.Message.Contains("INSERT_TIME");
    }
}