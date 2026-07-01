using Microsoft.AspNetCore.Identity;

namespace AzubiLog.Services.Identity;

public sealed class GermanIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError() => new() { Code = nameof(DefaultError), Description = "Ein unbekannter Fehler ist aufgetreten." };

    public override IdentityError ConcurrencyFailure() => new() { Code = nameof(ConcurrencyFailure), Description = "Konkurrenzfehler beim Aktualisieren des Datensatzes." };

    public override IdentityError PasswordMismatch() => new() { Code = nameof(PasswordMismatch), Description = "Das Passwort und die Passwortbestätigung stimmen nicht überein." };

    public override IdentityError InvalidToken() => new() { Code = nameof(InvalidToken), Description = "Der verwendete Token ist ungültig." };

    public override IdentityError LoginAlreadyAssociated() => new() { Code = nameof(LoginAlreadyAssociated), Description = "Es existiert bereits ein Konto mit diesem Login." };

    public override IdentityError InvalidUserName(string? userName) => new() { Code = nameof(InvalidUserName), Description = $"Der Benutzername '{userName ?? string.Empty}' ist ungültig." };

    public override IdentityError InvalidEmail(string? email) => new() { Code = nameof(InvalidEmail), Description = $"Die E-Mail-Adresse '{email ?? string.Empty}' ist ungültig." };

    public override IdentityError DuplicateUserName(string? userName) => new() { Code = nameof(DuplicateUserName), Description = "Dieser Benutzername wird bereits verwendet." };

    public override IdentityError DuplicateEmail(string? email) => new() { Code = nameof(DuplicateEmail), Description = "Diese E-Mail-Adresse wird bereits verwendet." };

    public override IdentityError InvalidRoleName(string? role) => new() { Code = nameof(InvalidRoleName), Description = $"Der Rollenname '{role ?? string.Empty}' ist ungültig." };

    public override IdentityError DuplicateRoleName(string? role) => new() { Code = nameof(DuplicateRoleName), Description = "Dieser Rollenname ist bereits vergeben." };

    public override IdentityError PasswordTooShort(int length) => new() { Code = nameof(PasswordTooShort), Description = $"Passwörter müssen mindestens {length} Zeichen lang sein." };

    public override IdentityError PasswordRequiresNonAlphanumeric() => new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Passwörter müssen mindestens ein nicht alphanumerisches Zeichen enthalten." };

    public override IdentityError PasswordRequiresDigit() => new() { Code = nameof(PasswordRequiresDigit), Description = "Passwörter müssen mindestens eine Ziffer (0-9) enthalten." };

    public override IdentityError PasswordRequiresLower() => new() { Code = nameof(PasswordRequiresLower), Description = "Passwörter müssen mindestens einen Kleinbuchstaben enthalten." };

    public override IdentityError PasswordRequiresUpper() => new() { Code = nameof(PasswordRequiresUpper), Description = "Passwörter müssen mindestens einen Großbuchstaben enthalten." };

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => new() { Code = nameof(PasswordRequiresUniqueChars), Description = $"Passwörter müssen mindestens {uniqueChars} unterschiedliche Zeichen enthalten." };
}
