
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using BasketSurvay.Authentication;
using BasketSurvay.Health;
using BasketSurvay.OpenApiTransformers;
using BasketSurvay.Settings;
using FluentValidation.AspNetCore;
using Hangfire;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;



namespace BasketSurvay
{
    public static class DependancyInjection
    {
        public static IServiceCollection AddDependacies(this IServiceCollection Services,
            IConfiguration configuration)
        {
            Services.AddControllers();

            Services.AddHybridCache();

            Services.AddAuthenticationConfig(configuration);

            Services.AddExceptionHandler<GlobalExceptionHandler>();
            Services.AddProblemDetails();

            Services
                  .AddRegisteringService()                             //for registering services
                  .AddMapsterConfig()                                 // for mapster 
                  .AddBackGroundJobsConfig(configuration)            //for hangfire 
                  .AddFluentValidationConfig()                      //for valiadtion code 
                  .AddDbContextConfig(configuration)
                  .AddApiVersioningConfig();

            Services
                .AddOpenApiServices()
                .AddEndpointsApiExplorer();


            return Services;
        }

        private static IServiceCollection AddMapsterConfig(this IServiceCollection services)
        {
            var MappingConfig = TypeAdapterConfig.GlobalSettings;
            MappingConfig.Scan(Assembly.GetExecutingAssembly());
            services.AddSingleton<IMapper>(new Mapper(MappingConfig));

            return services;
        }
        private static IServiceCollection AddOpenApiServices(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var apiVersionDescriptionProvider = serviceProvider.GetRequiredService<IApiVersionDescriptionProvider>();

            foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
            {
                services.AddOpenApi(description.GroupName, options =>
                {

                    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
                    options.AddDocumentTransformer(new MultiableVersioningSchemeTransformer(description));
                });
            }

            return services;
        }
        private static IServiceCollection AddApiVersioningConfig(this IServiceCollection services)
        {

            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true; // to make consumer know the aviliable versions
                options.ApiVersionReader = new HeaderApiVersionReader("X-api-Version");
            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            return services;
        }
        private static IServiceCollection AddBackGroundJobsConfig(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Hangfire services.
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(configuration.GetConnectionString("HangfireConnection")));

            // Add the processing server as IHostedService
            services.AddHangfireServer();

            return services;
        }
        private static IServiceCollection AddFluentValidationConfig(this IServiceCollection services)
        {
            services.AddFluentValidationAutoValidation()
                .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
        private static IServiceCollection AddRegisteringService(this IServiceCollection services)
        {
            services.AddScoped<IPollServices, PollServices>();
            services.AddScoped<IQuestionService, QuestionService>();
            services.AddScoped<IVoteService, VoteService>();
            services.AddScoped<IResultService, ResultService>();
            services.AddScoped<IEmailSender, EmailService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            // services.AddScoped<ICacheService,CacheService>();

            services.AddRateLimiter(RateLimiterOptions =>
            {
                RateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                RateLimiterOptions.AddPolicy("IpLimit", httpContext =>
                {
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 3, // Maximum requests in the window
                            Window = TimeSpan.FromSeconds(10) // 1 minute window
                        }
                    );
                });

                RateLimiterOptions.AddPolicy("UserLimit", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.GetUserId(),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 3, // Maximum requests in the window
                            Window = TimeSpan.FromSeconds(10) // 1 minute window
                        }
                    )
               );

                RateLimiterOptions.AddConcurrencyLimiter("concurrency", options =>
                {
                    options.PermitLimit = 10; // Maximum concurrent requests
                    options.QueueLimit = 5; // Maximum requests in the queue
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst; // Process oldest requests first
                });
            });

            services.AddHttpContextAccessor();

            return services;
        }
        private static IServiceCollection AddDbContextConfig(this IServiceCollection services,
            IConfiguration Configuration)
        {
            var ConnectionString = Configuration.GetConnectionString("DefaultConnection") ??
                throw new InvalidOperationException("Conection String 'DefaultConnection' not found");

            services.AddDbContext<ApplicationDbContext>(options =>
                         options.UseSqlServer(ConnectionString));

            services.AddHealthChecks()
                .AddSqlServer(name: "database", connectionString: ConnectionString, tags: ["DB"])
                .AddHangfire(options => { options.MinimumAvailableServers = 1; }, name: "hangfire", tags: ["api", "DB"])
                .AddCheck<MailProviderHealthCheck>(name: "mail provider", tags: ["api"]);

            return services;
        }
        private static IServiceCollection AddAuthenticationConfig(this IServiceCollection services,
            IConfiguration configuration)
        {

            services.AddScoped<IAuthService, AuthService>();

            services.
                AddOptions<MailSettings>()
                .BindConfiguration(nameof(MailSettings))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<JwtOptions>()
                .BindConfiguration(JwtOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            var settings = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();

            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();
            services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

            services.AddSingleton<IJwtProvider, JwtProvider>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
             .AddJwtBearer(o =>
             {
                 o.SaveToken = true;
                 o.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateIssuerSigningKey = true,
                     ValidateIssuer = true,
                     ValidateAudience = true,
                     ValidateLifetime = true,
                     IssuerSigningKey = new
                     SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings!.Key)),
                     ValidIssuer = settings.Issuer,
                     ValidAudience = settings.Audience
                 };
             });

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 8;
                options.SignIn.RequireConfirmedEmail = true;
                options.User.RequireUniqueEmail = true;
            });

            return services;
        }
    }
}
