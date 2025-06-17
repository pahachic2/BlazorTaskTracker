using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Net;
using TaskTracker.Models;
using TaskTracker.Models.DTOs;
using TaskTracker.Web.Services;
using Microsoft.AspNetCore.Components;

namespace TaskTracker.Web.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly IToastService _toastService;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly NavigationManager _navigationManager;

    public ApiService(HttpClient httpClient, ILocalStorageService localStorage, IToastService toastService, NavigationManager navigationManager)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _toastService = toastService;
        _navigationManager = navigationManager;
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

    // Методы для организаций (временные заглушки для демо)
    public async Task<List<OrganizationResponse>> GetUserOrganizationsAsync()
    {
        await SetAuthorizationHeaderAsync();
        return await GetAsync<List<OrganizationResponse>>("/api/organizations") ?? new List<OrganizationResponse>();
    }

    public async Task<OrganizationResponse?> GetOrganizationByIdAsync(string organizationId)
    {
        await SetAuthorizationHeaderAsync();
        return await GetAsync<OrganizationResponse>($"/api/organizations/{organizationId}");
    }

    public async Task<OrganizationResponse?> CreateOrganizationAsync(CreateOrganizationRequest request)
    {
        await SetAuthorizationHeaderAsync();
        var response = await PostAsync<CreateOrganizationRequest, OrganizationResponse>("/api/organizations", request);
        if (response != null)
        {
            _toastService.ShowSuccess("Организация создана!", $"Организация \"{response.Name}\" успешно создана");
        }
        return response;
    }

    public async Task<OrganizationResponse?> UpdateOrganizationAsync(string organizationId, UpdateOrganizationRequest request)
    {
        await SetAuthorizationHeaderAsync();
        var response = await PutAsync<UpdateOrganizationRequest, OrganizationResponse>($"/api/organizations/{organizationId}", request);
        if (response != null)
        {
            _toastService.ShowSuccess("Организация обновлена!", $"Организация \"{response.Name}\" успешно обновлена");
        }
        return response;
    }

    public async Task<bool> DeleteOrganizationAsync(string organizationId)
    {
        await SetAuthorizationHeaderAsync();
        var success = await DeleteAsync($"/api/organizations/{organizationId}");
        if (success)
        {
            _toastService.ShowSuccess("Организация удалена!", "Организация успешно удалена");
        }
        return success;
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

    // НОВЫЕ МЕТОДЫ ДЛЯ СИСТЕМЫ ПРИГЛАШЕНИЙ

    // Участники организации
    public async Task<List<OrganizationMemberResponse>> GetOrganizationMembersAsync(string organizationId)
    {
        await SetAuthorizationHeaderAsync();
        return await GetAsync<List<OrganizationMemberResponse>>($"/api/organizations/{organizationId}/members") ?? new List<OrganizationMemberResponse>();
    }

    // Приглашения (авторизованные методы)
    public async Task<InvitationResponse?> InviteUserAsync(string organizationId, InviteUserRequest request)
    {
        await SetAuthorizationHeaderAsync();
        var response = await PostAsync<InviteUserRequest, InvitationResponse>($"/api/organizations/{organizationId}/invitations", request);
        if (response != null)
        {
            if (response.UserWasRegistered && !response.EmailSent)
            {
                _toastService.ShowSuccess("Приглашение создано!", $"Пользователь {request.Email} увидит приглашение в интерфейсе при входе в систему");
            }
            else if (!response.UserWasRegistered && response.EmailSent)
            {
                _toastService.ShowSuccess("Приглашение отправлено!", $"Email с приглашением отправлен на {request.Email}");
            }
            else
            {
                _toastService.ShowSuccess("Приглашение отправлено!", $"Приглашение отправлено на {request.Email}");
            }
        }
        return response;
    }

    public async Task<List<InvitationResponse>> GetOrganizationInvitationsAsync(string organizationId)
    {
        await SetAuthorizationHeaderAsync();
        return await GetAsync<List<InvitationResponse>>($"/api/organizations/{organizationId}/invitations") ?? new List<InvitationResponse>();
    }

    public async Task<bool> RevokeInvitationAsync(string invitationId)
    {
        await SetAuthorizationHeaderAsync();
        var success = await DeleteAsync($"/api/organizations/invitations/{invitationId}");
        if (success)
        {
            _toastService.ShowSuccess("Приглашение отозвано!", "Приглашение успешно отменено");
        }
        return success;
    }

    // Публичные методы для обработки приглашений
    public async Task<InvitationInfoResponse?> GetInvitationInfoAsync(string token)
    {
        return await GetAsync<InvitationInfoResponse>($"/api/invitations/{token}");
    }

    public async Task<AcceptInvitationResponse?> AcceptInvitationAsync(AcceptInvitationRequest request)
    {
        Console.WriteLine($"🌐 API_SERVICE: Начинаем принятие приглашения с токеном {request.Token[..10]}...");
        Console.WriteLine($"🌐 API_SERVICE: URL: /api/organizations/invitations/{request.Token}/accept");
        
        // Endpoint теперь требует авторизации и находится в группе organizations
        var response = await PostAsync<AcceptInvitationRequest, AcceptInvitationResponse>($"/api/organizations/invitations/{request.Token}/accept", request);
        
        Console.WriteLine($"🌐 API_SERVICE: Получили ответ. Response: {response != null}, Success: {response?.Success}");
        
        if (response != null)
        {
            Console.WriteLine($"🌐 API_SERVICE: Показываем toast успеха");
            _toastService.ShowSuccess("Приглашение принято!", $"Добро пожаловать в организацию {response.Organization?.Name ?? "организацию"}!");
        }
        else
        {
            Console.WriteLine($"❌ API_SERVICE: Ответ пустой или null");
        }
        
        return response;
    }

    public async Task<bool> DeclineInvitationAsync(DeclineInvitationRequest request)
    {
        // Для публичных endpoints не устанавливаем заголовок авторизации
        var success = await PostAsyncPublic<DeclineInvitationRequest, object>($"/api/invitations/{request.Token}/decline", request) != null;
        if (success)
        {
            _toastService.ShowInfo("Приглашение отклонено", "Вы отклонили приглашение в организацию");
        }
        return success;
    }

    // НОВЫЙ МЕТОД: Получить приглашения текущего пользователя
    public async Task<List<InvitationResponse>> GetUserInvitationsAsync()
    {
        try
        {
            Console.WriteLine("🌐 API: Запрос приглашений пользователя");
            await SetAuthorizationHeaderAsync();
            
            var result = await GetAsync<List<InvitationResponse>>("/api/organizations/user/invitations") ?? new List<InvitationResponse>();
            Console.WriteLine($"🌐 API: Получено {result.Count} приглашений от сервера");
            
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ API: Ошибка при получении приглашений: {ex.Message}");
            throw;
        }
    }

    // Поиск и добавление пользователей

    public async Task<UserSearchResponse?> SearchUserByEmailAsync(string organizationId, string email)
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            Console.WriteLine($"🔍 API CLIENT: Поиск пользователя по email: {email}");
            
            var response = await _httpClient.GetAsync($"/api/organizations/{organizationId}/search-user?email={Uri.EscapeDataString(email)}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<UserSearchResponse>(json, _jsonOptions);
                Console.WriteLine($"✅ API CLIENT: Поиск завершен. Найден: {result?.Found}");
                return result;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ API CLIENT: Ошибка поиска пользователя: {response.StatusCode} - {errorContent}");
                return new UserSearchResponse { Found = false, User = null };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ API CLIENT: Исключение при поиске пользователя {email}: {ex.Message}");
            return new UserSearchResponse { Found = false, User = null };
        }
    }

    public async Task<bool> AddExistingUserAsync(string organizationId, string userId, OrganizationRole role = OrganizationRole.Member)
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            Console.WriteLine($"➕ API CLIENT: Добавление пользователя {userId} в организацию {organizationId}");
            
            var request = new { UserId = userId, Role = role };
            var response = await PostAsync<object, object>($"/api/organizations/{organizationId}/add-user", request);
            
            if (response != null)
            {
                Console.WriteLine($"✅ API CLIENT: Пользователь {userId} успешно добавлен");
                _toastService.ShowSuccess("Пользователь добавлен!", "Пользователь успешно добавлен в организацию");
                return true;
            }
            
            Console.WriteLine($"❌ API CLIENT: Не удалось добавить пользователя");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ API CLIENT: Исключение при добавлении пользователя {userId}: {ex.Message}");
            _toastService.ShowError("Ошибка добавления", ex.Message);
            return false;
        }
    }

    // Вспомогательные методы
    private async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            Console.WriteLine($"🌐 API: GET запрос к {endpoint}");
            Console.WriteLine($"🌐 API: BaseAddress = {_httpClient.BaseAddress}");
            
            // Проверяем наличие заголовка авторизации
            if (_httpClient.DefaultRequestHeaders.Authorization != null)
            {
                Console.WriteLine($"🌐 API: Authorization header = {_httpClient.DefaultRequestHeaders.Authorization.Scheme} {_httpClient.DefaultRequestHeaders.Authorization.Parameter?[..10]}...");
            }
            else
            {
                Console.WriteLine("⚠️ API: Authorization header отсутствует!");
            }

            var response = await _httpClient.GetAsync(endpoint);
            
            Console.WriteLine($"🌐 API: Статус ответа: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"🌐 API: Получен ответ длиной {content.Length} символов");
                var result = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                return result;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"❌ API: Ошибка {response.StatusCode}: {errorContent}");
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("🔒 API: Пользователь не авторизован - перенаправляем на логин");
                _navigationManager.NavigateTo("/login");
            }
            
            return default;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ API: Исключение при GET {endpoint}: {ex.Message}");
            Console.WriteLine($"❌ API: Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        try
        {
            Console.WriteLine($"🌐 API: POST запрос к {endpoint}");
            await SetAuthorizationHeaderAsync();
            
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            Console.WriteLine($"🌐 API: Данные для отправки: {json}");
            
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            Console.WriteLine($"🌐 API: Отправляем POST запрос...");
            var response = await _httpClient.PostAsync(endpoint, content);
            Console.WriteLine($"🌐 API: Получили ответ со статусом: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"🌐 API: Успешный ответ: {responseJson}");
                var result = JsonSerializer.Deserialize<TResponse>(responseJson, _jsonOptions);
                return result;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ API: Ошибка {response.StatusCode}: {errorContent}");
                _toastService.ShowError("Ошибка создания", $"Код ошибки: {response.StatusCode}");
                Console.WriteLine($"POST {endpoint} failed: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ API: Исключение в POST {endpoint}: {ex.Message}");
            Console.WriteLine($"❌ API: Stack trace: {ex.StackTrace}");
            _toastService.ShowError("Ошибка соединения", "Не удалось подключиться к серверу");
            Console.WriteLine($"Error in POST {endpoint}: {ex.Message}");
        }
        return default;
    }

    private async Task<TResponse?> PostAsyncPublic<TRequest, TResponse>(string endpoint, TRequest data)
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