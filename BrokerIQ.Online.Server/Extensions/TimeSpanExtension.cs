using System;

namespace BrokerIQ.Online.Server.Extensions
{
    public static class TimeSpanExtension
    {
        public static string ToShortDisplayValue(this TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero) return "Not set";

            if (timeSpan >= TimeSpan.FromDays(364))
            {
                var year = (int)((timeSpan.TotalDays / 365.2425)+0.5);

                var spanUnit = year > 1 ? "years" : "year";

                return $"{year} {spanUnit}";
            }
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
            if (timeSpan == TimeSpan.FromDays(365*5)) return "5 years before review date";
            if (timeSpan == TimeSpan.FromDays(365*4)) return "4 years before review date";
            if (timeSpan == TimeSpan.FromDays(365*3)) return "3 years before review date";
            if (timeSpan == TimeSpan.FromDays(365*2)) return "2 years before review date";
            if (timeSpan == TimeSpan.FromDays(365)) return "1 year before review date";

            if (timeSpan >= TimeSpan.FromDays(30))
            {
                var numberOfMonths = (timeSpan.TotalDays / 30);
                var spanUnit = numberOfMonths > 1 ? "months" : "month";

                return $"{timeSpan.TotalDays} days ({numberOfMonths} {spanUnit}) before review date";
            }

            if (timeSpan >= TimeSpan.FromDays(1))
            {
                var spanUnit = timeSpan.TotalDays > 1 ? "days" : "day";

                return $"{timeSpan.TotalDays} {spanUnit} before review date";
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