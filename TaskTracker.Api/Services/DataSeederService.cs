using TaskTracker.Models;

namespace TaskTracker.Api.Services;

public class DataSeederService
{
    private readonly IDatabaseService<User> _userDatabase;
    private readonly IDatabaseService<Project> _projectDatabase;
    private readonly IDatabaseService<UserProject> _userProjectDatabase;
    private readonly IDatabaseService<KanbanColumn> _columnDatabase;
    private readonly IDatabaseService<KanbanTask> _taskDatabase;
    private readonly IUserService _userService;

    public DataSeederService(
        IDatabaseService<User> userDatabase,
        IDatabaseService<Project> projectDatabase,
        IDatabaseService<UserProject> userProjectDatabase,
        IDatabaseService<KanbanColumn> columnDatabase,
        IDatabaseService<KanbanTask> taskDatabase,
        IUserService userService)
    {
        _userDatabase = userDatabase;
        _projectDatabase = projectDatabase;
        _userProjectDatabase = userProjectDatabase;
        _columnDatabase = columnDatabase;
        _taskDatabase = taskDatabase;
        _userService = userService;
    }

    public async Task SeedDataAsync()
    {
        Console.WriteLine("🌱 Начинаем заполнение базы данных тестовыми данными...");

        // Проверяем, есть ли уже данные
        var existingProjects = await _projectDatabase.GetAllAsync();
        if (existingProjects.Any())
        {
            Console.WriteLine("📋 База данных уже содержит проекты. Пропускаем заполнение.");
            return;
        }

        // Создаем тестовых пользователей
        var users = await CreateTestUsersAsync();
        Console.WriteLine($"👥 Создано {users.Count} тестовых пользователей");

        // Создаем тестовые проекты
        var projects = await CreateTestProjectsAsync(users);
        Console.WriteLine($"📂 Создано {projects.Count} тестовых проектов");

        // Создаем колонки для проектов
        var columns = await CreateTestColumnsAsync(projects);
        Console.WriteLine($"📋 Создано {columns.Count} колонок");

        // Создаем тестовые задачи
        var tasks = await CreateTestTasksAsync(projects, columns, users);
        Console.WriteLine($"✅ Создано {tasks.Count} тестовых задач");

        Console.WriteLine("🎉 Заполнение базы данных завершено!");
    }

