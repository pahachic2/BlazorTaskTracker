using TaskTracker.Models;
using TaskTracker.Models.DTOs;

namespace TaskTracker.Api.Services;

public class KanbanService : IKanbanService
{
    private readonly IDatabaseService<KanbanColumn> _columnDatabase;
    private readonly IDatabaseService<KanbanTask> _taskDatabase;
    private readonly IDatabaseService<Project> _projectDatabase;
    private readonly IDatabaseService<UserProject> _userProjectDatabase;
    private readonly IDatabaseService<UserOrganization> _userOrganizationDatabase;
    private readonly IDatabaseService<User> _userDatabase;

    public KanbanService(
        IDatabaseService<KanbanColumn> columnDatabase,
        IDatabaseService<KanbanTask> taskDatabase,
        IDatabaseService<Project> projectDatabase,
        IDatabaseService<UserProject> userProjectDatabase,
        IDatabaseService<UserOrganization> userOrganizationDatabase,
        IDatabaseService<User> userDatabase)
    {
        _columnDatabase = columnDatabase;
        _taskDatabase = taskDatabase;
        _projectDatabase = projectDatabase;
        _userProjectDatabase = userProjectDatabase;
        _userOrganizationDatabase = userOrganizationDatabase;
        _userDatabase = userDatabase;
    }

    // Методы для колонок
    public async Task<List<ColumnResponse>> GetProjectColumnsAsync(string projectId, string userId)
    {
        Console.WriteLine($"🔍 API: Запрос колонок для проекта {projectId} от пользователя {userId}");
        
        // Проверяем доступ к проекту
        if (!await HasProjectAccessAsync(projectId, userId))
        {
            Console.WriteLine($"❌ API: Нет доступа к проекту {projectId}");
            return new List<ColumnResponse>();
        }

        var columns = await _columnDatabase.FindAsync(c => c.ProjectId == projectId);
        var sortedColumns = columns.OrderBy(c => c.Order).ToList();

        Console.WriteLine($"📂 API: Найдено {sortedColumns.Count} колонок для проекта {projectId}");

        var result = new List<ColumnResponse>();
        foreach (var column in sortedColumns)
        {
            var tasks = await _taskDatabase.FindAsync(t => t.ColumnId == column.Id);
            var sortedTasks = tasks.OrderBy(t => t.Order).ToList();

            Console.WriteLine($"📋 API: Колонка '{column.Title}' (ID: {column.Id}) содержит {sortedTasks.Count} задач:");
            foreach (var task in sortedTasks)
            {
                Console.WriteLine($"   📝 API: Задача '{task.Title}' (ID: {task.Id})");
            }

            result.Add(new ColumnResponse
            {
                Id = column.Id,
                Title = column.Title,
                ProjectId = column.ProjectId,
                Order = column.Order,
                CreatedAt = column.CreatedAt,
                UpdatedAt = column.UpdatedAt,
                Tasks = (await Task.WhenAll(sortedTasks.Select(async t => await MapTaskToResponseAsync(t)))).ToList()
            });
        }

        Console.WriteLine($"✅ API: Возвращаем {result.Count} колонок с общим количеством задач: {result.Sum(c => c.Tasks.Count)}");
        return result;
    }

