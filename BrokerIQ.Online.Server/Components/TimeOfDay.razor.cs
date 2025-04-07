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

    protected string VisibilityClass => Visible ? string.Empty : "d-none";

    private async Task UpdateValue()
    {
        var hour = ctlHour.Value;
        var minute = ctlMinute.Value;

        if (string.IsNullOrWhiteSpace(hour) || string.IsNullOrWhiteSpace(minute)) Value = null;

        var parsed = TimeSpan.TryParse($"{hour}:{minute}", out TimeSpan parsedTime);

        Value = parsed ? parsedTime : null;

        if (TimeChanged is not null) TimeChanged(Value);

        await ValueChanged.InvokeAsync(Value);
    }
}