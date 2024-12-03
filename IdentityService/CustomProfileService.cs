using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityModel;
using IdentityService.Models;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Shared.Events.User;
using System.Security.Claims;

namespace IdentityService
{
    public class CustomProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRequestClient<UserCompanyRolesRequested> _client;


        public CustomProfileService(UserManager<ApplicationUser> userManager, IRequestClient<UserCompanyRolesRequested> client)
        {
            _userManager = userManager;
            _client = client;
        }
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var user = await _userManager.GetUserAsync(context.Subject);

            if (user == null)
                return;


            var claims = new List<Claim> { new Claim(JwtClaimTypes.Name, user.UserName) , // Add name claim
            new Claim (JwtClaimTypes.Email, user.Email) ,//Add email claim
            };

            // Add role claims
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(JwtClaimTypes.Role, role)));

            //Add company role claims
            var companyRoles = await _client.GetResponse<UserCompanyRolesRequestResult>(new UserCompanyRolesRequested { UserId = user.Id });
            claims.AddRange(companyRoles.Message.Roles.Select(role => new Claim("company_role", role.Key + "_" + role.Value)));

            // Include the claims in the issued token
            context.IssuedClaims.AddRange(claims);
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var user = await _userManager.GetUserAsync(context.Subject);
            context.IsActive = user != null;
        }
    }
}
