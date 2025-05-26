using CompanyService.Consumers;
using CompanyService.DB;
using CompanyService.Interfaces;
using CompanyService.Services;
using IdentityModel;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shared.Events.Booking;
using Shared.Events.User;
using Shared.Exceptions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<Context>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<BookingConfirmationRequestConsumer>();
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("company", false));
    x.AddRequestClient<UserEmailRequested>();
    x.AddRequestClient<WorkerBookingsRequested>();
    x.AddRequestClient<GetClientBookingsRequested>();
    x.AddRequestClient<BookingStatisticsRequest>();

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

builder.Services.AddScoped<IBookingValidationService, BookingValidationService>();
builder.Services.AddScoped<ICompanyService, CompanyServ>();
builder.Services.AddScoped<IProductService, ProductServ>();
builder.Services.AddScoped<IScheduleService, ScheduleServ>();
builder.Services.AddScoped<IBookingService, BookingService>();

var app = builder.Build();

app.ConfigureExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
