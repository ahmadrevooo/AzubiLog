using System.ComponentModel.DataAnnotations;
using AzubiLog.Data;
using AzubiLog.Models;
using AzubiLog.Services.Identity;
using Microsoft.EntityFrameworkCore;

namespace AzubiLog.Services.DashboardNotes;

public class DashboardNoteService(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService) : IDashboardNoteService
{
    public async Task<IReadOnlyList<DashboardNoteViewModel>> GetRecentNotesAsync(
        int take,
        CancellationToken cancellationToken = default)
    {
        var user = await currentUserService.GetRequiredUserAsync(cancellationToken);

        return await dbContext.DashboardNotes
            .AsNoTracking()
            .Where(note => note.UserId == user.Id)
            .OrderByDescending(note => note.CreatedAt)
            .Take(take)
            .Select(note => new DashboardNoteViewModel(
                note.Id,
                note.Content,
                note.CreatedAt,
                note.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task CreateNoteAsync(
        DashboardNoteFormModel form,
        CancellationToken cancellationToken = default)
    {
        Validate(form);
        var user = await currentUserService.GetRequiredUserAsync(cancellationToken);

        dbContext.DashboardNotes.Add(new DashboardNote
        {
            UserId = user.Id,
            Content = form.Content.Trim(),
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void Validate(DashboardNoteFormModel form)
    {
        var context = new ValidationContext(form);
        Validator.ValidateObject(form, context, validateAllProperties: true);
    }
}
