using System.Globalization;
using AzubiLog.Data;
using AzubiLog.Models;
using AzubiLog.Services.ReportEntries;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AzubiLog.Services.Pdf;

public class WeeklyReportPdfService(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager) : IWeeklyReportPdfService
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
        var user = await userManager.FindByEmailAsync(ApplicationDataInitializer.SingleUserEmail)
            ?? throw new InvalidOperationException("Default apprentice user was not found.");
        var monday = GetMonday(date);
        var friday = monday.AddDays(4);

        var entries = await dbContext.ReportEntries
            .Include(entry => entry.Category)
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
                        entry.Category?.Name ?? "-",
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
            CompanyName = string.IsNullOrWhiteSpace(user.School) ? "Ausbildungsbetrieb" : user.School,
            Occupation = string.IsNullOrWhiteSpace(user.TrainingOccupation) ? "Ausbildungsberuf" : user.TrainingOccupation,
            CalendarWeek = ISOWeek.GetWeekOfYear(date),
            Year = ISOWeek.GetYear(date),
            TotalHours = entries.Sum(entry => entry.Duration ?? 0m),
            Days = days
        };
    }

    private static IDocument BuildDocument(WeeklyReportPdfModel model)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.4f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(text => text.FontSize(9).FontColor(Colors.Grey.Darken4));

                page.Header().Element(header => ComposeHeader(header, model));
                page.Content().Element(content => ComposeContent(content, model));
                page.Footer().Element(ComposeFooter);
            });
        });
    }

    private static void ComposeHeader(IContainer container, WeeklyReportPdfModel model)
    {
        container.Column(column =>
        {
            column.Spacing(8);
            column.Item().Text("Ausbildungsnachweis")
                .FontSize(18)
                .Bold()
                .FontColor(Colors.Grey.Darken4);
            column.Item().Row(row =>
            {
                row.RelativeItem().Element(item => HeaderField(item, "Auszubildende/r", model.ApprenticeName));
                row.RelativeItem().Element(item => HeaderField(item, "Ausbildungsbetrieb", model.CompanyName));
            });
            column.Item().Row(row =>
            {
                row.RelativeItem().Element(item => HeaderField(item, "Ausbildungsberuf", model.Occupation));
                row.RelativeItem().Element(item => HeaderField(item, "Kalenderwoche / Jahr", $"KW {model.CalendarWeek:00} / {model.Year}"));
            });
        });
    }

    private static void HeaderField(IContainer container, string label, string value)
    {
        container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingBottom(4)
            .Column(column =>
            {
                column.Item().Text(label).FontSize(7).FontColor(Colors.Grey.Darken1);
                column.Item().Text(value).FontSize(10).SemiBold();
            });
    }

    private static void ComposeContent(IContainer container, WeeklyReportPdfModel model)
    {
        container.PaddingTop(14).Column(column =>
        {
            column.Spacing(10);

            foreach (var day in model.Days)
            {
                column.Item().Element(item => ComposeDay(item, day));
            }

            column.Item()
                .AlignRight()
                .Text($"Gesamtstunden: {model.TotalHours:0.##} h")
                .FontSize(11)
                .Bold();
        });
    }

    private static void ComposeDay(IContainer container, WeeklyReportPdfDay day)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(column =>
        {
            column.Spacing(6);
            column.Item().Row(row =>
            {
                row.RelativeItem().Text($"{day.Label}, {day.Date:dd.MM.yyyy}").Bold().FontSize(11);
                row.AutoItem().Text($"{day.Entries.Sum(entry => entry.Hours):0.##} h").Bold();
            });

            if (day.Entries.Count == 0)
            {
                column.Item().Text("Keine Eintraege vorhanden.").FontColor(Colors.Grey.Darken1);
                return;
            }

            foreach (var entry in day.Entries)
            {
                column.Item().Element(item => ComposeEntry(item, entry));
            }
        });
    }

    private static void ComposeEntry(IContainer container, WeeklyReportPdfEntry entry)
    {
        container.BorderTop(1).BorderColor(Colors.Grey.Lighten3).PaddingTop(6).Column(column =>
        {
            column.Spacing(3);
            column.Item().Row(row =>
            {
                row.RelativeItem().Text($"{entry.CategoryName} - {entry.Title}").SemiBold();
                row.AutoItem().Text($"{entry.StartTime:HH:mm} - {entry.EndTime:HH:mm} | {entry.Hours:0.##} h");
            });

            if (entry.DayType == ReportEntryDayType.VocationalSchool)
            {
                column.Item().Text("Berufsschultag").Bold();

                if (!string.IsNullOrWhiteSpace(entry.Subject))
                {
                    column.Item().Text($"Faecher: {entry.Subject}");
                }
            }
            else if (!string.IsNullOrWhiteSpace(entry.Description))
            {
                column.Item().Text(entry.Description);
            }

            if (!string.IsNullOrWhiteSpace(entry.Notes))
            {
                column.Item().Text($"Notizen: {entry.Notes}").FontColor(Colors.Grey.Darken1);
            }
        });
    }

    private static void ComposeFooter(IContainer container)
    {
        container.PaddingTop(18).Column(column =>
        {
            column.Spacing(14);
            column.Item().Row(row =>
            {
                row.RelativeItem().Element(item => SignatureField(item, "Auszubildende/r"));
                row.ConstantItem(40);
                row.RelativeItem().Element(item => SignatureField(item, "Ausbilder/in"));
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

    private static void SignatureField(IContainer container, string label)
    {
        container.Column(column =>
        {
            column.Item().Height(28);
            column.Item().BorderTop(1).BorderColor(Colors.Grey.Darken2).PaddingTop(4).Text(label).FontSize(8);
        });
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
