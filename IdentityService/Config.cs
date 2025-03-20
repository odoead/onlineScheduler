using Duende.IdentityServer.Models;

namespace IdentityService;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResource("roles", new List<string> { "role" })

        };

    public static IEnumerable<ApiResource> ApiResources() =>
           new List<ApiResource>
           {
                new ApiResource("api", "The api")
                {
                    Scopes = { "api" },
                    UserClaims ={"role" }
                }
           };
    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("api"),
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {

            new Client
            {
                ClientId = "angular",
                ClientName= "Angular",
                RequirePkce= true,
                AllowAccessTokensViaBrowser = true,
                RequireClientSecret= false,
                AllowedGrantTypes = GrantTypes.Code,
                RedirectUris = new List<string>{ "http://localhost:4200/signin-callback", "http://localhost:4200/assets/silent-callback.html" },
                FrontChannelLogoutUri = "http://localhost:4200/signout-oidc",
                PostLogoutRedirectUris = { "http://localhost:4200/signout-callback-oidc" },
                AllowOfflineAccess = true,
                AllowedCorsOrigins = { "http://localhost:4200" },
                RequireConsent = false,
                AccessTokenLifetime = 600,
                AllowedScopes = { "openid", "profile", "api", "roles", },
            },
            new Client
            {
                ClientId = "microservice-client",
                ClientSecrets = { new Secret("key".Sha256()) },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = { "api" }
            }
        };
}
