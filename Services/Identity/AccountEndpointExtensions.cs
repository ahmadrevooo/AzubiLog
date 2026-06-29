using AzubiLog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Json;

namespace AzubiLog.Services.Identity;

public static class AccountEndpointExtensions
{
    private const string RegistrationErrorsQueryKey = "errors";

    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/account/register/submit", async (
            HttpContext context,
            AccountFlowService accountFlow) =>
        {
            var form = await context.Request.ReadFormAsync();
            var firstName = form["firstName"].ToString();
            var lastName = form["lastName"].ToString();
            var email = form["email"].ToString();
            var password = form["password"].ToString();
            var confirmPassword = form["confirmPassword"].ToString();
            if (string.IsNullOrWhiteSpace(firstName)
                || string.IsNullOrWhiteSpace(lastName)
                || string.IsNullOrWhiteSpace(email)
                || string.IsNullOrWhiteSpace(password)
                || password != confirmPassword)
            {
                return Results.Redirect("/account/register?error=invalid");
            }

            var result = await accountFlow.RegisterAsync(firstName, lastName, email, password, context.RequestAborted);
            if (!result.Succeeded)
            {
                var encodedErrors = EncodeErrors(result.Errors.Select(static error => error.Description));
                return Results.Redirect($"/account/register?error=failed&{RegistrationErrorsQueryKey}={Uri.EscapeDataString(encodedErrors)}");
            }

            return Results.Redirect("/account/login?registered=true");
        }).AllowAnonymous().DisableAntiforgery();

        endpoints.MapPost("/account/login/submit", async (
            HttpContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager) =>
        {
            var form = await context.Request.ReadFormAsync();
            var email = form["email"].ToString();
            var password = form["password"].ToString();
            var rememberMe = string.Equals(form["rememberMe"].ToString(), "true", StringComparison.OrdinalIgnoreCase);
            var returnUrl = GetLocalReturnUrl(context, form["returnUrl"].ToString());

            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                return Results.Redirect("/account/login?error=invalid");
            }

            if (!await userManager.CheckPasswordAsync(user, password))
            {
                return Results.Redirect("/account/login?error=invalid");
            }

            await signInManager.SignInAsync(user, rememberMe);
            return Results.Redirect(returnUrl);
        }).AllowAnonymous().DisableAntiforgery();

        endpoints.MapPost("/account/logout/submit", async (SignInManager<ApplicationUser> signInManager) =>
        {
            await signInManager.SignOutAsync();
            return Results.Redirect("/account/login?loggedOut=true");
        }).RequireAuthorization().DisableAntiforgery();

        endpoints.MapPost("/account/forgot-password/submit", async (
            HttpContext context,
            UserManager<ApplicationUser> userManager,
            AccountFlowService accountFlow) =>
        {
            var form = await context.Request.ReadFormAsync();
            var email = form["email"].ToString();
            var user = await userManager.FindByEmailAsync(email);

            if (user is not null)
            {
                await accountFlow.SendPasswordResetLinkAsync(user);
            }

            return Results.Redirect("/account/forgot-password?sent=true");
        }).AllowAnonymous().DisableAntiforgery();

        endpoints.MapPost("/account/reset-password/submit", async (
            HttpContext context,
            UserManager<ApplicationUser> userManager) =>
        {
            var form = await context.Request.ReadFormAsync();
            var email = form["email"].ToString();
            var code = form["code"].ToString();
            var password = form["password"].ToString();
            var confirmPassword = form["confirmPassword"].ToString();

            if (string.IsNullOrWhiteSpace(email)
                || string.IsNullOrWhiteSpace(code)
                || string.IsNullOrWhiteSpace(password)
                || password != confirmPassword)
            {
                return Results.Redirect($"/account/reset-password?email={Uri.EscapeDataString(email)}&code={Uri.EscapeDataString(code)}&error=invalid");
            }

            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                return Results.Redirect("/account/login?reset=true");
            }

            var token = AccountFlowService.DecodeToken(code);
            var result = await userManager.ResetPasswordAsync(user, token, password);
            if (!result.Succeeded)
            {
                return Results.Redirect($"/account/reset-password?email={Uri.EscapeDataString(email)}&code={Uri.EscapeDataString(code)}&error=failed");
            }

            return Results.Redirect("/account/login?reset=true");
        }).AllowAnonymous().DisableAntiforgery();

        return endpoints;
    }

    private static string GetLocalReturnUrl(HttpContext context, string returnUrl)
    {
        return !string.IsNullOrWhiteSpace(returnUrl) && Uri.TryCreate(returnUrl, UriKind.Relative, out _)
            ? returnUrl
            : "/";
    }

    private static string EncodeErrors(IEnumerable<string> errors)
    {
        var json = JsonSerializer.Serialize(errors.Where(static error => !string.IsNullOrWhiteSpace(error)));
        return WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(json));
    }
}
