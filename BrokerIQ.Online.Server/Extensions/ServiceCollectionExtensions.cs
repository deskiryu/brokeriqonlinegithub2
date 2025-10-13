using BrokerIQ.Online.Mapper;
using BrokerIQ.Online.Server.Helper;
using BrokerIQ.Online.Server.Services;
using BrokerIQ.Online.Server.Services.Interface;
using BrokerIQ.Online.Services;
using BrokerIQ.Online.Services.Abstract;
using BrokerIQ.Online.Services.Concrete;
using BrokerIQ.Online.Services.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace BrokerIQ.Online.Server.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureBrokerIQServices(this IServiceCollection services)
        {
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
            services.AddScoped<IBrokerConsentDocumentService, BrokerConsentDocumentService>();
            services.AddScoped<IAlertService, AlertService>();
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
            services.AddScoped<IOldChartDataService, OldChartDataService>();
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
            services.AddScoped<IBrokerIntegrationService, BrokerIntegrationService>();
            services.AddScoped<ICalendlyService, CalendlyService>();
            services.AddScoped<IPipedriveService, PipedriveService>();
            services.AddScoped<ICustomerAppointmentService, CustomerAppointmentService>();
            services.AddScoped<IDocumentVaultTypeService, DocumentVaultTypeService>();
            services.AddScoped<IWealthService, WealthService>();
            services.AddScoped<IWealthDocumentService, WealthDocumentService>();
            services.AddScoped<IWealthTypeService, WealthTypeService>();
            services.AddScoped<IGoalService, GoalService>();
            services.AddScoped<ICustomerGoalService, CustomerGoalService>();
            services.AddScoped<ICustomerWarningService, CustomerWarningService>();
            services.AddScoped<IEmailMessageTemplateService, EmailMessageTemplateService>();
            services.AddScoped<IBlogService, BlogService>();
            services.AddScoped<IWorkflowService, WorkflowService>();


            services.AddAutoMapper(typeof(ReviewItMapper));
            services.AddScoped<LoggedInAppState>();
            services.AddScoped<MessageCountState>();
            services.AddScoped<CookieService>();

            return services;
        }
    }
}
