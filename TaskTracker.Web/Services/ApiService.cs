using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TaskTracker.Models.DTOs;

namespace TaskTracker.Web.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly IToastService _toastService;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService(HttpClient httpClient, ILocalStorageService localStorage, IToastService toastService)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _toastService = toastService;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    // Методы аутентификации
    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var response = await PostAsync<LoginRequest, AuthResponse>("/api/auth/login", request);
        if (response != null)
        {
            await SetAuthenticationHeaderAsync(response.Token);
            _toastService.ShowSuccess("Добро пожаловать!", $"Успешный вход как {response.Username}");
        }
        return response;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        var response = await PostAsync<RegisterRequest, AuthResponse>("/api/auth/register", request);
        if (response != null)
        {
            await SetAuthenticationHeaderAsync(response.Token);
            _toastService.ShowSuccess("Регистрация успешна!", $"Добро пожаловать, {response.Username}!");
        }
        return response;
    }

    // Методы для проектов
    public async Task<List<ProjectResponse>> GetUserProjectsAsync()
    {
        await SetAuthorizationHeaderAsync();
        return await GetAsync<List<ProjectResponse>>("/api/projects") ?? new List<ProjectResponse>();
    }

    public async Task<ProjectResponse?> GetProjectByIdAsync(string projectId)
    {
        await SetAuthorizationHeaderAsync();
        return await GetAsync<ProjectResponse>($"/api/projects/{projectId}");
    }

    public async Task<ProjectResponse?> CreateProjectAsync(CreateProjectRequest request)
    {
        await SetAuthorizationHeaderAsync();
        var response = await PostAsync<CreateProjectRequest, ProjectResponse>("/api/projects", request);
        if (response != null)
        {
            _toastService.ShowSuccess("Проект создан!", $"Проект \"{response.Name}\" успешно создан");
        }
        return response;
    }

    public async Task<ProjectResponse?> UpdateProjectAsync(string projectId, UpdateProjectRequest request)
    {
        await SetAuthorizationHeaderAsync();
        var response = await PutAsync<UpdateProjectRequest, ProjectResponse>($"/api/projects/{projectId}", request);
        if (response != null)
        {
            _toastService.ShowSuccess("Проект обновлен!", $"Проект \"{response.Name}\" успешно обновлен");
        }
        return response;
    }

    public async Task<bool> DeleteProjectAsync(string projectId)
    {
        await SetAuthorizationHeaderAsync();
        var success = await DeleteAsync($"/api/projects/{projectId}");
        if (success)
        {
            _toastService.ShowSuccess("Проект удален!", "Проект успешно удален");
        }
        return success;
    }

    // Методы для канбан доски
    public async Task<List<ColumnResponse>> GetProjectColumnsAsync(string projectId)
    {
        await SetAuthorizationHeaderAsync();
        return await GetAsync<List<ColumnResponse>>($"/api/kanban/projects/{projectId}/columns") ?? new List<ColumnResponse>();
    }

    public async Task<ColumnResponse?> CreateColumnAsync(CreateColumnRequest request)
    {
        await SetAuthorizationHeaderAsync();
        var response = await PostAsync<CreateColumnRequest, ColumnResponse>("/api/kanban/columns", request);
        if (response != null)
        {
            _toastService.ShowSuccess("Колонка создана!", $"Колонка \"{response.Title}\" добавлена в проект");
        }
        return response;
    }

    public async Task<ColumnResponse?> UpdateColumnAsync(string columnId, UpdateColumnRequest request)
    {
        await SetAuthorizationHeaderAsync();
        var response = await PutAsync<UpdateColumnRequest, ColumnResponse>($"/api/kanban/columns/{columnId}", request);
        if (response != null)
        {
            _toastService.ShowSuccess("Колонка обновлена!", $"Колонка \"{response.Title}\" успешно обновлена");
        }
        return response;
    }

    public async Task<bool> DeleteColumnAsync(string columnId)
    {
        await SetAuthorizationHeaderAsync();
        var success = await DeleteAsync($"/api/kanban/columns/{columnId}");
        if (success)
        {
            _toastService.ShowSuccess("Колонка удалена!", "Колонка и все её задачи удалены");
        }
        return success;
    }

    // Методы для задач
    public async Task<List<TaskResponse>> GetColumnTasksAsync(string columnId)
    {
        await SetAuthorizationHeaderAsync();
        return await GetAsync<List<TaskResponse>>($"/api/kanban/columns/{columnId}/tasks") ?? new List<TaskResponse>();
    }

    public async Task<TaskResponse?> GetTaskByIdAsync(string taskId)
    {
        await SetAuthorizationHeaderAsync();
        return await GetAsync<TaskResponse>($"/api/kanban/tasks/{taskId}");
    }

    public async Task<TaskResponse?> CreateTaskAsync(CreateTaskRequest request)
    {
        await SetAuthorizationHeaderAsync();
        var response = await PostAsync<CreateTaskRequest, TaskResponse>("/api/kanban/tasks", request);
        if (response != null)
        {
            _toastService.ShowSuccess("Задача создана!", $"Задача \"{response.Title}\" добавлена в проект");
        }
        return response;
    }

    public async Task<TaskResponse?> UpdateTaskAsync(string taskId, UpdateTaskRequest request)
    {
        await SetAuthorizationHeaderAsync();
        var response = await PutAsync<UpdateTaskRequest, TaskResponse>($"/api/kanban/tasks/{taskId}", request);
        if (response != null)
        {
            _toastService.ShowSuccess("Задача обновлена!", $"Задача \"{response.Title}\" успешно обновлена");
        }
        return response;
    }

    public async Task<bool> DeleteTaskAsync(string taskId)
    {
        await SetAuthorizationHeaderAsync();
        var success = await DeleteAsync($"/api/kanban/tasks/{taskId}");
        if (success)
        {
            _toastService.ShowSuccess("Задача удалена!", "Задача успешно удалена");
        }
        return success;
    }

    public async Task<TaskResponse?> MoveTaskAsync(string taskId, MoveTaskRequest request)
    {
        await SetAuthorizationHeaderAsync();
        
        var endpoint = $"/api/kanban/tasks/{taskId}/move";
        Console.WriteLine($"🌐 API CLIENT: Отправка PUT запроса на {endpoint}");
        Console.WriteLine($"📦 API CLIENT: Данные запроса - NewColumnId: {request.NewColumnId}, NewOrder: {request.NewOrder}");
        
        var result = await PutAsync<MoveTaskRequest, TaskResponse>(endpoint, request);
        
        if (result != null)
        {
            Console.WriteLine($"✅ API CLIENT: Успешный ответ - TaskId: {result.Id}, ColumnId: {result.ColumnId}");
            _toastService.ShowSuccess("Задача перемещена!", $"Задача \"{result.Title}\" успешно перемещена");
        }
        else
        {
            Console.WriteLine($"❌ API CLIENT: Получен null ответ от сервера");
            _toastService.ShowError("Ошибка перемещения!", "Не удалось переместить задачу. Попробуйте еще раз.");
        }
        
        return result;
    }

    // Вспомогательные методы
    private async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json, _jsonOptions);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _toastService.ShowError("Ошибка загрузки данных", $"Код ошибки: {response.StatusCode}");
                Console.WriteLine($"GET {endpoint} failed: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            _toastService.ShowError("Ошибка соединения", "Не удалось подключиться к серверу");
            Console.WriteLine($"Error in GET {endpoint}: {ex.Message}");
        }
        return default;
    }

    private async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(endpoint, content);
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResponse>(responseJson, _jsonOptions);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _toastService.ShowError("Ошибка создания", $"Код ошибки: {response.StatusCode}");
                Console.WriteLine($"POST {endpoint} failed: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            _toastService.ShowError("Ошибка соединения", "Не удалось подключиться к серверу");
            Console.WriteLine($"Error in POST {endpoint}: {ex.Message}");
        }
        return default;
    }

    private async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync(endpoint, content);
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResponse>(responseJson, _jsonOptions);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _toastService.ShowError("Ошибка обновления", $"Код ошибки: {response.StatusCode}");
                Console.WriteLine($"PUT {endpoint} failed: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            _toastService.ShowError("Ошибка соединения", "Не удалось подключиться к серверу");
            Console.WriteLine($"Error in PUT {endpoint}: {ex.Message}");
        }
        return default;
    }

    private async Task<bool> DeleteAsync(string endpoint)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(endpoint);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _toastService.ShowError("Ошибка удаления", $"Код ошибки: {response.StatusCode}");
                Console.WriteLine($"DELETE {endpoint} failed: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            _toastService.ShowError("Ошибка соединения", "Не удалось подключиться к серверу");
            Console.WriteLine($"Error in DELETE {endpoint}: {ex.Message}");
        }
        return false;
    }

    private async Task SetAuthenticationHeaderAsync(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        // Сохраняем токен в localStorage
        await _localStorage.SetAuthTokenAsync(token);
    }

    private async Task SetAuthorizationHeaderAsync()
    {
        var userData = await _localStorage.GetUserDataAsync();
        if (userData.IsAuthenticated && !string.IsNullOrEmpty(userData.Token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userData.Token);
        }
    }
} 