    public async Task<ColumnResponse> CreateColumnAsync(CreateColumnRequest request, string userId)
    {
        // Проверяем доступ к проекту
        if (!await HasProjectAccessAsync(request.ProjectId, userId))
            throw new UnauthorizedAccessException("Нет доступа к проекту");

        var column = new KanbanColumn
        {
            Title = request.Title,
            ProjectId = request.ProjectId,
            Order = request.Order,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _columnDatabase.CreateAsync(column);

        return new ColumnResponse
        {
            Id = column.Id,
            Title = column.Title,
            ProjectId = column.ProjectId,
            Order = column.Order,
            CreatedAt = column.CreatedAt,
            UpdatedAt = column.UpdatedAt,
            Tasks = new List<TaskResponse>()
        };
    }

    public async Task<ColumnResponse?> UpdateColumnAsync(string columnId, UpdateColumnRequest request, string userId)
    {
        var column = await _columnDatabase.GetByIdAsync(columnId);
        if (column == null)
            return null;

        // Проверяем доступ к проекту
        if (!await HasProjectAccessAsync(column.ProjectId, userId))
            return null;

        column.Title = request.Title;
        column.Order = request.Order;
        column.UpdatedAt = DateTime.UtcNow;

        await _columnDatabase.UpdateAsync(columnId, column);

        var tasks = await _taskDatabase.FindAsync(t => t.ColumnId == columnId);
        
        return new ColumnResponse
        {
            Id = column.Id,
            Title = column.Title,
            ProjectId = column.ProjectId,
            Order = column.Order,
            CreatedAt = column.CreatedAt,
            UpdatedAt = column.UpdatedAt,
            Tasks = (await Task.WhenAll(tasks.OrderBy(t => t.Order).Select(async t => await MapTaskToResponseAsync(t)))).ToList()
        };
    }

    public async Task<bool> DeleteColumnAsync(string columnId, string userId)
    {
        var column = await _columnDatabase.GetByIdAsync(columnId);
        if (column == null)
            return false;

        // Проверяем доступ к проекту
        if (!await HasProjectAccessAsync(column.ProjectId, userId))
            return false;

        // Удаляем все задачи в колонке
        var tasks = await _taskDatabase.FindAsync(t => t.ColumnId == columnId);
        foreach (var task in tasks)
        {
            await _taskDatabase.DeleteAsync(task.Id);
        }

        await _columnDatabase.DeleteAsync(columnId);
        return true;
    }

    // Методы для задач
    public async Task<List<TaskResponse>> GetColumnTasksAsync(string columnId, string userId)
    {
        var column = await _columnDatabase.GetByIdAsync(columnId);
        if (column == null)
            return new List<TaskResponse>();

        // Проверяем доступ к проекту
        if (!await HasProjectAccessAsync(column.ProjectId, userId))
            return new List<TaskResponse>();

        var tasks = await _taskDatabase.FindAsync(t => t.ColumnId == columnId);
        var sortedTasks = tasks.OrderBy(t => t.Order);
        return (await Task.WhenAll(sortedTasks.Select(async t => await MapTaskToResponseAsync(t)))).ToList();
    }

    public async Task<TaskResponse?> GetTaskByIdAsync(string taskId, string userId)
    {
        var task = await _taskDatabase.GetByIdAsync(taskId);
        if (task == null)
        {
            Console.WriteLine($"❌ API: Задача {taskId} не найдена в базе данных");
            
            // Дополнительная диагностика - посмотрим все задачи в базе
            var allTasks = await _taskDatabase.FindAsync(t => true);
            Console.WriteLine($"📊 API: Всего задач в базе данных: {allTasks.Count()}");
            
            if (allTasks.Any())
            {
                Console.WriteLine($"📋 API: Первые 5 задач в базе:");
                foreach (var t in allTasks.Take(5))
                {
                    Console.WriteLine($"   - ID: {t.Id}, Title: {t.Title}, Project: {t.ProjectId}, Column: {t.ColumnId}");
                }
            }
            
            return null;
        }

        // Проверяем доступ к проекту
        if (!await HasProjectAccessAsync(task.ProjectId, userId))
            return null;

        return await MapTaskToResponseAsync(task);
    }

    public async Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request, string userId)
    {
        var column = await _columnDatabase.GetByIdAsync(request.ColumnId);
        if (column == null)
            throw new ArgumentException("Колонка не найдена");

        // Проверяем доступ к проекту
        if (!await HasProjectAccessAsync(request.ProjectId, userId))
            throw new UnauthorizedAccessException("Нет доступа к проекту");

        // Валидируем и конвертируем исполнителей
        var validatedAssigneeIds = await ValidateAssigneesAsync(request.AssigneeIds ?? new List<string>(), request.ProjectId);

        var task = new KanbanTask
        {
            Title = request.Title,
            Description = request.Description ?? string.Empty,
            Tags = request.Tags,
            AssigneeIds = validatedAssigneeIds,
            DueDate = request.DueDate,
            ColumnId = request.ColumnId,
            ProjectId = request.ProjectId,
            CreatedBy = userId,
            Priority = request.Priority,
            Status = Models.TaskStatus.ToDo,
            Order = await GetNextTaskOrderAsync(request.ColumnId),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _taskDatabase.CreateAsync(task);
        Console.WriteLine($"✅ KANBAN: Создана задача {task.Id} с исполнителями: {string.Join(", ", validatedAssigneeIds)}");

        // Обновляем счетчик задач в проекте
        await UpdateProjectTaskCountAsync(request.ProjectId);

        return await MapTaskToResponseAsync(task);
    }

    public async Task<TaskResponse?> UpdateTaskAsync(string taskId, UpdateTaskRequest request, string userId)
    {
        var task = await _taskDatabase.GetByIdAsync(taskId);
        if (task == null)
            return null;

        // Проверяем доступ к проекту
        if (!await HasProjectAccessAsync(task.ProjectId, userId))
            return null;

        // Валидируем новых исполнителей
        var validatedAssigneeIds = await ValidateAssigneesAsync(request.AssigneeIds ?? new List<string>(), task.ProjectId);

        task.Title = request.Title;
        task.Description = request.Description ?? string.Empty;
        task.Tags = request.Tags;
        task.AssigneeIds = validatedAssigneeIds;
        task.DueDate = request.DueDate;
        task.Priority = request.Priority;
        task.Status = request.Status;
        task.UpdatedAt = DateTime.UtcNow;

        await _taskDatabase.UpdateAsync(taskId, task);
        Console.WriteLine($"✅ KANBAN: Обновлена задача {taskId} с исполнителями: {string.Join(", ", validatedAssigneeIds)}");

        return await MapTaskToResponseAsync(task);
    }

    public async Task<bool> DeleteTaskAsync(string taskId, string userId)
    {
        var task = await _taskDatabase.GetByIdAsync(taskId);
        if (task == null)
            return false;

        // Проверяем доступ к проекту
        if (!await HasProjectAccessAsync(task.ProjectId, userId))
            return false;

        await _taskDatabase.DeleteAsync(taskId);

        // Обновляем счетчик задач в проекте
        await UpdateProjectTaskCountAsync(task.ProjectId);

        return true;
    }

    public async Task<TaskResponse?> MoveTaskAsync(string taskId, MoveTaskRequest request, string userId)
    {
        Console.WriteLine($"🔍 API: Попытка перемещения задачи {taskId} в колонку {request.NewColumnId} пользователем {userId}");
        
        var task = await _taskDatabase.GetByIdAsync(taskId);
        if (task == null)
        {
            Console.WriteLine($"❌ API: Задача {taskId} не найдена в базе данных");
            return null;
        }

        Console.WriteLine($"✅ API: Задача {taskId} найдена в проекте {task.ProjectId}");

        // Проверяем доступ к проекту
        var hasAccess = await HasProjectAccessAsync(task.ProjectId, userId);
        if (!hasAccess)
        {
            Console.WriteLine($"❌ API: Пользователь {userId} не имеет доступа к проекту {task.ProjectId}");
            return null;
        }

        Console.WriteLine($"✅ API: Доступ к проекту {task.ProjectId} подтвержден");

        // Проверяем, что новая колонка существует и принадлежит тому же проекту
        var newColumn = await _columnDatabase.GetByIdAsync(request.NewColumnId);
        if (newColumn == null)
        {
            Console.WriteLine($"❌ API: Новая колонка {request.NewColumnId} не найдена");
            return null;
        }
        
        if (newColumn.ProjectId != task.ProjectId)
        {
            Console.WriteLine($"❌ API: Колонка {request.NewColumnId} принадлежит проекту {newColumn.ProjectId}, а задача - проекту {task.ProjectId}");
            return null;
        }

        Console.WriteLine($"✅ API: Новая колонка {request.NewColumnId} найдена и принадлежит тому же проекту");

        task.ColumnId = request.NewColumnId;
        task.Order = request.NewOrder;
        task.UpdatedAt = DateTime.UtcNow;

        // Обновляем статус задачи в зависимости от колонки
        var oldStatus = task.Status;
        task.Status = newColumn.Title.ToLower() switch
        {
            "to do" => Models.TaskStatus.ToDo,
            "in progress" => Models.TaskStatus.InProgress,
            "done" => Models.TaskStatus.Done,
            _ => task.Status
        };

        Console.WriteLine($"🔄 API: Обновление задачи - колонка: {task.ColumnId}, порядок: {task.Order}, статус: {oldStatus} -> {task.Status}");

        await _taskDatabase.UpdateAsync(taskId, task);

        Console.WriteLine($"✅ API: Задача {taskId} успешно перемещена в колонку {request.NewColumnId}");

        return await MapTaskToResponseAsync(task);
    }

    // Вспомогательные методы
    private async Task<bool> HasProjectAccessAsync(string projectId, string userId)
    {
        Console.WriteLine($"🔍 API: Проверка доступа пользователя {userId} к проекту {projectId}");
        
        // Сначала проверяем прямое членство в проекте (старая логика)
        var userProjects = await _userProjectDatabase.FindAsync(
            up => up.ProjectId == projectId && up.UserId == userId && up.IsActive);
        
        if (userProjects.Any())
        {
            Console.WriteLine($"✅ API: Найден прямой доступ к проекту через UserProject");
            return true;
        }
        
        // Теперь проверяем доступ через организацию (новая логика)
        var project = await _projectDatabase.GetByIdAsync(projectId);
        if (project == null)
        {
            Console.WriteLine($"❌ API: Проект {projectId} не найден");
            return false;
        }
        
        Console.WriteLine($"🔍 API: Проект принадлежит организации {project.OrganizationId}");
        
        // Проверяем, является ли пользователь членом организации
        var userOrganizations = await _userOrganizationDatabase.FindAsync(
            uo => uo.OrganizationId == project.OrganizationId && uo.UserId == userId && uo.IsActive);
        
        var hasOrgAccess = userOrganizations.Any();
        Console.WriteLine($"🔍 API: Доступ к организации {project.OrganizationId}: {hasOrgAccess}");
        
        if (!hasOrgAccess)
        {
            // Дополнительная диагностика
            var allUserProjects = await _userProjectDatabase.FindAsync(up => up.UserId == userId);
            var allUserOrgs = await _userOrganizationDatabase.FindAsync(uo => uo.UserId == userId);
            Console.WriteLine($"📊 API: Всего проектов у пользователя {userId}: {allUserProjects.Count()}");
            Console.WriteLine($"📊 API: Всего организаций у пользователя {userId}: {allUserOrgs.Count()}");
            
            var projectConnections = await _userProjectDatabase.FindAsync(up => up.ProjectId == projectId);
            Console.WriteLine($"📊 API: Всего пользователей в проекте {projectId}: {projectConnections.Count()}");
        }
        
        return hasOrgAccess;
    }

    private async Task<int> GetNextTaskOrderAsync(string columnId)
    {
        var tasks = await _taskDatabase.FindAsync(t => t.ColumnId == columnId);
        return tasks.Any() ? tasks.Max(t => t.Order) + 1 : 0;
    }

    private async Task UpdateProjectTaskCountAsync(string projectId)
    {
        var tasks = await _taskDatabase.FindAsync(t => t.ProjectId == projectId);
        var project = await _projectDatabase.GetByIdAsync(projectId);
        
        if (project != null)
        {
            project.TaskCount = tasks.Count();
            project.UpdatedAt = DateTime.UtcNow;
            await _projectDatabase.UpdateAsync(projectId, project);
        }
    }

    private static TaskResponse MapTaskToResponse(KanbanTask task)
    {
        return new TaskResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Tags = task.Tags,
            Assignees = task.Assignees,
            DueDate = task.DueDate,
            ColumnId = task.ColumnId,
            ProjectId = task.ProjectId,
            CreatedBy = task.CreatedBy,
            Order = task.Order,
            Priority = task.Priority,
            Status = task.Status,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }

    private async Task<List<string>> ValidateAssigneesAsync(List<string> assigneeIds, string projectId)
    {
        var validatedAssigneeIds = new List<string>();
        var project = await _projectDatabase.GetByIdAsync(projectId);

        if (project == null)
        {
            Console.WriteLine($"❌ KANBAN: Проект {projectId} не найден для валидации исполнителей");
            return validatedAssigneeIds;
        }

        Console.WriteLine($"🔍 KANBAN: Валидация {assigneeIds.Count} исполнителей для проекта {projectId}");

        foreach (var assigneeId in assigneeIds)
        {
            var user = await _userDatabase.GetByIdAsync(assigneeId);
            if (user == null)
            {
                Console.WriteLine($"❌ KANBAN: Пользователь {assigneeId} не найден, пропускаем");
                continue;
            }

            // Проверяем доступ к проекту через организацию
            var userOrganizations = await _userOrganizationDatabase.FindAsync(
                uo => uo.OrganizationId == project.OrganizationId && uo.UserId == assigneeId && uo.IsActive);
            
            if (userOrganizations.Any())
            {
                validatedAssigneeIds.Add(assigneeId);
                Console.WriteLine($"✅ KANBAN: Пользователь {user.Username} ({assigneeId}) добавлен к исполнителям");
            }
            else
            {
                Console.WriteLine($"❌ KANBAN: Пользователь {user.Username} ({assigneeId}) не является членом организации {project.OrganizationId}");
            }
        }

        Console.WriteLine($"✅ KANBAN: Валидировано {validatedAssigneeIds.Count} из {assigneeIds.Count} исполнителей");
        return validatedAssigneeIds;
    }

    private async Task<TaskResponse> MapTaskToResponseAsync(KanbanTask task)
    {
        // Мигрируем старые данные если необходимо
        await MigrateLegacyTaskData(task);
        
        // Базовое маппинг
        var response = MapTaskToResponse(task);

        // Заполняем информацию об исполнителях
        if (task.AssigneeIds.Any())
        {
            var assignees = await _userDatabase.FindAsync(u => task.AssigneeIds.Contains(u.Id));
            var assigneesList = assignees.ToList();
            
            // Заполняем Assignees для UI совместимости (имена)
            response.Assignees = assigneesList.Select(u => u.Username).ToList();
            
            // Заполняем AssigneeDetails для подробной информации
            if (response.AssigneeDetails == null)
                response.AssigneeDetails = new List<TaskAssigneeInfo>();
            
            foreach (var assignee in assigneesList)
            {
                response.AssigneeDetails.Add(new TaskAssigneeInfo
                {
                    UserId = assignee.Id,
                    Username = assignee.Username,
                    Email = assignee.Email
                });
            }
        }

        return response;
    }

    /// <summary>
    /// Мигрирует старые данные assignees в AssigneeIds для обратной совместимости
    /// </summary>
    private async Task MigrateLegacyTaskData(KanbanTask task)
    {
        bool needsUpdate = false;
        
        // Проверяем и очищаем некорректные данные в AssigneeIds
        if (task.AssigneeIds.Any())
        {
            var validAssigneeIds = new List<string>();
            foreach (var assigneeId in task.AssigneeIds)
            {
                // Проверяем, что это валидный ObjectId (24 символа hex)
                if (MongoDB.Bson.ObjectId.TryParse(assigneeId, out _))
                {
                    validAssigneeIds.Add(assigneeId);
                }
                else
                {
                    Console.WriteLine($"⚠️ API: Некорректный AssigneeId '{assigneeId}' в задаче {task.Id}, удаляю");
                    needsUpdate = true;
                }
            }
            
            if (needsUpdate)
            {
                task.AssigneeIds = validAssigneeIds;
                // Если у нас есть некорректные ID, пробуем восстановить из Assignees
                if (task.Assignees.Any())
                {
                    Console.WriteLine($"🔄 API: Пытаюсь восстановить исполнителей из имен пользователей для задачи {task.Id}");
                    foreach (var assigneeName in task.Assignees)
                    {
                        var users = await _userDatabase.FindAsync(u => u.Username == assigneeName);
                        var user = users.FirstOrDefault();
                        if (user != null && !task.AssigneeIds.Contains(user.Id))
                        {
                            task.AssigneeIds.Add(user.Id);
                            Console.WriteLine($"✅ API: Восстановлен исполнитель {assigneeName} -> {user.Id}");
                        }
                    }
                }
            }
        }
        
        // Если есть старые данные assignees и нет новых AssigneeIds
        if (task.LegacyAssignees != null && task.LegacyAssignees.Any() && !task.AssigneeIds.Any())
        {
            Console.WriteLine($"🔄 API: Мигрирую данные assignees для задачи {task.Id}");
            
            // Переносим данные из assignees в AssigneeIds
            task.AssigneeIds = new List<string>(task.LegacyAssignees);
            
            // Очищаем старое поле
            task.LegacyAssignees = null;
            needsUpdate = true;
            
            Console.WriteLine($"✅ API: Данные мигрированы для задачи {task.Id}: {task.AssigneeIds.Count} исполнителей");
        }
        
        // Сохраняем в базу данных если были изменения
        if (needsUpdate)
        {
            await _taskDatabase.UpdateAsync(task.Id, task);
            Console.WriteLine($"💾 API: Обновлены данные задачи {task.Id}");
        }
    }
} 