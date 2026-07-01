using System.Globalization;
using AzubiLog.Data;
using AzubiLog.Models;
using AzubiLog.Services.Identity;
using AzubiLog.Services.ReportEntries;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AzubiLog.Services.Pdf;

public class WeeklyReportPdfService(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService) : IWeeklyReportPdfService
{
    /// <inheritdoc />
    public async Task<byte[]> GenerateWeeklyReportPdfAsync(
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var model = await BuildModelAsync(date, cancellationToken);
        return BuildDocument(model).GeneratePdf();
    }

    /// <inheritdoc />
    public string GetFileName(DateTime date)
    {
        var week = ISOWeek.GetWeekOfYear(date);
        var year = ISOWeek.GetYear(date);
        return $"wochenbericht-kw-{week:00}-{year}.pdf";
    }

    private async Task<WeeklyReportPdfModel> BuildModelAsync(
        DateTime date,
        CancellationToken cancellationToken)
    {
        var user = await currentUserService.GetRequiredUserAsync(cancellationToken);
        var monday = GetMonday(date);
        var friday = monday.AddDays(4);

        var entries = await dbContext.ReportEntries
            .Where(entry => entry.UserId == user.Id
                && entry.Date.Date >= monday
                && entry.Date.Date <= friday
                && entry.Status == ReportEntryStatus.Saved)
            .OrderBy(entry => entry.Date)
            .ThenBy(entry => entry.StartTime)
            .ToListAsync(cancellationToken);

        var days = Enumerable.Range(0, 5)
            .Select(offset =>
            {
                var day = monday.AddDays(offset);
                var dayEntries = entries
                    .Where(entry => entry.Date.Date == day)
                    .Select(entry => new WeeklyReportPdfEntry(
                        entry.Date,
                        entry.Category?.Name ?? "",
                        entry.Title,
                        entry.Description,
                        entry.Note,
                        entry.Subject,
                        entry.DayType,
                        entry.StartTime,
                        entry.EndTime,
                        entry.Duration ?? 0m))
                    .ToList();

                return new WeeklyReportPdfDay(day, GetGermanDayLabel(offset), dayEntries);
            })
            .ToList();

        return new WeeklyReportPdfModel
        {
            ApprenticeName = FormatApprenticeName(user),
            CompanyName = string.IsNullOrWhiteSpace(user.CompanyName) ? "Ausbildungsbetrieb" : user.CompanyName,
            Occupation = string.IsNullOrWhiteSpace(user.TrainingOccupation) ? "Ausbildungsberuf" : user.TrainingOccupation,
            TrainingYear = user.TrainingYear <= 0 ? 1 : user.TrainingYear,
            TrainerName = string.IsNullOrWhiteSpace(user.TrainerName) ? "Ausbilder/in" : user.TrainerName,
            School = user.School,
            ClassName = user.ClassName,
            CalendarWeek = ISOWeek.GetWeekOfYear(date),
            Year = ISOWeek.GetYear(date),
            TotalHours = entries.Sum(entry => entry.Duration ?? 0m),
            AccentColor = string.IsNullOrWhiteSpace(user.PdfAccentColor) ? "#2563eb" : user.PdfAccentColor,
            Days = days
        };
    }

