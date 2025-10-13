using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace BrokerIQ.Online.Server.Shared;

public partial class TimeControl : ComponentBase
{
    private const string MINUTES = "Minutes";
    private const string HOURS = "Hours";
    private const string DAYS = "Days";

    private int _amount { get; set; }

    private string _unit = MINUTES;

    private string? _lastValue;

    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }

    protected override void OnParametersSet()
    {
        if (_lastValue == Value)
        {
            return;
        }

        _lastValue = Value;
        if (Value != null && TimeSpan.TryParse(Value, out var ts))
        {
            if (ts.TotalDays >= 1 && ts.TotalDays % 1 == 0)
            {
                _unit = "Days";
                _amount = (int)ts.TotalDays;
            }
            else if (ts.TotalHours >= 1 && ts.TotalHours % 1 == 0)
            {
                _unit = "Hours";
                _amount = (int)ts.TotalHours;
            }
            else
            {
                _unit = "Minutes";
                _amount = (int)ts.TotalMinutes;
            }
        }
        else
        {
            _unit = "Minutes";
            _amount = 0;
        }
    }

    private Task OnAmountChanged(int value)
    {
        _amount = value;
        return UpdateValue();
    }

    private Task OnUnitChanged(string value)
    {
        _unit = value;
        return UpdateValue();
    }

    private Task UpdateValue()
    {
        var ts = _unit switch
        {
            MINUTES => TimeSpan.FromMinutes(_amount),
            HOURS => TimeSpan.FromHours(_amount),
            DAYS => TimeSpan.FromDays(_amount),
            _ => TimeSpan.Zero
        };
        Value = ts.ToString();

        return ValueChanged.InvokeAsync(Value);
    }
}
