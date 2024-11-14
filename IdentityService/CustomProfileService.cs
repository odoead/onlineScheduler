﻿using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityModel;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentityService
{
    public class CustomProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomProfileService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var user = await _userManager.GetUserAsync(context.Subject);

            if (user == null)
                return;

            // Add name claim
            var claims = new List<Claim> { new Claim(JwtClaimTypes.Name, user.UserName) };

            // Add role claims
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(JwtClaimTypes.Role, role)));

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
