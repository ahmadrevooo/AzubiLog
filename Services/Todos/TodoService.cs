using System.ComponentModel.DataAnnotations;
using AzubiLog.Data;
using AzubiLog.Models;
using AzubiLog.Services.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AzubiLog.Services.Todos;

public class TodoService(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService) : ITodoService
{
    public async Task<TodoPageViewModel> GetTodoPageAsync(CancellationToken cancellationToken = default)
    {
        var user = await currentUserService.GetRequiredUserAsync(cancellationToken);
        var todos = await dbContext.Todos
            .Where(todo => todo.UserId == user.Id)
            .OrderBy(todo => todo.IsCompleted)
            .ThenBy(todo => todo.DueDate ?? DateTime.MaxValue)
            .ThenBy(todo => todo.CreatedAt)
            .ToListAsync(cancellationToken);

        return new TodoPageViewModel
        {
            NewTodo = new TodoFormModel(),
            OpenTodos = todos.Where(todo => !todo.IsCompleted).Select(MapToViewModel).ToList(),
            CompletedTodos = todos.Where(todo => todo.IsCompleted).Select(MapToViewModel).ToList()
        };
    }

    public async Task<IReadOnlyList<TodoItemViewModel>> GetOpenTodosAsync(
        int take,
        CancellationToken cancellationToken = default)
    {
        var user = await currentUserService.GetRequiredUserAsync(cancellationToken);
        return await dbContext.Todos
            .Where(todo => todo.UserId == user.Id && !todo.IsCompleted)
            .OrderBy(todo => todo.DueDate ?? DateTime.MaxValue)
            .ThenBy(todo => todo.CreatedAt)
            .Take(take)
            .Select(todo => new TodoItemViewModel(
                todo.Id,
                todo.Title,
                todo.Description,
                todo.DueDate,
                todo.IsCompleted))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetOpenTodoCountAsync(CancellationToken cancellationToken = default)
    {
        var user = await currentUserService.GetRequiredUserAsync(cancellationToken);
        return await dbContext.Todos
            .CountAsync(todo => todo.UserId == user.Id && !todo.IsCompleted, cancellationToken);
    }

    public async Task CreateTodoAsync(TodoFormModel form, CancellationToken cancellationToken = default)
    {
        Validate(form);
        var user = await currentUserService.GetRequiredUserAsync(cancellationToken);

        dbContext.Todos.Add(new TodoItem
        {
            UserId = user.Id,
            Title = form.Title.Trim(),
            Description = form.Description.Trim(),
            DueDate = form.DueDate?.Date,
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteTodoAsync(int todoId, CancellationToken cancellationToken = default)
    {
        var user = await currentUserService.GetRequiredUserAsync(cancellationToken);
        var todo = await GetTodoForUserAsync(todoId, user.Id, cancellationToken);

        todo.IsCompleted = true;
        todo.CompletedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteTodoAsync(int todoId, CancellationToken cancellationToken = default)
    {
        var user = await currentUserService.GetRequiredUserAsync(cancellationToken);
        var todo = await GetTodoForUserAsync(todoId, user.Id, cancellationToken);

        dbContext.Todos.Remove(todo);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<TodoItem> GetTodoForUserAsync(
        int todoId,
        string userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Todos
            .FirstOrDefaultAsync(todo => todo.Id == todoId && todo.UserId == userId, cancellationToken)
            ?? throw new InvalidOperationException("Todo item was not found.");
    }

    private static TodoItemViewModel MapToViewModel(TodoItem todo)
    {
        return new TodoItemViewModel(
            todo.Id,
            todo.Title,
            todo.Description,
            todo.DueDate,
            todo.IsCompleted);
    }

    private static void Validate(TodoFormModel form)
    {
        var context = new ValidationContext(form);
        Validator.ValidateObject(form, context, validateAllProperties: true);
    }
}
