using ApiWithAuth.Abstraction;
using ApiWithAuth.Middlewares;
using ApiWithAuth.Models;
using ApiWithAuth.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Polly;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection;

namespace CoolWebApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMeLocalization(this IServiceCollection services)
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources");

        var supportedCultures = new List<CultureInfo> { new("en"), new("fa") };
        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture("en");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
            options.AddInitialRequestCultureProvider(new CustomRequestCultureProvider(async context =>
            {
                //Write your code here

                return await Task.FromResult(new ProviderCultureResult("en"));
                 
            }));
        });

        return services;
    }

    public static IServiceCollection AddMeApiVersioning(this IServiceCollection services)
    {
        services.Configure<RouteOptions>(options => { options.LowercaseUrls = true; });

        services.AddApiVersioning(
                options =>
                {
                    // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
                    options.ReportApiVersions = true;
                })
            .AddApiExplorer(
                options =>
                {
                    // add the versioned api explorer, which also adds
                    // IApiVersionDescriptionProvider service
                    // note: the specified format code will format the version as "'v'major[.minor][-status]"
                    options.GroupNameFormat = "'v'VVV";

                    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                    // can also be used to control the format of the API version in route templates
                    options.SubstituteApiVersionInUrl = true;
                })
            // this enables binding ApiVersion as a endpoint callback parameter. if you don't use
            // it, then you should remove this configuration.
            .EnableApiVersionBinding();

        return services;
    }

    public static IServiceCollection AddMeSwagger(this IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        services.AddSwaggerGen(options =>
        {
            // add a custom operation filter which sets default values
            options.OperationFilter<SwaggerDefaultValues>();
            options.OperationFilter<SwaggerLanguageHeader>();

            // JWT Bearer Authorization
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                }
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
        });

        return services;
    }

    public static IServiceCollection AddMeWeatherClient(this IServiceCollection services, IConfiguration configuration)
    {
        var weatherSetting = new WeatherSetting();
        configuration.GetSection("WeatherSetting").Bind(weatherSetting);
        services.AddSingleton(weatherSetting);

        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(10);

        services.AddHttpClient<IWeatherClient, WeatherClient>()
            .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(2)))
            .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(6, TimeSpan.FromSeconds(5)))
            .AddPolicyHandler(request =>
            {
                if (request.Method == HttpMethod.Get)
                    return timeoutPolicy;

                return Policy.NoOpAsync<HttpResponseMessage>();
            });

        return services;
    }
}