# LocalStorageService

Сервис для работы с локальным хранилищем браузера в Blazor WebAssembly приложении.

## Описание

`LocalStorageService` предоставляет удобный интерфейс для работы с localStorage браузера, инкапсулируя вызовы JavaScript через IJSRuntime.

## Возможности

- ✅ Универсальные методы для работы с localStorage
- ✅ Специализированные методы для данных авторизации
- ✅ Асинхронные операции
- ✅ Типобезопасность
- ✅ Инкапсуляция ключей localStorage
- ✅ Интеграция с DI контейнером

## Регистрация в DI

```csharp
// Program.cs
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
```

## Использование

### Базовые операции

```csharp
@inject ILocalStorageService LocalStorage

// Сохранение данных
await LocalStorage.SetItemAsync("myKey", "myValue");

// Получение данных
var value = await LocalStorage.GetItemAsync("myKey");

// Проверка существования ключа
var exists = await LocalStorage.ContainsKeyAsync("myKey");

// Удаление данных
await LocalStorage.RemoveItemAsync("myKey");

// Очистка всего localStorage
await LocalStorage.ClearAsync();
```

### Работа с данными авторизации

```csharp
// Сохранение токена
await LocalStorage.SetAuthTokenAsync(token);

// Получение токена
var token = await LocalStorage.GetAuthTokenAsync();

// Сохранение данных пользователя
await LocalStorage.SetUserDataAsync(username, email);

// Получение всех данных пользователя
var userData = await LocalStorage.GetUserDataAsync();

// Очистка всех данных авторизации
await LocalStorage.ClearAuthDataAsync();
```

### Пример компонента

```razor
@inject ILocalStorageService LocalStorage
@inject NavigationManager Navigation

@code {
    private UserData userData = UserData.Empty;

    protected override async Task OnInitializedAsync()
    {
        // Получаем данные пользователя при инициализации
        userData = await LocalStorage.GetUserDataAsync();
        
        // Перенаправляем на логин, если пользователь не авторизован
        if (!userData.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
        }
    }
    
    private async Task Logout()
    {
        await LocalStorage.ClearAuthDataAsync();
        Navigation.NavigateTo("/login");
    }
}
```

## API

### ILocalStorageService

#### Базовые методы
- `SetItemAsync(string key, string value)` - Сохранить значение
- `GetItemAsync(string key)` - Получить значение
- `RemoveItemAsync(string key)` - Удалить значение
- `ClearAsync()` - Очистить все данные
- `ContainsKeyAsync(string key)` - Проверить существование ключа

#### Методы для авторизации
- `SetAuthTokenAsync(string token)` - Сохранить токен
- `GetAuthTokenAsync()` - Получить токен
- `SetUserDataAsync(string username, string email)` - Сохранить данные пользователя
- `GetUsernameAsync()` - Получить имя пользователя
- `GetEmailAsync()` - Получить email
- `GetUserDataAsync()` - Получить все данные пользователя
- `ClearAuthDataAsync()` - Очистить данные авторизации

### UserData

Модель для данных пользователя (находится в `TaskTracker.Models`):

```csharp
public class UserData
{
    public string? Token { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);
    public static UserData Empty => new UserData();
}
```

## Ключи localStorage

Сервис использует следующие ключи:
- `authToken` - токен авторизации
- `username` - имя пользователя
- `email` - email пользователя

## Преимущества

1. **Инкапсуляция** - скрывает детали работы с IJSRuntime
2. **Типобезопасность** - статическая типизация вместо строковых ключей
3. **Переиспользование** - единый интерфейс для всех компонентов
4. **Тестируемость** - легко мокается для unit тестов
5. **Удобство** - специализированные методы для частых операций
6. **Совместимость** - модели доступны во всех проектах решения

## Обработка ошибок

Все методы сервиса являются асинхронными и могут выбрасывать исключения при проблемах с JavaScript. Рекомендуется оборачивать вызовы в try-catch блоки для критичного кода.

```csharp
try
{
    var token = await LocalStorage.GetAuthTokenAsync();
    // обработка успешного получения
}
catch (Exception ex)
{
    // обработка ошибки
    Console.WriteLine($"Ошибка получения токена: {ex.Message}");
}
```

## Структура проекта

После переноса моделей структура выглядит следующим образом:

```
TaskTracker.Models/           # 🔗 Общие модели для всех проектов
├── UserData.cs              # Модель данных пользователя
├── Project.cs               # Модель проекта
├── KanbanModels.cs          # Модели канбан доски
└── DTOs/                    # DTO для API
    └── AuthDTOs.cs

TaskTracker.Web/             # 🌐 Web приложение
├── Services/                # Сервисы только для Web
│   ├── ILocalStorageService.cs
│   ├── LocalStorageService.cs
│   └── README.md
└── Components/              # Blazor компоненты
```

Это позволяет:
- Использовать модели в API проекте для валидации
- Общий доступ к моделям из всех проектов
- Лучшая архитектура и разделение ответственности

```csharp
try
{
    var token = await LocalStorage.GetAuthTokenAsync();
    // обработка успешного получения
}
catch (Exception ex)
{
    // обработка ошибки
    Console.WriteLine($"Ошибка получения токена: {ex.Message}");
}
``` 