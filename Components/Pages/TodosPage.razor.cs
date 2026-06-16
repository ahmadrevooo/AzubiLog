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

    protected TodoPageViewModel? ViewModel { get; private set; }

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

        await TodoService.CreateTodoAsync(ViewModel.NewTodo);
        await ReloadAsync();
    }

    protected async Task HandleCompleteAsync(int todoId)
    {
        await TodoService.CompleteTodoAsync(todoId);
        await ReloadAsync();
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

        await TodoService.DeleteTodoAsync(todoId);
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        ViewModel = await TodoService.GetTodoPageAsync();
    }
}
