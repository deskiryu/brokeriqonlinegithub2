using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReviewIt.Web.AppSettings;
using ReviewIt.Web.Mapper;
using ReviewIt.Web.Services;
using ReviewIt.Web.Services.Abstract;
using ReviewIt.Web.Services.Concrete;
using ReviewIt.Web.Services.Interface;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ReviewIt.Web.ClientApplication
{

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            //builder.Services.Configure<ReviewItAPIDetails>(Configuration.GetSection(typeof(ReviewItAPIDetails).Name));
            builder.Services.AddScoped<IRequestProviderService, RequestProviderService>();
            builder.Services.AddScoped<ICustomerService, CustomerService>();
            builder.Services.AddScoped<IInsuranceService, InsuranceService>();
            builder.Services.AddScoped<InsuranceDocumentService, InsuranceDocumentService>();
            builder.Services.AddScoped<ICustomerDocumentService, CustomerDocumentService>();
            builder.Services.AddScoped<IAlertService, AlertService>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
            builder.Services.AddAutoMapper(typeof(ReviewItMapper));

            await builder.Build().RunAsync();
        }

    }
}
