using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Server.Extensions
{
    public static class DateTimeConverter
    {
        public static string ToBiqDateTimeString( this DateTime dateIn)
        {
            var sourceUtc = DateTime.SpecifyKind(dateIn, DateTimeKind.Utc);
            var destinationTimezoneId = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            var sourceLocalTime = TimeZoneInfo.ConvertTimeFromUtc(sourceUtc, destinationTimezoneId);
            return sourceLocalTime.ToString("dd/MM/yyyy hh:mm:ss");
        }

        public static string ToBiqDateString(this DateTime dateIn)
        {
            var sourceUtc = DateTime.SpecifyKind(dateIn, DateTimeKind.Utc);
            var destinationTimezoneId = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            var sourceLocalTime = TimeZoneInfo.ConvertTimeFromUtc(sourceUtc, destinationTimezoneId);
            return sourceLocalTime.ToString("dd/MM/yyyy");
        }
    }
}
