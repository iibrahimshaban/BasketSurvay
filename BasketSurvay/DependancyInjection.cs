
using BasketSurvay.Authentication;
using BasketSurvay.Persistence;
using FluentValidation.AspNetCore;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using System.Reflection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Options;



namespace BasketSurvay
{
    public static class DependancyInjection
    {
        public static IServiceCollection AddDependacies(this IServiceCollection Services ,
            IConfiguration configuration)
        {
            Services.AddControllers();

            Services.AddAuthenticationConfig(configuration);

            Services
                  .AddOpenApi()                            //add swagger 
                  .AddRegisteringService()               //for registering services
                  .AddMapsterConfig()                  // for mapster 
                  .AddFluentValidationConfig()       //for valiadtion code 
                  .AddDbContextConfig(configuration); // add data base 

           return Services;
        }
        private static IServiceCollection AddMapsterConfig(this IServiceCollection services)
        {
            var MappingConfig = TypeAdapterConfig.GlobalSettings;
            MappingConfig.Scan(Assembly.GetExecutingAssembly());
            services.AddSingleton<IMapper>(new Mapper(MappingConfig));

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

            return services;
        }
        private static IServiceCollection AddDbContextConfig(this IServiceCollection services,
            IConfiguration Configuration) 
        {
            var ConnectionString = Configuration.GetConnectionString("DefaultConnection") ??
                throw new InvalidOperationException("Conection String 'DefaultConnection' not found");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(ConnectionString));

            return services;
        }
        private static IServiceCollection AddAuthenticationConfig(this IServiceCollection services,
            IConfiguration configuration)
        {
            
            services.AddScoped<IAuthService, AuthService>();

            //services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

            services.AddOptions<JwtOptions>()
                .BindConfiguration(JwtOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            var settings = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();

            services.AddIdentity<ApplicationUser,IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

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

            return services;
        }
    }
}
