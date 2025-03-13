using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace SWGIndustries.Components.Account;

public static class AccountExtensions
{
    public static IEndpointConventionBuilder MapAdditionalAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var accountGroup = endpoints.MapGroup("/Account");

        accountGroup.MapPost("/PerformExternalLogin", (
            [FromForm] string provider,
            [FromForm] string returnUrl) => TypedResults.Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, [provider]));

        accountGroup.MapGet("/Logout", async (
            HttpContext context) =>
        {
            await context.SignOutAsync();
            return TypedResults.LocalRedirect("~/");
        });

        return accountGroup;
    }    
}
