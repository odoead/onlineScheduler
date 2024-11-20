using CompanyService.Consumers;
using CompanyService.DB;
using CompanyService.Interfaces;
using CompanyService.Services;
using IdentityModel;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shared.Events.User;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<Context>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ICompanyService, CompanyServ>();
builder.Services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<BookingConfirmationRequestedConsumer>();
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("company", false));
    x.AddRequestClient<UserEmailRequested>();

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


builder.Services.AddScoped<IBookingValidationService, BookingValidationService>();
builder.Services.AddScoped<ICompanyService, CompanyServ>();
builder.Services.AddScoped<IProductService, ProductServ>();
builder.Services.AddScoped<IScheduleService, ScheduleServ>();

var app = builder.Build();

// Configure the HTTP request pipeline.


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
