using Duende.IdentityServer.Services;
using IdentityServerHost;
using IdentityService.Consumers;
using IdentityService.Data;
using IdentityService.Models;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Shared.Events.User;
using Shared.Exceptions;

namespace IdentityService;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();

        /*builder.Services.AddCors(options =>
        {
            options.AddPolicy("customPolicy", b =>
            {
                b.AllowAnyHeader()
                    .AllowAnyMethod().AllowCredentials().WithOrigins(builder.Configuration["Client"], "http://localhost:80", "http://localhost:443", "http://localhost:3000");
            });
        });*/

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddMassTransit(x =>
        {
            x.AddConsumersFromNamespaceContaining<UserEmailRequestConsumer>();
            x.AddRequestClient<UserCompanyRolesRequested>();

            x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("user", false));

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

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 6;
        }).AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddTransient<IProfileService, CustomProfileService>();

        builder.Services
            .AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                options.EmitStaticAudienceClaim = true;
            })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiResources(Config.ApiResources())
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.Clients)
            .AddAspNetIdentity<ApplicationUser>()
            .AddProfileService<CustomProfileService>()
            .AddTestUsers(TestUsers.Users);



        builder.Services.AddAuthentication();
        /*.AddGoogle(options =>
        {
            options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

            // register your IdentityServer with Google at https://console.developers.google.com
            // enable the Google+ API
            // set the redirect URI to https://localhost:5001/signin-google
            options.ClientId = "copy client ID from Google here";
            options.ClientSecret = "copy client secret from Google here";
        }
        );*/

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.ConfigureExceptionHandler();

        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStaticFiles();
        app.UseRouting();

       // app.UseCors("customPolicy");
        app.UseIdentityServer();
        app.UseAuthorization();

        app.MapRazorPages()
            .RequireAuthorization();

        return app;
    }
}