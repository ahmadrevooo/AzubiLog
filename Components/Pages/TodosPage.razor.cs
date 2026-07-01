using System.ComponentModel.DataAnnotations;
using AzubiLog.Services.Todos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AzubiLog.Components.Pages;

public partial class TodosPage : ComponentBase
{
    [Inject]
    private ITodoService TodoService { get; set; } = null!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    [Inject]
    private ILogger<TodosPage> Logger { get; set; } = null!;

    protected TodoPageViewModel? ViewModel { get; private set; }
    protected string? ErrorMessage { get; private set; }

    [SupplyParameterFromForm(FormName = "todo-create-form", Name = "TodoForm")]
    protected TodoFormModel? NewTodo { get; set; }

    protected TodoFormModel TodoForm => NewTodo ??= new TodoFormModel();

    protected override async Task OnInitializedAsync()
    {
        await ReloadAsync();
    }

    protected async Task HandleCreateAsync()
    {
        if (ViewModel is null)
        {
            return;
        }

        ErrorMessage = null;
        try
        {
            await TodoService.CreateTodoAsync(TodoForm);
            await ReloadAsync();
        }
        catch (ValidationException ex)
        {
            Logger.LogWarning(ex, "Validation failed when creating todo");
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to create todo");
            ErrorMessage = "Aufgabe konnte nicht erstellt werden.";
        }
    }

    protected async Task HandleCompleteAsync(int todoId)
    {
        ErrorMessage = null;
        try
        {
            await TodoService.CompleteTodoAsync(todoId);
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to complete todo {TodoId}", todoId);
            ErrorMessage = "Aufgabe konnte nicht abgeschlossen werden.";
        }
    }

    protected async Task HandleDeleteAsync(int todoId)
    {
        var confirmed = await JsRuntime.InvokeAsync<bool>(
            "confirm",
            Localizer["TodoDeleteConfirm"].Value);
        if (!confirmed)
        {
            return;
        }

        ErrorMessage = null;
        try
        {
            await TodoService.DeleteTodoAsync(todoId);
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete todo {TodoId}", todoId);
            ErrorMessage = "Aufgabe konnte nicht gelöscht werden.";
        }
    }

    private async Task ReloadAsync()
    {
        ViewModel = await TodoService.GetTodoPageAsync();
        NewTodo = new TodoFormModel();
    }
}
