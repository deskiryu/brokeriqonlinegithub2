using System;

namespace BrokerIQ.Online.Server.Extensions
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using BrokerIQ.Dto.Enum;
    using Microsoft.JSInterop;

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
            _ => new TimeSpan(14, 0, 0, 0)

        };

        public async static Task ChangeIcon(IJSRuntime js, string location)
        {
            await js.InvokeAsync<object>(
                "changeTabIcon",
                location);
        }

        public static CustomerCategoryEnum[] BuildCustomerCategoriesByRelevance()
        {
            var result = new CustomerCategoryEnum[]{
                CustomerCategoryEnum.None,
                CustomerCategoryEnum.NewProspect
            };

            var remainingValues = Enum.GetValues(typeof(CustomerCategoryEnum))
                .Cast<CustomerCategoryEnum>()
                .Except(result);

            return result.Concat(remainingValues).ToArray();
        }


        public static bool IsSystemMessage(this BrokerDefinedMessageEnum message)
        {
            var systemMessages = new BrokerDefinedMessageEnum[] {
                        BrokerDefinedMessageEnum.FirstLoginMessage,
                        BrokerDefinedMessageEnum.FirstLoginDelayMessage,
                        BrokerDefinedMessageEnum.FirstDocumentRequirementsMessage,
                        BrokerDefinedMessageEnum.ProgressDocumentRequirementsMessage,
                        BrokerDefinedMessageEnum.CompleteDocumentRequirementsMessage
            };

            return systemMessages.Contains(message);
        }

        public static int GetOrderValue(this Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .FirstOrDefault()?.GetCustomAttribute<DisplayAttribute>().GetOrder() ?? 0;
        }

    }
}
