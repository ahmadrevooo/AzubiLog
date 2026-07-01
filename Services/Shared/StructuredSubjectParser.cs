using System.Text.Json;
using AzubiLog.Models;

namespace AzubiLog.Services.Shared;

public static class StructuredSubjectParser
{
    public static List<ClassTimetableEntry.StructuredSubjectEntry> Parse(string? subjectsText)
    {
        if (string.IsNullOrWhiteSpace(subjectsText))
        {
            return [];
        }

        var trimmed = subjectsText.Trim();
        if (!trimmed.StartsWith("[", StringComparison.Ordinal))
        {
            return [];
        }

        try
        {
            var entries = JsonSerializer.Deserialize<List<ClassTimetableEntry.StructuredSubjectEntry>>(
                trimmed,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return entries?
                .Where(entry => !string.IsNullOrWhiteSpace(entry.Fach))
                .ToList()
                ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
