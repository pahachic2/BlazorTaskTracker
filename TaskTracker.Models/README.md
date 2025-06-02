# TaskTracker.Models

Проект содержит все модели данных для TaskTracker, приведенные к единому стандарту MongoDB.

## Структура моделей

### 🗄️ Основные модели (MongoDB)

#### `User.cs`
Модель пользователя для хранения в MongoDB:
- Идентификатор пользователя
- Имя пользователя, email, хеш пароля
- Даты создания и статус активности
- MongoDB атрибуты для правильной сериализации

#### `Project.cs`
Модель проекта для хранения в MongoDB:
- Идентификатор, название, описание
- Цвет и иконка для UI
- Список участников и владелец
- Счетчик задач и статус активности
- Даты создания и обновления

#### `KanbanModels.cs`
Модели для канбан доски:

**KanbanColumn:**
- Колонка канбан доски
- Связь с проектом и порядок сортировки
- `Tasks` помечено как `[BsonIgnore]` - заполняется при загрузке

**KanbanTask:**
- Задача канбан доски
- Заголовок, описание, теги, исполнители
- Приоритет, статус, сроки выполнения
- Связи с проектом, колонкой и создателем
- Порядок сортировки

#### `UserProject.cs`
Модель связи пользователя с проектом:
- Роль пользователя в проекте (Owner, Admin, Member, Viewer)
- Дата присоединения и статус активности

#### Перечисления:
- `TaskPriority` - приоритеты задач (Low, Medium, High, Critical)
- `TaskStatus` - статусы задач (ToDo, InProgress, Review, Done, Archived)
- `ProjectRole` - роли в проекте (Owner, Admin, Member, Viewer)

### 💾 Модели LocalStorage

#### `UserData.cs`
Модель для хранения данных пользователя в localStorage браузера:
- Токен авторизации, имя пользователя, email
- Флаг авторизации `IsAuthenticated`
- Используется сервисом `LocalStorageService`

### 📡 DTO модели для API

#### `DTOs/AuthDTOs.cs`
DTO для авторизации:
- `LoginRequest`, `RegisterRequest`
- `AuthResponse`

#### `DTOs/ProjectDTOs.cs`
DTO для работы с проектами:
- `CreateProjectRequest`, `UpdateProjectRequest`
- `ProjectResponse`

#### `DTOs/TaskDTOs.cs`
DTO для работы с задачами и колонками:
- `CreateTaskRequest`, `UpdateTaskRequest`, `MoveTaskRequest`
- `TaskResponse`
- `CreateColumnRequest`, `UpdateColumnRequest`
- `ColumnResponse`

### 🔧 Служебные классы

#### `TaskMovedEventArgs`
Класс для передачи событий перемещения задач между колонками в UI.

## Принципы архитектуры

### ✅ MongoDB Совместимость
- Все основные модели используют MongoDB.Bson атрибуты
- `[BsonId]` для идентификаторов
- `[BsonElement("name")]` для полей
- `[BsonIgnore]` для полей, не хранящихся в БД
- `DateTime.UtcNow` для временных меток

### ✅ Разделение ответственности
- **Модели сущностей** - для хранения в MongoDB
- **DTO модели** - для API взаимодействия
- **UI модели** - для работы с пользовательским интерфейсом

### ✅ Валидация
- DTO содержат атрибуты валидации `[Required]`, `[StringLength]`
- Модели сущностей сосредоточены на структуре данных

### ✅ Типобезопасность
- Использование перечислений для статусов и ролей
- Строгая типизация для всех свойств
- Nullable типы где необходимо

## Использование

```csharp
// Создание новой задачи
var task = new KanbanTask
{
    Title = "Новая задача",
    Description = "Описание задачи",
    Priority = TaskPriority.High,
    Status = TaskStatus.ToDo,
    ProjectId = projectId,
    ColumnId = columnId,
    CreatedBy = userId
};

// DTO для API
var createRequest = new CreateTaskRequest
{
    Title = task.Title,
    Description = task.Description,
    Priority = task.Priority,
    ProjectId = task.ProjectId,
    ColumnId = task.ColumnId
};
```

## Миграция

При переходе на новую структуру:
1. Все `DateTime.Now` заменены на `DateTime.UtcNow`
2. Добавлены MongoDB атрибуты к существующим моделям
3. Созданы DTO для четкого разделения API и данных
4. Удалены дублирующиеся модели из Web проекта

Это обеспечивает:
- Консистентность данных
- Простоту тестирования
- Возможность переиспользования моделей
- Готовность к интеграции с MongoDB 