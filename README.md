# Blazor Task Tracker

Веб-приложение для отслеживания задач, созданное на C# и Blazor.

## Технологии

- **C# .NET 9.0** - основной язык программирования
- **Blazor Server** - фреймворк для создания интерактивных веб-приложений
- **ASP.NET Core** - веб-фреймворк

## Структура проекта

```
BlazorTaskTracker/
├── Components/
│   ├── Layout/          # Компоненты макета
│   ├── Pages/           # Страницы приложения
│   ├── App.razor        # Корневой компонент
│   ├── Routes.razor     # Конфигурация маршрутов
│   └── _Imports.razor   # Глобальные импорты
├── wwwroot/             # Статические файлы
├── Properties/          # Свойства проекта
├── Program.cs           # Точка входа приложения
├── appsettings.json     # Конфигурация приложения
└── BlazorTaskTracker.csproj  # Файл проекта

```

## Запуск проекта

### Предварительные требования

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) или выше

### Команды для запуска

1. **Сборка проекта:**
   ```bash
   dotnet build
   ```

2. **Запуск в режиме разработки:**
   ```bash
   dotnet run
   ```

3. **Запуск с автоматической перезагрузкой:**
   ```bash
   dotnet watch run
   ```

4. **Быстрый запуск (Windows):**
   ```bash
   start.bat
   ```

После запуска приложение будет доступно по адресу:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`

## Возможности

Приложение включает:
- 🎉 **Красивое стартовое окно "Hello World"** с современным дизайном
- 🏠 Главная страница с градиентным фоном и анимациями
- 🔢 Счетчик (пример интерактивного компонента)
- 🌤️ Прогноз погоды (пример работы с данными)
- 📱 Адаптивный дизайн
- ⚡ Интерактивность на стороне сервера
- 🎨 Bootstrap Icons для красивых иконок

## Разработка

Для начала разработки функций трекера задач можно:

1. Создать модели данных в папке `Models/`
2. Добавить новые страницы в `Components/Pages/`
3. Создать сервисы для работы с данными
4. Настроить базу данных (Entity Framework Core)

## Полезные команды

```bash
# Добавление нового пакета NuGet
dotnet add package PackageName

# Создание нового компонента Razor
dotnet new razorcomponent -n ComponentName

# Обновление зависимостей
dotnet restore

# Публикация приложения
dotnet publish -c Release
```

## Следующие шаги

- [ ] Добавить модели для задач
- [ ] Создать страницы управления задачами
- [ ] Настроить базу данных
- [ ] Добавить аутентификацию
- [ ] Реализовать CRUD операции