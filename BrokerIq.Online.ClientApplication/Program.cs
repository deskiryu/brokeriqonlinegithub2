using System;
using System.Net.Http;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using BrokerIQ.Online.AppSettings;
using BrokerIQ.Online.Server.AppSettings;
using BrokerIQ.Online.Server.Extensions;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace BrokerIQ.Online.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.Services.AddMudServices();
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddMudBlazorDialog();

            builder.Services.AddBlazoredSessionStorage();

            builder.Services.AddBlazoredLocalStorage();

            builder.Services.Configure<ReviewItAPIDetails>(builder.Configuration.GetSection(typeof(ReviewItAPIDetails).Name));
            builder.Services.Configure<FileUploadSettings>(builder.Configuration.GetSection(typeof(FileUploadSettings).Name));
            builder.Services.Configure<MetaDefenderCoreDetails>(builder.Configuration.GetSection(typeof(MetaDefenderCoreDetails).Name));
            builder.Services.Configure<CalendlySettings>(builder.Configuration.GetSection(typeof(CalendlySettings).Name));
            builder.Services.Configure<TutorialVideos>(builder.Configuration.GetSection(typeof(TutorialVideos).Name));

            builder.Services.ConfigureBrokerIQServices();

            await builder.Build().RunAsync();
        }
    }
}