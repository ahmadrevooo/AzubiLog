namespace AzubiLog.Services.Identity;

public sealed class BrevoSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string SenderName { get; set; } = "AzubiLog";
    public string SenderEmail { get; set; } = string.Empty;
}
