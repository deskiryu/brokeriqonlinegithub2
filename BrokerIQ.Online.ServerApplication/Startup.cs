using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Text.Json;
using Blazored.SessionStorage;
using BrokerIQ.Online.AppSettings;
using BrokerIQ.Online.Server.AppSettings;
using BrokerIQ.Online.Server.Helper;
using BrokerIQ.Online.Server.Services;
using BrokerIQ.Online.Services.Interface;
using BrokerIQ.Online.Mapper;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using BrokerIQ.Online.Services;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Concrete;
using BrokerIQ.Online.Server.Services.Interface;

namespace BrokerIQ.Online.ServerApplication
{
    public class Startup
    {
        private IWebHostEnvironment CurrentEnvironment { get; set; }
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            this.CurrentEnvironment = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizePage("/clientlist");
                options.Conventions.AuthorizePage("/videolist");
                options.Conventions.AuthorizePage("/notifications");
                options.Conventions.AuthorizePage("/clientdetail");
                options.Conventions.AuthorizePage("/clientedit");
                options.Conventions.AuthorizePage("/notifications");
                options.Conventions.AuthorizePage("/insuranceedit");
                options.Conventions.AuthorizePage("/mortgageedit");
            }
            );
            services.AddMudServices();
            services.AddMudBlazorDialog();

            services.Configure<ReviewItAPIDetails>(Configuration.GetSection(typeof(ReviewItAPIDetails).Name));
            services.Configure<AzureStorageDetails>(Configuration.GetSection(typeof(AzureStorageDetails).Name));
            services.Configure<FileUploadSettings>(Configuration.GetSection(typeof(FileUploadSettings).Name));
            services.Configure<MetaDefenderCoreDetails>(Configuration.GetSection(typeof(MetaDefenderCoreDetails).Name));

            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IRequestProviderService, RequestProviderService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IInsuranceService, InsuranceService>();
            services.AddScoped<IInsuranceDocumentService, InsuranceDocumentService>();
            services.AddScoped<IMortgageService, MortgageService>();
            services.AddScoped<IMortgageDocumentService, MortgageDocumentService>();
            services.AddScoped<ICustomerDocumentService, CustomerDocumentService>();
            services.AddScoped<IDocumentsRequirementService, DocumentsRequirementService>();
            services.AddScoped<IBrokerDefinedMessageService, BrokerDefinedMessageService>();
            services.AddScoped<IAlertService, AlertService>();
            services.AddScoped<ILocalStorageService, LocalStorageService>();
            services.AddScoped<IVideoService, VideoService>();
            services.AddScoped<IAudioService, AudioService>();
            services.AddScoped<IAudioRecordingService, AudioRecordingService>();
            services.AddScoped<ILogoService, LogoService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IBrokerService, BrokerService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IBrokerStaffService, BrokerStaffService>();
            services.AddScoped<IHealthService, HealthService>();
            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<IEmailInviteService, EmailInviteService>();
            services.AddScoped<IMenuPlanService, MenuPlanService>();
            services.AddScoped<IChartDataService, ChartDataService>();
            services.AddScoped<INoteService, NoteService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IMetaDefenderCoreService, MetaDefenderCoreService>();
            services.AddScoped<IVersionService, VersionService>();
            services.AddScoped<ITelephoneInviteService, TelephoneInviteService>();
            services.AddScoped<IBrokerIdentifierService, BrokerIdentifierService>();
            services.AddScoped<IClientReferralService, ClientReferralService>();
            services.AddScoped<ITrainingVideoService, TrainingVideoService>();
            services.AddScoped<IBrokerReminderOptionService, BrokerReminderOptionService>();
            services.AddScoped<IBrokerSubscriptionService, BrokerSubscriptionService>();
            services.AddScoped<IInsuranceQuoteService, InsuranceQuoteService>();
            services.AddScoped<IOccupationService, OccupationService>();
            services.AddScoped<IAssignmentService, AssignmentService>();

            services.AddAutoMapper(typeof(ReviewItMapper));
            services.AddScoped<LoggedInAppState>();
            services.AddScoped<MessageCountState>();

            if (!CurrentEnvironment.IsDevelopment())
            {
                services.AddSignalR().AddAzureSignalR(options =>
                {
                    options.ServerStickyMode = Microsoft.Azure.SignalR.ServerStickyMode.Required;
                    options.MaxPollIntervalInSeconds = 300;
                });
            }
            services.AddServerSideBlazor().AddCircuitOptions(o =>
            {
                o.DetailedErrors = true;
            }).AddHubOptions(h =>
            {
                h.ClientTimeoutInterval = TimeSpan.FromMinutes(10);
                h.KeepAliveInterval = TimeSpan.FromSeconds(3);
                h.HandshakeTimeout = TimeSpan.FromMinutes(10);
            });

            services.AddBlazoredSessionStorage(config =>
            {
                config.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                config.JsonSerializerOptions.IgnoreNullValues = true;
                config.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
                config.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                config.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                config.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
                config.JsonSerializerOptions.WriteIndented = false;
            });

            services.AddLogging(
            builder =>
            {
                builder.AddFilter("Microsoft", LogLevel.Warning)
                       .AddFilter("System", LogLevel.Information)
                       .AddConsole();
            });

            services.AddHealthChecks()
                .AddCheck<HealthCheck>("health_check");
            services.AddAzureAppConfiguration();
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapHealthChecks("/health");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action}");
            });
        }
    }
}
