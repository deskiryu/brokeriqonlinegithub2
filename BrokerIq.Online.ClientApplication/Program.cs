using System;
using System.Net.Http;
using System.Threading.Tasks;
using Blazored.SessionStorage;
using BrokerIQ.Online.AppSettings;
using BrokerIQ.Online.Mapper;
using BrokerIQ.Online.Server.AppSettings;
using BrokerIQ.Online.Server.Helper;
using BrokerIQ.Online.Server.Services;
using BrokerIQ.Online.Server.Services.Interface;
using BrokerIQ.Online.Services;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Concrete;
using BrokerIQ.Online.Services.Interface;
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

            builder.Services.Configure<ReviewItAPIDetails>(builder.Configuration.GetSection(typeof(ReviewItAPIDetails).Name));
            builder.Services.Configure<FileUploadSettings>(builder.Configuration.GetSection(typeof(FileUploadSettings).Name));
            builder.Services.Configure<MetaDefenderCoreDetails>(builder.Configuration.GetSection(typeof(MetaDefenderCoreDetails).Name));
            builder.Services.Configure<CalendlySettings>(builder.Configuration.GetSection(typeof(CalendlySettings).Name));

            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<IRequestProviderService, RequestProviderService>();
            builder.Services.AddScoped<ICustomerService, CustomerService>();
            builder.Services.AddScoped<IInsuranceService, InsuranceService>();
            builder.Services.AddScoped<IInsuranceDocumentService, InsuranceDocumentService>();
            builder.Services.AddScoped<IMortgageService, MortgageService>();
            builder.Services.AddScoped<IMortgageDocumentService, MortgageDocumentService>();
            builder.Services.AddScoped<ICustomerDocumentService, CustomerDocumentService>();
            builder.Services.AddScoped<IDocumentsRequirementService, DocumentsRequirementService>();
            builder.Services.AddScoped<IBrokerDefinedMessageService, BrokerDefinedMessageService>();
            builder.Services.AddScoped<IAlertService, AlertService>();
            builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
            builder.Services.AddScoped<IVideoService, VideoService>();
            builder.Services.AddScoped<IAudioService, AudioService>();
            builder.Services.AddScoped<IAudioRecordingService, AudioRecordingService>();
            builder.Services.AddScoped<ILogoService, LogoService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IBrokerService, BrokerService>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IBrokerStaffService, BrokerStaffService>();
            builder.Services.AddScoped<IHealthService, HealthService>();
            builder.Services.AddScoped<IAddressService, AddressService>();
            builder.Services.AddScoped<IEmailInviteService, EmailInviteService>();
            builder.Services.AddScoped<IMenuPlanService, MenuPlanService>();
            builder.Services.AddScoped<IChartDataService, ChartDataService>();
            builder.Services.AddScoped<INoteService, NoteService>();
            builder.Services.AddScoped<IChatService, ChatService>();
            builder.Services.AddScoped<IMetaDefenderCoreService, MetaDefenderCoreService>();
            builder.Services.AddScoped<IVersionService, VersionService>();
            builder.Services.AddScoped<ITelephoneInviteService, TelephoneInviteService>();
            builder.Services.AddScoped<IClientReferralService, ClientReferralService>();
            builder.Services.AddScoped<ITrainingVideoService, TrainingVideoService>();
            builder.Services.AddScoped<IBrokerReminderOptionService, BrokerReminderOptionService>();
            builder.Services.AddScoped<IBrokerSubscriptionService, BrokerSubscriptionService>();
            builder.Services.AddScoped<IInsuranceQuoteService, InsuranceQuoteService>();
            builder.Services.AddScoped<IOccupationService, OccupationService>();
            builder.Services.AddScoped<IBrokerIdentifierService, BrokerIdentifierService>();
            builder.Services.AddScoped<IAssignmentService, AssignmentService>();
            builder.Services.AddScoped<IBrokerIntegrationService, BrokerIntegrationService>();
            builder.Services.AddScoped<ICalendlyService, CalendlyService>();
            builder.Services.AddScoped<ICustomerAppointmentService, CustomerAppointmentService>();

            builder.Services.AddAutoMapper(typeof(ReviewItMapper));
            builder.Services.AddScoped<LoggedInAppState>();
            builder.Services.AddScoped<MessageCountState>();

            await builder.Build().RunAsync();
        }
    }
}