    private static IDocument BuildDocument(WeeklyReportPdfModel model)
    {
        var accent = ParseHexColor(model.AccentColor);
        var accentLight = Color.FromHex(LightenHex(model.AccentColor, 0.92f));

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.4f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(text => text.FontSize(9).FontColor(Colors.Grey.Darken4));

                page.Header().Element(header => ComposeHeader(header, model, accent, accentLight));
                page.Content().Element(content => ComposeContent(content, model, accent, accentLight));
                page.Footer().Element(footer => ComposeFooter(footer, accent));
            });
        });
    }

    private static void ComposeHeader(IContainer container, WeeklyReportPdfModel model, Color accent, Color accentLight)
    {
        container.Column(column =>
        {
            column.Spacing(6);

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(titleCol =>
                {
                    titleCol.Item().Text("Ausbildungsnachweis")
                        .FontSize(20)
                        .Bold()
                        .FontColor(accent);
                    titleCol.Item().Text($"Kalenderwoche {model.CalendarWeek:00} / {model.Year}")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken2);
                });
                row.AutoItem().AlignRight().AlignMiddle()
                    .Background(accentLight)
                    .Padding(8)
                    .Text($"{model.TotalHours:0.##} h")
                    .FontSize(14)
                    .Bold()
                    .FontColor(accent);
            });

            column.Item().PaddingTop(6).BorderBottom(2).BorderColor(accent);

            column.Item().PaddingTop(8).Row(row =>
            {
                row.RelativeItem().Element(item => HeaderField(item, "Auszubildende/r", model.ApprenticeName, accent));
                row.RelativeItem().Element(item => HeaderField(item, "Ausbildungsbetrieb", model.CompanyName, accent));
            });
            column.Item().Row(row =>
            {
                row.RelativeItem().Element(item => HeaderField(item, "Ausbildungsberuf", model.Occupation, accent));
                row.RelativeItem().Element(item => HeaderField(item, "Ausbildungsjahr", model.TrainingYear.ToString(CultureInfo.CurrentCulture), accent));
            });
            column.Item().Row(row =>
            {
                row.RelativeItem().Element(item => HeaderField(item, "Ausbilder/in", model.TrainerName, accent));
                var schoolInfo = BuildSchoolInfo(model.School, model.ClassName);
                row.RelativeItem().Element(item => HeaderField(item, "Berufsschule / Klasse", schoolInfo, accent));
            });
        });
    }

    private static string BuildSchoolInfo(string school, string className)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(school)) parts.Add(school);
        if (!string.IsNullOrWhiteSpace(className)) parts.Add(className);
        return parts.Count > 0 ? string.Join(" / ", parts) : "-";
    }

    private static void HeaderField(IContainer container, string label, string value, Color accent)
    {
        container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingBottom(4)
            .PaddingTop(4)
            .Column(column =>
            {
                column.Item().Text(label).FontSize(7).FontColor(accent);
                column.Item().Text(value).FontSize(10).SemiBold();
            });
    }

    private static void ComposeContent(IContainer container, WeeklyReportPdfModel model, Color accent, Color accentLight)
    {
        container.PaddingTop(12).Column(column =>
        {
            column.Spacing(8);

            foreach (var day in model.Days)
            {
                column.Item().Element(item => ComposeDay(item, day, accent, accentLight));
            }
        });
    }

    private static void ComposeDay(IContainer container, WeeklyReportPdfDay day, Color accent, Color accentLight)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Column(column =>
        {
            column.Item()
                .Background(accentLight)
                .Padding(8)
                .Row(row =>
                {
                    row.RelativeItem().Text($"{day.Label}, {day.Date:dd.MM.yyyy}")
                        .Bold().FontSize(10).FontColor(accent);
                    row.AutoItem().Text($"{day.Entries.Sum(entry => entry.Hours):0.##} h")
                        .Bold().FontSize(10).FontColor(accent);
                });

            if (day.Entries.Count == 0)
            {
                column.Item().Padding(8).Text("Keine Einträge vorhanden.")
                    .FontColor(Colors.Grey.Darken1).Italic();
                return;
            }

            foreach (var entry in day.Entries)
            {
                column.Item().Element(item => ComposeEntry(item, entry, accent));
            }
        });
    }

    private static void ComposeEntry(IContainer container, WeeklyReportPdfEntry entry, Color accent)
    {
        container.BorderTop(1).BorderColor(Colors.Grey.Lighten3).Padding(8).Column(column =>
        {
            column.Spacing(3);
            column.Item().Row(row =>
            {
                row.RelativeItem().Text(entry.Title).SemiBold().FontSize(9);
                row.AutoItem().Text($"{entry.StartTime:HH:mm} – {entry.EndTime:HH:mm} | {entry.Hours:0.##} h")
                    .FontSize(8).FontColor(Colors.Grey.Darken2);
            });

            if (entry.DayType == ReportEntryDayType.VocationalSchool)
            {
                column.Item().PaddingTop(2).Text("Berufsschultag")
                    .Bold().FontSize(8).FontColor(accent);

                if (!string.IsNullOrWhiteSpace(entry.Subject))
                {
                    column.Item().PaddingTop(2).Text($"Fächer: {entry.Subject}")
                        .FontSize(8).FontColor(Colors.Grey.Darken2);
                }

                if (!string.IsNullOrWhiteSpace(entry.Description))
                {
                    column.Item().PaddingTop(2).Text(entry.Description)
                        .FontSize(8);
                }
            }
            else if (!string.IsNullOrWhiteSpace(entry.Description))
            {
                column.Item().PaddingTop(2).Text(entry.Description).FontSize(8);
            }

            if (!string.IsNullOrWhiteSpace(entry.Notes))
            {
                column.Item().PaddingTop(2).Text($"Notiz: {entry.Notes}")
                    .FontSize(8).Italic().FontColor(Colors.Grey.Darken1);
            }
        });
    }

    private static void ComposeFooter(IContainer container, Color accent)
    {
        container.PaddingTop(14).Column(column =>
        {
            column.Spacing(12);
            column.Item().Row(row =>
            {
                row.RelativeItem().Element(item => SignatureField(item, "Auszubildende/r", accent));
                row.ConstantItem(40);
                row.RelativeItem().Element(item => SignatureField(item, "Ausbilder/in", accent));
            });
            column.Item().AlignCenter().DefaultTextStyle(style => style.FontSize(8).FontColor(Colors.Grey.Darken1)).Text(text =>
            {
                text.Span("Seite ");
                text.CurrentPageNumber();
                text.Span(" / ");
                text.TotalPages();
            });
        });
    }

    private static void SignatureField(IContainer container, string label, Color accent)
    {
        container.Column(column =>
        {
            column.Item().Height(28);
            column.Item().BorderTop(1.5f).BorderColor(accent).PaddingTop(4)
                .Text(label).FontSize(8).FontColor(Colors.Grey.Darken2);
        });
    }

    private static Color ParseHexColor(string hex)
    {
        return Color.FromHex(hex);
    }

    private static string LightenHex(string hex, float factor)
    {
        hex = hex.TrimStart('#');
        var r = Convert.ToInt32(hex[..2], 16);
        var g = Convert.ToInt32(hex[2..4], 16);
        var b = Convert.ToInt32(hex[4..6], 16);

        r = r + (int)((255 - r) * factor);
        g = g + (int)((255 - g) * factor);
        b = b + (int)((255 - b) * factor);

        return $"#{Math.Min(r, 255):X2}{Math.Min(g, 255):X2}{Math.Min(b, 255):X2}";
    }

    private static string FormatApprenticeName(ApplicationUser user)
    {
        var name = $"{user.FirstName} {user.LastName}".Trim();
        return string.IsNullOrWhiteSpace(name) ? user.Email ?? "Auszubildende/r" : name;
    }

    private static DateTime GetMonday(DateTime date)
    {
        var offset = ((int)date.DayOfWeek + 6) % 7;
        return date.Date.AddDays(-offset);
    }

    private static string GetGermanDayLabel(int offset)
    {
        return offset switch
        {
            0 => "Montag",
            1 => "Dienstag",
            2 => "Mittwoch",
            3 => "Donnerstag",
            _ => "Freitag"
        };
    }
}
