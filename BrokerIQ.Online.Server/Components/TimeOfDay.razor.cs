using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BrokerIQ.Online.Server.Components;

public partial class TimeOfDay : ComponentBase
{
    [Parameter]
    public bool Disabled { get; set; } = false;

    [Parameter]
    public bool Visible { get; set; } = true;

    [Parameter]
    public TimeSpan? Value { get; set; }

    [Parameter]
    public EventCallback<TimeSpan?> ValueChanged { get; set; }

    [Parameter]
    public Action<TimeSpan?> TimeChanged { get; set; }

    MudSelect<string> ctlHour;
    MudSelect<string> ctlMinute;

    string HourAsString { get; set; }
    string MinuteAsString { get; set; }

    protected string VisibilityClass => Visible ? string.Empty : "d-none";

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        if (!Value.HasValue) return;

        HourAsString = Value.Value.Hours.ToString("00");
        MinuteAsString = Value.Value.Minutes.ToString("00");
    }

    private async Task UpdateValue()
    {
        var hour = HourAsString = ctlHour.Value;
        var minute = MinuteAsString = ctlMinute.Value;

        if (string.IsNullOrWhiteSpace(HourAsString) || string.IsNullOrWhiteSpace(MinuteAsString)) Value = null;

        var parsed = TimeSpan.TryParse($"{hour}:{minute}", out TimeSpan parsedTime);

        Value = parsed ? parsedTime : null;

        if (TimeChanged is not null) TimeChanged(Value);

        await ValueChanged.InvokeAsync(Value);
    }
}