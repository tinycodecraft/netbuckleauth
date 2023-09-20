using System.Reflection;
using System.Text;
using ApiWithAuth;
using ApiWithAuth.Abstraction;
using ApiWithAuth.Extensions;
using ApiWithAuth.Middlewares;
using ApiWithAuth.Models;
using ApiWithAuth.Services;
using CoolWebApi.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);
var authsetting = builder.Configuration.GetSection(Constants.Setting.AuthSetting);
var encryptionService = new StringEncrypService();
authsetting[nameof(AuthSetting.Secret)] = encryptionService.EncryptString(authsetting[nameof(AuthSetting.SecretKey)] ?? "");

builder.Services.Configure<AuthSetting>(authsetting);

// Add services to the container.
builder.Services.AddDbContext<UsersContext>();
builder.Services.AddScoped<TokenService, TokenService>();
builder.Services.AddControllers().AddDataAnnotationsLocalization();

builder.Services.AddMeLocalization();
builder.Services.AddMeApiVersioning();

builder.Services.AddTransient<ProblemDetailsFactory, CustomDetailsFactory>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(option =>
{
    
    //option.Version = "v1";
    //option.Title = authsetting[nameof(AuthSetting.Issuer)];
    //option.Description = authsetting[nameof(AuthSetting.Issuer)];
    //option.AddSecurity("JWT", Enumerable.Empty<string>(), new NSwag.OpenApiSecurityScheme
    //{
    //    Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
    //    Name = "Authorization",
    //    In = NSwag.OpenApiSecurityApiKeyLocation.Header,
    //    Description = "Type into the textbox:Bearer {your JWT token}"
    //});
    //option.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
    option.OperationFilter<SwaggerDefaultValues>();
    option.OperationFilter<SwaggerLanguageHeader>();

    //done by SwaggerGenOptions
    //option.SwaggerDoc("v1", new OpenApiInfo { Title = "JWT Demo API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    option.IncludeXmlComments(xmlPath);
});

builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    //.AddAuthentication(option =>
    //{
    //    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    //    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        
    //})    
    .AddJwtBearer(options =>
    {
        
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ClockSkew = TimeSpan.Zero,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = authsetting[nameof(AuthSetting.Issuer)],
            ValidAudience = authsetting[nameof(AuthSetting.Audience)],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(authsetting[nameof(AuthSetting.Secret)] ?? "")
            ),
        };
    });

builder.Services
    .AddIdentityCore<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<UsersContext>();


var app = builder.Build();

app.UseApiExceptionHandling();

app.UseRequestLocalization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    //app.UseOpenApi();
    //app.UseSwaggerUi3();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();