using Hangfire;
using Hangfire.PostgreSql;
using IdentityModel;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NotificationService.Consumers;
using NotificationService.DB;
using NotificationService.Interfaces;
using Shared.Events.Company;
using Shared.Events.User;
using Shared.Exceptions;
using E = NotificationService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<Context>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<INotificationService, E.NotificationService>();

builder.Services.AddHangfire(config => config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
.UseSimpleAssemblyNameTypeSerializer()
.UseRecommendedSerializerSettings()
.UsePostgreSqlStorage(builder.Configuration
.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<BookingCreatedConsumer>();
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("notification", false));
    x.AddRequestClient<UserIdRequested>();
    x.AddRequestClient<UserEmailRequested>();
    x.AddRequestClient<NotificationAdditionalDataRequested>();


    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", host =>
        {
            host.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
            host.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
        });
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["IdentityServiceUrl"];
        options.Audience = "http://localhost:4200";
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            NameClaimType = JwtClaimTypes.Name,
            RoleClaimType = JwtClaimTypes.Role,
        };
    });

var app = builder.Build();

app.ConfigureExceptionHandler();
app.UseHangfireDashboard("/hangfire");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
