using BrokerIQ.Online.Server.Helper;
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
            return Get(dateIn).ToString("dd/MM/yyyy h:mm:ss tt");
        }

        public static string ToBiqDateString(this DateTime dateIn)
        {
            return Get(dateIn).ToString("dd/MM/yyyy");
        }

        private static DateTime Get(DateTime dateIn)
        {
            var zone = "GMT Standard Time";

            try
            {
                var sourceUtc = DateTime.SpecifyKind(dateIn, DateTimeKind.Utc);
                var destinationTimezoneId = TimeZoneInfo.FindSystemTimeZoneById(zone);
                var sourceLocalTime = TimeZoneInfo.ConvertTimeFromUtc(sourceUtc, destinationTimezoneId);
                return sourceLocalTime;
            }
            catch
            {

            }

            try
            {
                zone = "Europe/London";
                var sourceUtc = DateTime.SpecifyKind(dateIn, DateTimeKind.Utc);
                var destinationTimezoneId = TimeZoneInfo.FindSystemTimeZoneById(zone);
                var sourceLocalTime = TimeZoneInfo.ConvertTimeFromUtc(sourceUtc, destinationTimezoneId);
                return sourceLocalTime;
            }
            catch
            {

            }

            return dateIn;

        }
    }
}
