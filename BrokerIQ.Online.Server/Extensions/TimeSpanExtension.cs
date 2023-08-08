using System;

namespace BrokerIQ.Online.Server.Extensions
{
    public static class TimeSpanExtension
    {
        public static string ToDisplayValue(this TimeSpan timeSpan)
        {
            if (timeSpan.Days > 0) return timeSpan.Days == 1 ? $"1 day" : $"{timeSpan.Days} days";

            return "No interval defined";
        }
    }
}