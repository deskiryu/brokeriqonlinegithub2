using System;
using System.Collections.Generic;
using System.Linq;

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

    public static class TimeSpanHumanizer
    {
        /// <summary>
        /// Convert a TimeSpan to a human-readable string.
        /// Examples:
        ///   1d 2h 0m 5s -> "1 day and 2 hours"          (maxParts: 2)
        ///   00:00:05     -> "5 seconds"
        ///   -00:01:00    -> "-1 minute"
        ///   02:03:04.500 -> "2 hours, 3 minutes and 4 seconds" (default)
        ///   02:03:04.500 -> "2h 3m" (shortForm: true)
        /// </summary>
        /// <param name="span">The TimeSpan to format.</param>
        /// <param name="maxParts">Max number of units to include (e.g., 2 => “2 hours, 3 minutes”).</param>
        /// <param name="shortForm">If true, returns compact form like “2d 3h 5m”.</param>
        /// <param name="includeMilliseconds">Include ms if no larger units are present or if room remains.</param>
        public static string Humanize(this TimeSpan span, int maxParts = 3, bool shortForm = false, bool includeMilliseconds = false)
        {
            var negative = span.Ticks < 0;
            span = span.Duration();

            var units = new (int value, string singular, string plural, string shortLabel)[]
            {
            (span.Days,        "day",        "days",        "d"),
            (span.Hours,       "hour",       "hours",       "h"),
            (span.Minutes,     "minute",     "minutes",     "m"),
            (span.Seconds,     "second",     "seconds",     "s"),
            };

            var parts = new List<string>();

            foreach (var (value, s, p, sh) in units)
            {
                if (value <= 0) continue;

                parts.Add(shortForm ? $"{value}{sh}" : $"{value} {(value == 1 ? s : p)}");
                if (parts.Count == maxParts) break;
            }

            // Optionally add milliseconds if requested and we still have room (or if everything else was zero)
            if (includeMilliseconds && (parts.Count == 0 || parts.Count < maxParts) && span.Milliseconds > 0)
            {
                var ms = span.Milliseconds;
                parts.Add(shortForm ? $"{ms}ms" : $"{ms} {(ms == 1 ? "millisecond" : "milliseconds")}");
            }

            // Nothing non-zero? Fall back to zero seconds (or ms if requested)
            if (parts.Count == 0)
                parts.Add(shortForm ? (includeMilliseconds ? "0ms" : "0s") : (includeMilliseconds ? "0 milliseconds" : "0 seconds"));

            string text = shortForm
                ? string.Join(" ", parts)
                : parts.Count == 1
                    ? parts[0]
                    : string.Join(", ", parts.Take(parts.Count - 1)) + " and " + parts.Last();

            return negative ? "-" + text : text;
        }
    }
}