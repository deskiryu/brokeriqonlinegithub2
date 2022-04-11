using System;
using System.Collections.Generic;
using System.Text;

namespace BrokerIQ.Online.Server.Extensions
{
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.JSInterop;
    using BrokerIQ.Online.Server.Enumuration;

    public static class Extensions
    {
        public async static Task SaveAs(IJSRuntime js, string filename, byte[] data)
        {
            await js.InvokeAsync<object>(
                "saveAsFile",
                filename,
                Convert.ToBase64String(data));
        }

        public async static Task PreviewFile(IJSRuntime js, MemoryStream memoryStream, bool isJpeg=false)
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

        public async static Task StartWarningTimer(IJSRuntime js, int timeoutIntervalms)
        {
            await js.InvokeAsync<object>(
                "inactivityTime", timeoutIntervalms);
        }

        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .FirstOrDefault()?.GetCustomAttribute<DisplayAttribute>().GetName() ?? "";
        }

        public static TimeSpan TransformToTS(this RecentPeriodEnum rpe) => rpe switch
        {
            RecentPeriodEnum.TwoWeeks => new TimeSpan(14,0,0,0),
            RecentPeriodEnum.FourWeeks => new TimeSpan(28, 0, 0, 0),
            RecentPeriodEnum.ThreeMonths => new TimeSpan(90, 0, 0, 0),
            RecentPeriodEnum.SixMonths => new TimeSpan(180, 0, 0, 0),
            _ => new TimeSpan(14, 0, 0, 0)

        };

        public async static Task ChangeIcon(IJSRuntime js, string location)
        {
                await js.InvokeAsync<object>(
                    "changeTabIcon",
                    location);
        }
    }
}
