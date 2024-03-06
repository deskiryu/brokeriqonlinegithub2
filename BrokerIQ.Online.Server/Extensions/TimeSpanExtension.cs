using System;

namespace BrokerIQ.Online.Server.Extensions
{
    public static class TimeSpanExtension
    {
        public static string ToShortDisplayValue(this TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero) return "Not set";

            if (timeSpan >= TimeSpan.FromDays(1))
            {
                var spanUnit = timeSpan.TotalDays > 1 ? "days" : "day";

                return $"{timeSpan.TotalDays} {spanUnit}";
            }

            if (timeSpan >= TimeSpan.FromHours(1))
            {
                var spanUnit = timeSpan.TotalDays > 1 ? "hours" : "hour";

                return $"{timeSpan.TotalHours} {spanUnit}";
            }

            if (timeSpan >= TimeSpan.FromMinutes(1))
            {
                return $"{timeSpan.TotalMinutes} minutes";
            }

            return "No interval defined";
        }

        public static string ToDisplayValue(this TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero) return "Not set";

            if (timeSpan == TimeSpan.FromDays(365)) return "365 days (1 year) before final date";

            if (timeSpan >= TimeSpan.FromDays(30))
            {
                var numberOfMonths = (timeSpan.TotalDays / 30);
                var spanUnit = numberOfMonths > 1 ? "months" : "month";

                return $"{timeSpan.TotalDays} days ({numberOfMonths} {spanUnit}) before final date";
            }

            if (timeSpan >= TimeSpan.FromDays(1))
            {
                var spanUnit = timeSpan.TotalDays > 1 ? "days" : "day";

                return $"{timeSpan.TotalDays} {spanUnit} before final date";
            }

            if (timeSpan >= TimeSpan.FromHours(1))
            {
                var spanUnit = timeSpan.TotalDays > 1 ? "hours" : "hour";

                return $"{timeSpan.TotalHours} {spanUnit}  previous";
            }

            if (timeSpan >= TimeSpan.FromMinutes(1))
            {
                return $"{timeSpan.TotalMinutes} minutes previous";
            }

            return "No interval defined";
        }
    }
}