using BasketSurvay.Contracts.Validation;
using FluentValidation.AspNetCore;
using MapsterMapper;
using System.Reflection;

namespace BasketSurvay
{
    public static class DependancyInjection
    {
        public static IServiceCollection AddDependacies(this IServiceCollection Services)
        {
            Services.AddControllers();

          Services
                .AddOpenApi()                         //add swagger 
                .AddRegisteringServices()            //for registering services
                .AddMapsterServices()               // for mapster 
                .AddFluentValidationServices();    //for valiadtion code 

           return Services;
        }
        public static IServiceCollection AddMapsterServices(this IServiceCollection services)
        {
            var MappingConfig = TypeAdapterConfig.GlobalSettings;
            MappingConfig.Scan(Assembly.GetExecutingAssembly());
            services.AddSingleton<IMapper>(new Mapper(MappingConfig));

            return services;
        }
        public static IServiceCollection AddFluentValidationServices(this IServiceCollection services)
        {
            services.AddFluentValidationAutoValidation()
                .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
        public static IServiceCollection AddRegisteringServices(this IServiceCollection services)
        {
            services.AddScoped<IPollServices, PollServices>();

            return services;
        }
    }
}
