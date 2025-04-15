using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SWGIndustries.Services;

namespace SWGIndustries.Components.Account;

public static class AccountExtensions
{
    public static IEndpointConventionBuilder MapAdditionalAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var accountGroup = endpoints.MapGroup("/Account");

        accountGroup.MapPost("/PerformExternalLogin", (
            [FromForm] string provider,
            [FromForm] string returnUrl) =>
        {
            var redirectUri = $"/Account/ExternalLoginCallback?returnUrl={returnUrl}";
            return TypedResults.Challenge(new AuthenticationProperties { RedirectUri = redirectUri }, [provider]);
        });

        accountGroup.MapGet("/Logout", async (
            HttpContext context, 
            [FromServices] UserService userManager) =>
        {
            await context.SignOutAsync();
            userManager.UserLoggedOut();
            return TypedResults.LocalRedirect("~/");
        });

        accountGroup.MapGet("/ExternalLoginCallback", async (
            HttpContext context,
            [FromServices] UserService userManager,
            string returnUrl) =>
        {
            
            var userInfo = await userManager.UserLoggedIn(context);
            
            // If the user is created for the first time, redirect to the account management page for the user to finish account setup.
            return Results.Redirect(userInfo.IsAccountJustCreated ? "/Account/Manage" : returnUrl);
        });

        return accountGroup;
    }    
}
