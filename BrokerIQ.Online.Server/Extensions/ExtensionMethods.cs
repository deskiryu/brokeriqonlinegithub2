using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.JSInterop;

using BrokerIQ.Dto.Enum;

namespace BrokerIQ.Online.Server.Extensions
{
    public static class Extensions
    {
        public async static Task SaveAs(IJSRuntime js, string filename, byte[] data)
        {
            await js.InvokeAsync<object>(
                "saveAsFile",
                filename,
                Convert.ToBase64String(data));
        }

        public async static Task NavigateToNewTab(IJSRuntime js, string url)
        {
            await js.InvokeAsync<object>("open", url, "_blank");
        }

        public async static Task PreviewFile(IJSRuntime js, MemoryStream memoryStream, bool isJpeg = false)
        {
            var converted = Convert.ToBase64String(memoryStream.ToArray());

            if (isJpeg)
            {
                await js.InvokeAsync<object>(
                    "openInTabJpeg",
                    converted);
            }
            else
            {
                await js.InvokeAsync<object>(
                    "openInTab",
                    converted);
            }
        }

        public async static Task PreviewFileText(IJSRuntime js, string htmlIn)
        {
                await js.InvokeAsync<object>(
                    "openInTabText",
                    htmlIn);
        }

        public async static Task OpenLinkInNewTab(IJSRuntime js, string url)
        {
            await js.InvokeVoidAsync("open", url, "_blank");
        }

        public async static Task StartWarningTimer(IJSRuntime js, int timeoutIntervalms)
        {
            if (timeoutIntervalms <= 0)
            {
                //default
                timeoutIntervalms = 900000; //15 mins
            }
            await js.InvokeAsync<object>(
                "inactivityTime", timeoutIntervalms);
        }

        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .FirstOrDefault()?.GetCustomAttribute<DisplayAttribute>().GetName() ?? "";
        }

        public static string GetDisplayPrompt(this Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .FirstOrDefault()?.GetCustomAttribute<DisplayAttribute>().GetPrompt() ?? "";
        }

        public static TimeSpan TransformToTS(this RecentPeriodEnum rpe) => rpe switch
        {
            RecentPeriodEnum.TwoWeeks => new TimeSpan(14, 0, 0, 0),
            RecentPeriodEnum.FourWeeks => new TimeSpan(28, 0, 0, 0),
            RecentPeriodEnum.ThreeMonths => new TimeSpan(90, 0, 0, 0),
            RecentPeriodEnum.SixMonths => new TimeSpan(180, 0, 0, 0),
            RecentPeriodEnum.NineMonths => new TimeSpan(270, 0, 0, 0),
            RecentPeriodEnum.TwelveMonths => new TimeSpan(365, 0, 0, 0),
            _ => new TimeSpan(14, 0, 0, 0)

        };

        public async static Task ChangeIcon(IJSRuntime js, string location)
        {
            await js.InvokeAsync<object>(
                "changeTabIcon",
                location);
        }

        public static CustomerCategoryEnum[] GetAllCustomerCategories()
        {
            return GetFilteredCustomerCategories(new int[] { 0, 1, 2 });
        }

        public static CustomerCategoryEnum[] GetFilteredCustomerCategories(int[] toInclude)
        {
            return Enum.GetValues(typeof(CustomerCategoryEnum))
               .Cast<CustomerCategoryEnum>()
               .Where(e => toInclude.Contains(e.GetOrderValue()))
               .OrderBy(e => e.GetOrderValue())
               .ToArray();
        }

        public static int GetOrderValue(this Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .FirstOrDefault()?.GetCustomAttribute<DisplayAttribute>().GetOrder() ?? 0;
        }

        public static bool IsValidUrl(this string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri validatedUri)) //.NET URI validation.
            {
                //If true: validatedUri contains a valid Uri. Check for the scheme in addition.
                return validatedUri.Scheme == Uri.UriSchemeHttp || validatedUri.Scheme == Uri.UriSchemeHttps;
            }

            return false;
        }

        public static string FormatForMobileNotification(this string message)
        {
            return message.Replace("<--", "").Replace("-->", "").Trim();
        }
    }
}
