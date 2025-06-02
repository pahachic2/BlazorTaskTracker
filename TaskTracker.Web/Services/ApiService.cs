using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TaskTracker.Models.DTOs;

namespace TaskTracker.Web.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
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
        }
        return response;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        var response = await PostAsync<RegisterRequest, AuthResponse>("/api/auth/register", request);
        if (response != null)
        {
            await SetAuthenticationHeaderAsync(response.Token);
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
        return await PostAsync<CreateProjectRequest, ProjectResponse>("/api/projects", request);
    }

    public async Task<ProjectResponse?> UpdateProjectAsync(string projectId, UpdateProjectRequest request)
    {
        await SetAuthorizationHeaderAsync();
        return await PutAsync<UpdateProjectRequest, ProjectResponse>($"/api/projects/{projectId}", request);
    }

    public async Task<bool> DeleteProjectAsync(string projectId)
    {
        await SetAuthorizationHeaderAsync();
        return await DeleteAsync($"/api/projects/{projectId}");
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
        return await PostAsync<CreateColumnRequest, ColumnResponse>("/api/kanban/columns", request);
    }

    public async Task<ColumnResponse?> UpdateColumnAsync(string columnId, UpdateColumnRequest request)
    {
        await SetAuthorizationHeaderAsync();
        return await PutAsync<UpdateColumnRequest, ColumnResponse>($"/api/kanban/columns/{columnId}", request);
    }

    public async Task<bool> DeleteColumnAsync(string columnId)
    {
        await SetAuthorizationHeaderAsync();
        return await DeleteAsync($"/api/kanban/columns/{columnId}");
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
        return await PostAsync<CreateTaskRequest, TaskResponse>("/api/kanban/tasks", request);
    }

    public async Task<TaskResponse?> UpdateTaskAsync(string taskId, UpdateTaskRequest request)
    {
        await SetAuthorizationHeaderAsync();
        return await PutAsync<UpdateTaskRequest, TaskResponse>($"/api/kanban/tasks/{taskId}", request);
    }

    public async Task<bool> DeleteTaskAsync(string taskId)
    {
        await SetAuthorizationHeaderAsync();
        return await DeleteAsync($"/api/kanban/tasks/{taskId}");
    }

    public async Task<TaskResponse?> MoveTaskAsync(string taskId, MoveTaskRequest request)
    {
        await SetAuthorizationHeaderAsync();
        return await PutAsync<MoveTaskRequest, TaskResponse>($"/api/kanban/tasks/{taskId}/move", request);
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
        }
        catch (Exception ex)
        {
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
        }
        catch (Exception ex)
        {
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
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in PUT {endpoint}: {ex.Message}");
        }
        return default;
    }

    private async Task<bool> DeleteAsync(string endpoint)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in DELETE {endpoint}: {ex.Message}");
            return false;
        }
    }

    private async Task SetAuthenticationHeaderAsync(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        // Сохраняем токен в localStorage
        await _localStorage.SetAuthTokenAsync(token);
    }

    private async Task SetAuthorizationHeaderAsync()
    {
        var token = await _localStorage.GetAuthTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
} 