using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Server.Extensions
{
    public static class TimeSpanExtension
    {
        public static string ToDisplayValue(this TimeSpan timeSpan)
        {
            if (timeSpan.Days > 0) return BuildDayString(timeSpan);
            if (timeSpan.Days > 0) return BuildHourString(timeSpan);
            if (timeSpan.Days > 0) return BuildMinuteString(timeSpan);

            return "No interval defined";
        }

        private static string BuildDayString(TimeSpan timeSpan)
        {
            return $"{timeSpan.Days} days before final date";
        }

        private static string BuildHourString(TimeSpan timeSpan)
        {
            return $"{timeSpan.Hours} hours before final date";
        }

        private static string BuildMinuteString(TimeSpan timeSpan)
        {
            return $"{timeSpan.Minutes} minutes before final date";
        }
    }
}