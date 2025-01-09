using BookingService.Consumers;
using BookingService.DB;
using BookingService.Interfaces;
using IdentityModel;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shared.Events.Booking;
using Shared.Events.User;
using Shared.Exceptions;
using E = BookingService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
#region db config
builder.Services.AddDbContext<Context>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
#endregion

builder.Services.AddScoped<IBookingService, E.BookingService>();
builder.Services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<WorkerBookingsRequestConsumer>();
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("booking", false));
    x.AddRequestClient<IsValidBookingTimeRequested>();
    x.AddRequestClient<BookingConfirmationRequested>();
    x.AddRequestClient<UserEmailRequested>();
    x.AddRequestClient<UserIdRequested>();
    x.AddRequestClient<BookingEditRequest>();

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
        options.Audience = "api";
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
