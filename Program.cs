using System.Globalization;
using AzubiLog.Components;
using AzubiLog.Data;
using AzubiLog.Models;
using AzubiLog.Services;
using AzubiLog.Services.CalendarDayMarkers;
using AzubiLog.Services.Dashboard;
using AzubiLog.Services.Identity;
using AzubiLog.Services.Pdf;
using AzubiLog.Services.Profile;
using AzubiLog.Services.ReportEntries;
using AzubiLog.Services.Timetable;
using AzubiLog.Services.Todos;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

namespace AzubiLog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
            var dataProtectionKeysPath = Path.Combine(
                builder.Environment.ContentRootPath,
                "..",
                "DataProtectionKeys");
            Directory.CreateDirectory(dataProtectionKeysPath);
            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
                .SetApplicationName("AzubiLog");

            builder.Services.AddSingleton<IApplicationNavigationService, ApplicationNavigationService>();
            builder.Services.AddSingleton<IThemePreferenceService, ThemePreferenceService>();
            builder.Services.AddScoped<ApplicationDataInitializer>();
            builder.Services.AddScoped<DefaultUserData>();
            builder.Services.AddScoped<AccountFlowService>();
            builder.Services.AddHttpClient();
            builder.Services.Configure<BrevoSettings>(options =>
            {
                builder.Configuration.GetSection("Brevo").Bind(options);
                var envKey = Environment.GetEnvironmentVariable("BREVO_API_KEY");
                if (!string.IsNullOrWhiteSpace(envKey))
                {
                    options.ApiKey = envKey;
                }
            });
            builder.Services.AddScoped<IAccountEmailSender, BrevoEmailSender>();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
            builder.Services.AddScoped<ICalendarDayMarkerService, CalendarDayMarkerService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<IApprenticeProfileService, ApprenticeProfileService>();
            builder.Services.AddScoped<IReportEntryService, ReportEntryService>();
            builder.Services.AddScoped<ITodoService, TodoService>();
            builder.Services.AddScoped<ITimetableService, TimetableService>();

            builder.Services.AddScoped<IWeeklyReportPdfService, WeeklyReportPdfService>();
            builder.Services.AddScoped<IAuthorizationHandler, ConfirmedEmailHandler>();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
                    .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

            builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
                .AddIdentityCookies();
            builder.Services.Configure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, options =>
            {
                options.LoginPath = "/account/login";
                options.AccessDeniedPath = "/account/email-confirmation-notice";
            });
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("ConfirmedEmail", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.AddRequirements(new ConfirmedEmailRequirement());
                });
            });

            builder.Services.AddIdentityCore<ApplicationUser>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;
                    options.User.RequireUniqueEmail = true;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireNonAlphanumeric = false;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[] { new CultureInfo("de-DE") };

                options.DefaultRequestCulture = new RequestCulture("de-DE");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
                options.RequestCultureProviders.Clear();
            });

            var app = builder.Build();
            QuestPDF.Settings.License = LicenseType.Community;

            using (var scope = app.Services.CreateScope())
            {
                var initializer = scope.ServiceProvider.GetRequiredService<ApplicationDataInitializer>();
                initializer.InitializeAsync().GetAwaiter().GetResult();
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseRequestLocalization();
            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapAccountEndpoints();
            app.MapGet("/weekly-reports/export", async (
                DateTime? date,
                IWeeklyReportPdfService pdfService,
                CancellationToken cancellationToken) =>
            {
                var exportDate = date ?? DateTime.Today;
                var pdfBytes = await pdfService.GenerateWeeklyReportPdfAsync(exportDate, cancellationToken);
                return Results.File(pdfBytes, "application/pdf", pdfService.GetFileName(exportDate));
            }).RequireAuthorization("ConfirmedEmail");
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}