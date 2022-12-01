using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokerIQ.Online.Server.Extensions
{
    public static class TelephoneConvertor
    {

        public static string GetFormattedPhoneNumber(this string rawPhoneNumber)
        {
            // remove characters from number that WhatsApp doesn't process
            string formattedNumber = rawPhoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            // already in correct format
            if (formattedNumber.StartsWith("+"))
            {
                return formattedNumber;
            }

            // should already be valid number, just replace 00 with +
            if (formattedNumber.StartsWith("00"))
            {
                return formattedNumber.Replace("00", "+");
            }

            // if user's device is not in supported region, we can't assume the correct country code, return null to prompt error
            string countryCode = "44";
            if (countryCode == null)
            {
                return null;
            }

            if (formattedNumber.StartsWith("0"))
            {
                // strip leading 0 from number and prepend country code
                formattedNumber = $"+{countryCode}{formattedNumber.Substring(1)}";
            }
            else
            {
                formattedNumber = $"+{countryCode}{formattedNumber}";
            }

            return formattedNumber;
        }
    }
}
