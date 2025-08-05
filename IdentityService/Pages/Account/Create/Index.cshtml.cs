// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityModel;
using IdentityService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace IdentityServerHost.Pages.Create;

[SecurityHeaders]
[AllowAnonymous]
public class Index : PageModel
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly IIdentityServerInteractionService interaction;
    private readonly IEventService events;

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public Index(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IIdentityServerInteractionService interaction,
        IEventService events)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.interaction = interaction;
        this.events = events;
    }

    public IActionResult OnGet(string returnUrl)
    {
        Input = new InputModel { ReturnUrl = returnUrl };
        return Page();
    }

    public async Task<IActionResult> OnPost(string returnUrl)
    {
        Input = new InputModel { ReturnUrl = returnUrl };

        // check if we are in the context of an authorization request
        var context = await interaction.GetAuthorizationContextAsync(Input.ReturnUrl);

        // the user clicked the "cancel" button
        if (Input.Button != "create")
        {
            if (context != null)
            {
                // if the user cancels, send a result back into IdentityServer as if they 
                // denied the consent (even if this client does not require consent).
                // this will send back an access denied OIDC error response to the client.
                await interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

                // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                if (context.IsNativeClient())
                {
                    // The client is native, so this change in how to
                    // return the response is for better UX for the end user.
                    return this.LoadingPage(Input.ReturnUrl);
                }

                return Redirect(Input.ReturnUrl ?? "~/");
            }
            else
            {
                // since we don't have a valid context, then we just go back to the home page
                return Redirect("~/");
            }
        }

        // Check if username already exists
        var existingUser = await userManager.FindByNameAsync(Input.Username);
        if (existingUser != null)
        {
            ModelState.AddModelError("Input.Username", "Username already exists");
        }

        if (ModelState.IsValid && string.Equals(Input.Password, Input.ConfirmPassword))
        {
            var user = new ApplicationUser
            {
                UserName = Input.Username,
                Email = Input.Email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, Input.Password);
            if (result.Succeeded)
            {
                await userManager.AddClaimsAsync(user, new Claim[]
                {
                    new Claim(JwtClaimTypes.Name, Input.Username),
                    new Claim(JwtClaimTypes.Email, Input.Email)
                });

                // Log the event
                await events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName));

                //auto signin   user
                await signInManager.SignInAsync(user, isPersistent: false);

                if (Input.ReturnUrl != null)
                {
                    return Redirect(Input.ReturnUrl);
                }
                else
                {
                    return Redirect("~/");
                }
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }


        return Page();
    }
}
