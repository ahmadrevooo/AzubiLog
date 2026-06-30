using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AzubiLog.Models;
using Microsoft.Extensions.Options;

namespace AzubiLog.Services.Identity;

public sealed class BrevoEmailSender(
    IOptions<BrevoSettings> brevoOptions,
    IHttpClientFactory httpClientFactory,
    ILogger<BrevoEmailSender> logger) : IAccountEmailSender
{
    private readonly BrevoSettings _settings = brevoOptions.Value;

    public async Task SendEmailConfirmationAsync(ApplicationUser user, string confirmationLink)
    {
        var subject = "AzubiLog – E-Mail-Adresse bestätigen";
        var body = BuildEmailBody(
            $"Hallo {user.FirstName},",
            "bitte bestätige deine E-Mail-Adresse, um dein AzubiLog-Konto zu aktivieren.",
            confirmationLink,
            "E-Mail bestätigen");

        await SendAsync(user.Email!, subject, body);
    }

    public async Task SendPasswordResetAsync(ApplicationUser user, string resetLink)
    {
        var subject = "AzubiLog – Passwort zurücksetzen";
        var body = BuildEmailBody(
            $"Hallo {user.FirstName},",
            "du hast eine Anfrage zum Zurücksetzen deines Passworts gestellt. Klicke auf den folgenden Link, um ein neues Passwort zu vergeben. Der Link ist aus Sicherheitsgründen nur begrenzt gültig.",
            resetLink,
            "Passwort zurücksetzen");

        await SendAsync(user.Email!, subject, body);
    }

    private async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("api-key", _settings.ApiKey);

        var payload = new
        {
            sender = new { name = _settings.SenderName, email = _settings.SenderEmail },
            to = new[] { new { email = toEmail } },
            subject,
            htmlContent = htmlBody
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync("https://api.brevo.com/v3/smtp/email", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("E-Mail an {Email} gesendet: {Subject}", toEmail, subject);
            }
            else
            {
                logger.LogError("Brevo API Fehler ({StatusCode}): {Response}", response.StatusCode, responseBody);
                throw new InvalidOperationException($"E-Mail-Versand fehlgeschlagen: {response.StatusCode} - {responseBody}");
            }
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Fehler beim Senden der E-Mail an {Email}", toEmail);
            throw;
        }
    }

    private static string BuildEmailBody(string greeting, string message, string actionUrl, string actionText)
    {
        return $"""
            <!DOCTYPE html>
            <html lang="de">
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <title>AzubiLog</title>
            </head>
            <body style="margin:0; padding:0; background-color:#f4f6f8; font-family:-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;">
                <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background-color:#f4f6f8; padding:40px 20px;">
                    <tr>
                        <td align="center">
                            <table role="presentation" width="480" cellpadding="0" cellspacing="0" style="background-color:#ffffff; border-radius:12px; box-shadow:0 2px 8px rgba(0,0,0,0.06); padding:40px;">
                                <tr>
                                    <td style="text-align:center; padding-bottom:24px;">
                                        <span style="font-size:22px; font-weight:700; color:#1a1a2e;">AzubiLog</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="font-size:16px; color:#333; line-height:1.6;">
                                        <p style="margin:0 0 12px;">{greeting}</p>
                                        <p style="margin:0 0 24px;">{message}</p>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="center" style="padding:8px 0 24px;">
                                        <a href="{actionUrl}" style="display:inline-block; background-color:#4f46e5; color:#ffffff; text-decoration:none; font-weight:600; font-size:15px; padding:12px 32px; border-radius:8px;">{actionText}</a>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="font-size:13px; color:#888; line-height:1.5; border-top:1px solid #eee; padding-top:20px;">
                                        <p style="margin:0 0 8px;">Falls du diese Aktion nicht angefordert hast, kannst du diese E-Mail ignorieren.</p>
                                        <p style="margin:0; word-break:break-all;">Link funktioniert nicht? Kopiere diese URL in deinen Browser:<br/><a href="{actionUrl}" style="color:#4f46e5;">{actionUrl}</a></p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;
    }
}