    private async Task<List<User>> CreateTestUsersAsync()
    {
        var users = new List<User>();

        var testUsers = new[]
        {
            new { Username = "anna.ivanova", Email = "anna@example.com", Password = "password123" },
            new { Username = "petr.sidorov", Email = "petr@example.com", Password = "password123" },
            new { Username = "maria.petrova", Email = "maria@example.com", Password = "password123" },
            new { Username = "ivan.kozlov", Email = "ivan@example.com", Password = "password123" }
        };

        foreach (var userData in testUsers)
        {
            // Проверяем, не существует ли уже пользователь
            var existingUsers = await _userDatabase.FindAsync(u => u.Email == userData.Email);
            if (existingUsers.Any())
            {
                users.Add(existingUsers.First());
                continue;
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userData.Password);
            var user = new User
            {
                Username = userData.Username,
                Email = userData.Email,
                PasswordHash = hashedPassword,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _userDatabase.CreateAsync(user);
            users.Add(user);
        }

        return users;
    }

    private async Task<List<Project>> CreateTestProjectsAsync(List<User> users)
    {
        var projects = new List<Project>();
        var owner = users.First(); // Анна Иванова как владелец всех проектов

        var projectsData = new[]
        {
            new {
                Name = "TaskTracker Pro",
                Description = "Система управления задачами и проектами",
                Icon = "🚀",
                Color = "bg-blue-500",
                Members = new string[] { "Анна Иванова", "Петр Сидоров", "Мария Петрова", "Иван Козлов" }
            },
            new {
                Name = "Мобильное приложение",
                Description = "Разработка iOS и Android приложения",
                Icon = "📱",
                Color = "bg-green-500",
                Members = new string[] { "Анна Иванова", "Петр Сидоров" }
            },
            new {
                Name = "Дизайн система",
                Description = "UI/UX компоненты и гайдлайны",
                Icon = "🎨",
                Color = "bg-purple-500",
                Members = new string[] { "Анна Иванова", "Мария Петрова" }
            }
        };

        foreach (var projectData in projectsData)
        {
            var project = new Project
            {
                Name = projectData.Name,
                Description = projectData.Description,
                Icon = projectData.Icon,
                Color = projectData.Color,
                OwnerId = owner.Id,
                CreatedDate = DateTime.UtcNow.AddDays(-30 + projects.Count * 10),
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                TaskCount = 0,
                Members = projectData.Members.ToList()
            };

            await _projectDatabase.CreateAsync(project);
            projects.Add(project);

            // Создаем связи пользователей с проектом
            foreach (var memberName in projectData.Members)
            {
                var user = users.FirstOrDefault(u => u.Username.Contains(memberName.Split(' ')[0].ToLower()));
                if (user != null)
                {
                    var userProject = new UserProject
                    {
                        UserId = user.Id,
                        ProjectId = project.Id,
                        Role = user.Id == owner.Id ? ProjectRole.Owner : ProjectRole.Member,
                        JoinedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    await _userProjectDatabase.CreateAsync(userProject);
                }
            }
        }

        return projects;
    }

    private async Task<List<KanbanColumn>> CreateTestColumnsAsync(List<Project> projects)
    {
        var columns = new List<KanbanColumn>();

        var columnNames = new[] { "К выполнению", "В процессе", "На проверке", "Выполнено" };

        foreach (var project in projects)
        {
            for (int i = 0; i < columnNames.Length; i++)
            {
                var column = new KanbanColumn
                {
                    Title = columnNames[i],
                    ProjectId = project.Id,
                    Order = i + 1, // Начинаем с 1
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _columnDatabase.CreateAsync(column);
                columns.Add(column);
            }
        }

        return columns;
    }

    private async Task<List<KanbanTask>> CreateTestTasksAsync(
        List<Project> projects, 
        List<KanbanColumn> columns, 
        List<User> users)
    {
        var tasks = new List<KanbanTask>();
        var firstProject = projects.First();
        var projectColumns = columns.Where(c => c.ProjectId == firstProject.Id).ToList();

        // Задачи для первого проекта (TaskTracker Pro)
        var tasksData = new List<dynamic>
        {
            new {
                Title = "Создать дизайн главной страницы",
                Description = "Разработать макет главной страницы в Figma с учетом современных UX принципов",
                Tags = new string[] { "Design", "Frontend", "Urgent" },
                Assignees = new string[] { "Анна Иванова" },
                DueDate = DateTime.UtcNow.AddDays(3),
                ColumnIndex = 0, // К выполнению
                Priority = TaskPriority.High
            },
            new {
                Title = "Настроить CI/CD pipeline",
                Description = "Автоматизация деплоя и тестирования",
                Tags = new string[] { "Backend", "DevOps" },
                Assignees = new string[] { "Петр Сидоров" },
                DueDate = DateTime.UtcNow.AddDays(5),
                ColumnIndex = 0, // К выполнению
                Priority = TaskPriority.Medium
            },
            new {
                Title = "Реализовать авторизацию",
                Description = "JWT токены, refresh tokens, роли пользователей",
                Tags = new string[] { "Backend", "Feature" },
                Assignees = new string[] { "Иван Козлов", "Петр Сидоров" },
                DueDate = DateTime.UtcNow.AddDays(2),
                ColumnIndex = 1, // В процессе
                Priority = TaskPriority.High
            },
            new {
                Title = "Создать базу данных",
                Description = "Настройка PostgreSQL, миграции",
                Tags = new string[] { "Backend", "Database" },
                Assignees = new string[] { "Петр Сидоров" },
                DueDate = (DateTime?)null,
                ColumnIndex = 3, // Выполнено
                Priority = TaskPriority.Medium
            },
            new {
                Title = "Провести тестирование API",
                Description = "Unit тесты, интеграционные тесты",
                Tags = new string[] { "Testing", "QA" },
                Assignees = new string[] { "Мария Петрова" },
                DueDate = DateTime.UtcNow.AddDays(7),
                ColumnIndex = 2, // На проверке
                Priority = TaskPriority.Medium
            }
        };

        for (int i = 0; i < tasksData.Count; i++)
        {
            var taskData = tasksData[i];
            var targetColumn = projectColumns[taskData.ColumnIndex];
            var creator = users.First(); // Анна Иванова

            var task = new KanbanTask
            {
                Title = taskData.Title,
                Description = taskData.Description,
                Tags = ((string[])taskData.Tags).ToList(),
                Assignees = ((string[])taskData.Assignees).ToList(),
                DueDate = taskData.DueDate,
                ColumnId = targetColumn.Id,
                ProjectId = firstProject.Id,
                CreatedBy = creator.Id,
                Order = i,
                Priority = taskData.Priority,
                Status = taskData.ColumnIndex switch
                {
                    0 => Models.TaskStatus.ToDo,
                    1 => Models.TaskStatus.InProgress,
                    2 => Models.TaskStatus.Review,
                    3 => Models.TaskStatus.Done,
                    _ => Models.TaskStatus.ToDo
                },
                CreatedAt = DateTime.UtcNow.AddDays(-i),
                UpdatedAt = DateTime.UtcNow
            };

            await _taskDatabase.CreateAsync(task);
            tasks.Add(task);
        }

        // Обновляем счетчик задач в проекте
        firstProject.TaskCount = tasks.Count;
        await _projectDatabase.UpdateAsync(firstProject.Id, firstProject);

        return tasks;
    }
} 