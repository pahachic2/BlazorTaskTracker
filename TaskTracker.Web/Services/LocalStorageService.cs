using Microsoft.JSInterop;
using TaskTracker.Models;

namespace TaskTracker.Web.Services;

/// <summary>
/// Сервис для работы с локальным хранилищем браузера
/// </summary>
public class LocalStorageService : ILocalStorageService
{
    private readonly IJSRuntime _jsRuntime;
    
    // Константы для ключей
    private const string AUTH_TOKEN_KEY = "authToken";
    private const string USERNAME_KEY = "username";
    private const string EMAIL_KEY = "email";

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task SetItemAsync(string key, string value)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
    }

    public async Task<string?> GetItemAsync(string key)
    {
        return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
    }

    public async Task RemoveItemAsync(string key)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }

    public async Task ClearAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.clear");
    }

    public async Task<bool> ContainsKeyAsync(string key)
    {
        var value = await GetItemAsync(key);
        return !string.IsNullOrEmpty(value);
    }

    public async Task SetAuthTokenAsync(string token)
    {
        await SetItemAsync(AUTH_TOKEN_KEY, token);
    }

    public async Task<string?> GetAuthTokenAsync()
    {
        return await GetItemAsync(AUTH_TOKEN_KEY);
    }

    public async Task SetUserDataAsync(string username, string email)
    {
        await SetItemAsync(USERNAME_KEY, username);
        await SetItemAsync(EMAIL_KEY, email);
    }

    public async Task<string?> GetUsernameAsync()
    {
        return await GetItemAsync(USERNAME_KEY);
    }

    public async Task<string?> GetEmailAsync()
    {
        return await GetItemAsync(EMAIL_KEY);
    }

    public async Task<UserData> GetUserDataAsync()
    {
        return new UserData
        {
            Token = await GetAuthTokenAsync(),
            Username = await GetUsernameAsync(),
            Email = await GetEmailAsync()
        };
    }

    public async Task ClearAuthDataAsync()
    {
        await RemoveItemAsync(AUTH_TOKEN_KEY);
        await RemoveItemAsync(USERNAME_KEY);
        await RemoveItemAsync(EMAIL_KEY);
    }
} 