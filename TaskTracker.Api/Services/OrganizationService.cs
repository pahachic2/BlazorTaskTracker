using MongoDB.Driver;
using TaskTracker.Models;
using TaskTracker.Models.DTOs;

namespace TaskTracker.Api.Services;

public class OrganizationService : IOrganizationService
{
    private readonly IDatabaseService<Organization> _organizationDatabase;
    private readonly IDatabaseService<UserOrganization> _userOrganizationDatabase;
    private readonly IDatabaseService<Project> _projectDatabase;
    private readonly IDatabaseService<OrganizationInvitation> _invitationDatabase;
    private readonly IDatabaseService<User> _userDatabase;
    private readonly IEmailService _emailService;

    public OrganizationService(
        IDatabaseService<Organization> organizationDatabase,
        IDatabaseService<UserOrganization> userOrganizationDatabase,
        IDatabaseService<Project> projectDatabase,
        IDatabaseService<OrganizationInvitation> invitationDatabase,
        IDatabaseService<User> userDatabase,
        IEmailService emailService)
    {
        _organizationDatabase = organizationDatabase;
        _userOrganizationDatabase = userOrganizationDatabase;
        _projectDatabase = projectDatabase;
        _invitationDatabase = invitationDatabase;
        _userDatabase = userDatabase;
        _emailService = emailService;
    }

    public async Task<List<OrganizationResponse>> GetUserOrganizationsAsync(string userId)
    {
        // Получаем все связи пользователя с организациями
        var userOrganizations = await _userOrganizationDatabase.FindAsync(
            uo => uo.UserId == userId && uo.IsActive);

        if (!userOrganizations.Any())
            return new List<OrganizationResponse>();

        var organizationIds = userOrganizations.Select(uo => uo.OrganizationId).ToList();
        
        // Получаем сами организации
        var organizations = await _organizationDatabase.FindAsync(
            o => organizationIds.Contains(o.Id));

        var result = new List<OrganizationResponse>();
        foreach (var org in organizations)
        {
            var response = await MapToResponseAsync(org);
            result.Add(response);
        }

        return result;
    }

    public async Task<OrganizationResponse?> GetOrganizationByIdAsync(string organizationId, string userId)
    {
        // Проверяем, что пользователь имеет доступ к организации
        var userOrganizations = await _userOrganizationDatabase.FindAsync(
            uo => uo.OrganizationId == organizationId && uo.UserId == userId && uo.IsActive);
        
        var userOrganization = userOrganizations.FirstOrDefault();
        if (userOrganization == null)
            return null;

        var organization = await _organizationDatabase.GetByIdAsync(organizationId);
        return organization != null ? await MapToResponseAsync(organization) : null;
    }

    public async Task<OrganizationResponse> CreateOrganizationAsync(CreateOrganizationRequest request, string userId)
    {
        var organization = new Organization
        {
            Name = request.Name,
            Description = request.Description ?? string.Empty,
            Icon = request.Icon,
            Color = request.Color,
            OwnerId = userId,
            CreatedDate = DateTime.UtcNow,
            ProjectCount = 0,
            Members = new List<string> { userId }
        };

        await _organizationDatabase.CreateAsync(organization);

        // Создаем связь пользователя с организацией как владельца
        var userOrganization = new UserOrganization
        {
            UserId = userId,
            OrganizationId = organization.Id,
            Role = OrganizationRole.Owner,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _userOrganizationDatabase.CreateAsync(userOrganization);

        return await MapToResponseAsync(organization);
    }

    public async Task<OrganizationResponse?> UpdateOrganizationAsync(string organizationId, UpdateOrganizationRequest request, string userId)
    {
        // Проверяем, что пользователь является владельцем или админом
        var userOrganizations = await _userOrganizationDatabase.FindAsync(
            uo => uo.OrganizationId == organizationId && uo.UserId == userId && uo.IsActive &&
                  (uo.Role == OrganizationRole.Owner || uo.Role == OrganizationRole.Admin));
        
        var userOrganization = userOrganizations.FirstOrDefault();
        if (userOrganization == null)
            return null;

        var organization = await _organizationDatabase.GetByIdAsync(organizationId);
        if (organization == null)
            return null;

        organization.Name = request.Name;
        organization.Description = request.Description ?? string.Empty;
        organization.Icon = request.Icon;
        organization.Color = request.Color;

        await _organizationDatabase.UpdateAsync(organizationId, organization);
        return await MapToResponseAsync(organization);
    }

    public async Task<bool> DeleteOrganizationAsync(string organizationId, string userId)
    {
        // Проверяем, что пользователь является владельцем
        var userOrganizations = await _userOrganizationDatabase.FindAsync(
            uo => uo.OrganizationId == organizationId && uo.UserId == userId && uo.IsActive &&
                  uo.Role == OrganizationRole.Owner);
        
        var userOrganization = userOrganizations.FirstOrDefault();
        if (userOrganization == null)
            return false;

        var organization = await _organizationDatabase.GetByIdAsync(organizationId);
        if (organization == null)
            return false;

        // Удаляем организацию
        await _organizationDatabase.DeleteAsync(organizationId);
        return true;
    }

    public async Task<bool> AddMemberToOrganizationAsync(string organizationId, string userId, string memberUserId)
    {
        // Проверяем, что пользователь является владельцем или админом
        var userOrganizations = await _userOrganizationDatabase.FindAsync(
            uo => uo.OrganizationId == organizationId && uo.UserId == userId && uo.IsActive &&
                  (uo.Role == OrganizationRole.Owner || uo.Role == OrganizationRole.Admin));
        
        var userOrganization = userOrganizations.FirstOrDefault();
        if (userOrganization == null)
            return false;

        // Проверяем, не является ли пользователь уже участником
        var existingMembers = await _userOrganizationDatabase.FindAsync(
            uo => uo.OrganizationId == organizationId && uo.UserId == memberUserId);
        
        var existingMember = existingMembers.FirstOrDefault();
        if (existingMember != null)
        {
            // Обновляем статус существующего участника
            existingMember.IsActive = true;
            await _userOrganizationDatabase.UpdateAsync(existingMember.Id, existingMember);
        }
        else
        {
            // Добавляем нового участника
            var newUserOrganization = new UserOrganization
            {
                UserId = memberUserId,
                OrganizationId = organizationId,
                Role = OrganizationRole.Member,
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _userOrganizationDatabase.CreateAsync(newUserOrganization);
        }

        return true;
    }

    public async Task<bool> RemoveMemberFromOrganizationAsync(string organizationId, string userId, string memberUserId)
    {
        // Проверяем, что пользователь является владельцем или админом
        var userOrganizations = await _userOrganizationDatabase.FindAsync(
            uo => uo.OrganizationId == organizationId && uo.UserId == userId && uo.IsActive &&
                  (uo.Role == OrganizationRole.Owner || uo.Role == OrganizationRole.Admin));
        
        var userOrganization = userOrganizations.FirstOrDefault();
        if (userOrganization == null)
            return false;

        var memberOrganizations = await _userOrganizationDatabase.FindAsync(
            uo => uo.OrganizationId == organizationId && uo.UserId == memberUserId && uo.IsActive);
        
        var memberOrganization = memberOrganizations.FirstOrDefault();
        if (memberOrganization == null)
            return false;

        // Владельца нельзя удалить
        if (memberOrganization.Role == OrganizationRole.Owner)
            return false;

        memberOrganization.IsActive = false;
        await _userOrganizationDatabase.UpdateAsync(memberOrganization.Id, memberOrganization);

        return true;
    }

    public async Task<List<OrganizationResponse>> GetAllOrganizationsAsync()
    {
        var organizations = await _organizationDatabase.GetAllAsync();
        var result = new List<OrganizationResponse>();
        
        foreach (var org in organizations)
        {
            var response = await MapToResponseAsync(org);
            result.Add(response);
        }

        return result;
    }

    private async Task<OrganizationResponse> MapToResponseAsync(Organization organization)
    {
        // Подсчитываем количество проектов в организации
        var projects = await _projectDatabase.FindAsync(p => p.OrganizationId == organization.Id && p.IsActive);
        
        return new OrganizationResponse
        {
            Id = organization.Id,
            Name = organization.Name,
            Description = organization.Description,
            Icon = organization.Icon,
            Color = organization.Color,
            Members = organization.Members,
            OwnerId = organization.OwnerId,
            ProjectCount = projects.Count(),
            CreatedDate = organization.CreatedDate
        };
    }

    // Новые методы для системы приглашений

    public async Task<InvitationResponse> InviteUserAsync(string organizationId, InviteUserRequest request, string userId)
    {
        Console.WriteLine($"📧 ORG: Приглашение пользователя {request.Email} в организацию {organizationId}");

        // Проверяем, что пользователь является владельцем или админом
        var hasPermission = await HasOrganizationPermissionAsync(organizationId, userId, OrganizationRole.Admin);
        if (!hasPermission)
        {
            Console.WriteLine($"❌ ORG: Пользователь {userId} не имеет прав для приглашений в организацию {organizationId}");
            throw new UnauthorizedAccessException("Нет прав для отправки приглашений");
        }

        var organization = await _organizationDatabase.GetByIdAsync(organizationId);
        if (organization == null)
        {
            Console.WriteLine($"❌ ORG: Организация {organizationId} не найдена");
            throw new ArgumentException("Организация не найдена");
        }

        // Проверяем, не является ли пользователь уже участником
        var existingUser = await _userDatabase.FindAsync(u => u.Email == request.Email && u.IsActive);
        var user = existingUser.FirstOrDefault();
        
        if (user != null)
        {
            var existingMembership = await _userOrganizationDatabase.FindAsync(
                uo => uo.OrganizationId == organizationId && uo.UserId == user.Id && uo.IsActive);
            
            if (existingMembership.Any())
            {
                Console.WriteLine($"❌ ORG: Пользователь {request.Email} уже является участником организации");
                throw new InvalidOperationException("Пользователь уже является участником организации");
            }
        }

        // Проверяем, нет ли активного приглашения
        var existingInvitations = await _invitationDatabase.FindAsync(
            i => i.OrganizationId == organizationId && 
                 i.Email == request.Email && 
                 i.Status == InvitationStatus.Pending &&
                 i.ExpiresAt > DateTime.UtcNow);

        if (existingInvitations.Any())
        {
            Console.WriteLine($"❌ ORG: Активное приглашение для {request.Email} уже существует");
            throw new InvalidOperationException("Активное приглашение для этого email уже существует");
        }

        // Создаем приглашение
        var invitation = new OrganizationInvitation
        {
            OrganizationId = organizationId,
            Email = request.Email,
            InvitedBy = userId,
            Role = request.Role,
            Token = Guid.NewGuid().ToString("N"), // Безопасный токен
            Status = InvitationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            UserWasRegistered = user != null // Запоминаем, был ли пользователь зарегистрирован
        };

        await _invitationDatabase.CreateAsync(invitation);
        Console.WriteLine($"✅ ORG: Приглашение создано с токеном {invitation.Token}");

        // Получаем имя приглашающего
        var invitedByUser = await _userDatabase.GetByIdAsync(userId);
        var invitedByName = invitedByUser?.Username ?? "Администратор";

        // Email отправляем ТОЛЬКО если пользователь НЕ зарегистрирован
        bool emailSent = false;
        
        if (user == null)
        {
            // Пользователь НЕ зарегистрирован - отправляем email для регистрации и принятия
            Console.WriteLine($"📧 ORG: Пользователь {request.Email} не зарегистрирован, отправляем email");
            emailSent = await _emailService.SendInvitationEmailAsync(
                request.Email, 
                organization.Name, 
                invitedByName, 
                invitation.Token, 
                request.Role);

            if (!emailSent)
            {
                Console.WriteLine($"⚠️ ORG: Ошибка отправки email для незарегистрированного пользователя");
            }
        }
        else
        {
            // Пользователь УЖЕ зарегистрирован - НЕ отправляем email, покажем в интерфейсе
            Console.WriteLine($"👤 ORG: Пользователь {request.Email} уже зарегистрирован, приглашение будет показано в интерфейсе");
        }

        // Обновляем приглашение с информацией об отправке email
        invitation.EmailSent = emailSent;
        await _invitationDatabase.UpdateAsync(invitation.Id, invitation);

        return await MapInvitationToResponseAsync(invitation);
    }

    public async Task<List<InvitationResponse>> GetOrganizationInvitationsAsync(string organizationId, string userId)
    {
        Console.WriteLine($"📋 ORG: Получение приглашений для организации {organizationId}");

        // Проверяем доступ
        var hasPermission = await HasOrganizationPermissionAsync(organizationId, userId, OrganizationRole.Member);
        if (!hasPermission)
        {
            Console.WriteLine($"❌ ORG: Нет доступа к приглашениям организации {organizationId}");
            return new List<InvitationResponse>();
        }

        var invitations = await _invitationDatabase.FindAsync(i => i.OrganizationId == organizationId);
        var responses = new List<InvitationResponse>();

        foreach (var invitation in invitations.OrderByDescending(i => i.CreatedAt))
        {
            var response = await MapInvitationToResponseAsync(invitation);
            responses.Add(response);
        }

        Console.WriteLine($"✅ ORG: Найдено {responses.Count} приглашений");
        return responses;
    }

    public async Task<List<OrganizationMemberResponse>> GetOrganizationMembersAsync(string organizationId, string userId)
    {
        Console.WriteLine($"👥 ORG: Получение участников организации {organizationId}");

        // Проверяем доступ
        var hasPermission = await HasOrganizationPermissionAsync(organizationId, userId, OrganizationRole.Member);
        if (!hasPermission)
        {
            Console.WriteLine($"❌ ORG: Нет доступа к участникам организации {organizationId}");
            return new List<OrganizationMemberResponse>();
        }

        var userOrganizations = await _userOrganizationDatabase.FindAsync(
            uo => uo.OrganizationId == organizationId && uo.IsActive);

        var members = new List<OrganizationMemberResponse>();

        foreach (var userOrg in userOrganizations.OrderBy(uo => uo.JoinedAt))
        {
            var user = await _userDatabase.GetByIdAsync(userOrg.UserId);
            if (user != null)
            {
                members.Add(new OrganizationMemberResponse
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = userOrg.Role,
                    JoinedAt = userOrg.JoinedAt,
                    IsActive = userOrg.IsActive
                });
            }
        }

        Console.WriteLine($"✅ ORG: Найдено {members.Count} участников");
        return members;
    }

    public async Task<bool> RevokeInvitationAsync(string invitationId, string userId)
    {
        Console.WriteLine($"🚫 ORG: Отзыв приглашения {invitationId}");

        var invitation = await _invitationDatabase.GetByIdAsync(invitationId);
        if (invitation == null)
        {
            Console.WriteLine($"❌ ORG: Приглашение {invitationId} не найдено");
            return false;
        }

        // Проверяем права на отзыв
        var hasPermission = await HasOrganizationPermissionAsync(invitation.OrganizationId, userId, OrganizationRole.Admin);
        if (!hasPermission)
        {
            Console.WriteLine($"❌ ORG: Нет прав для отзыва приглашения");
            return false;
        }

        // Получаем информацию для уведомления перед удалением
        var organization = await _organizationDatabase.GetByIdAsync(invitation.OrganizationId);
        var revokedByUser = await _userDatabase.GetByIdAsync(userId);
        
        // Полностью удаляем приглашение из БД
        await _invitationDatabase.DeleteAsync(invitationId);

        // Отправляем уведомление об отзыве (только если пользователь НЕ зарегистрирован)
        if (organization != null && revokedByUser != null && !invitation.UserWasRegistered)
        {
            await _emailService.SendInvitationRevokedEmailAsync(
                invitation.Email, 
                organization.Name, 
                revokedByUser.Username);
        }

        Console.WriteLine($"✅ ORG: Приглашение {invitationId} удалено");
        return true;
    }

    public async Task<AcceptInvitationResponse> AcceptInvitationAsync(AcceptInvitationRequest request)
    {
        Console.WriteLine($"✅ ORG: Принятие приглашения с токеном {request.Token}");

        // Находим приглашение по токену
        var invitations = await _invitationDatabase.FindAsync(i => i.Token == request.Token);
        var invitation = invitations.FirstOrDefault();

        if (invitation == null)
        {
            Console.WriteLine($"❌ ORG: Приглашение с токеном {request.Token} не найдено");
            return new AcceptInvitationResponse 
            { 
                Success = false, 
                Message = "Приглашение не найдено" 
            };
        }

        // Проверяем статус и срок действия
        if (invitation.Status != InvitationStatus.Pending)
        {
            Console.WriteLine($"❌ ORG: Приглашение имеет неверный статус: {invitation.Status}");
            return new AcceptInvitationResponse 
            { 
                Success = false, 
                Message = "Приглашение уже использовано или отозвано" 
            };
        }

        if (invitation.ExpiresAt < DateTime.UtcNow)
        {
            Console.WriteLine($"❌ ORG: Приглашение истекло");
            invitation.Status = InvitationStatus.Expired;
            await _invitationDatabase.UpdateAsync(invitation.Id, invitation);
            return new AcceptInvitationResponse 
            { 
                Success = false, 
                Message = "Срок действия приглашения истек" 
            };
        }

        var organization = await _organizationDatabase.GetByIdAsync(invitation.OrganizationId);
        if (organization == null)
        {
            Console.WriteLine($"❌ ORG: Организация не найдена");
            return new AcceptInvitationResponse 
            { 
                Success = false, 
                Message = "Организация не найдена" 
            };
        }

        // Проверяем, существует ли пользователь
        User? user = null;
        
        // Если передан UserId (авторизованный пользователь), используем его
        if (!string.IsNullOrEmpty(request.UserId))
        {
            Console.WriteLine($"✅ ORG: Обрабатываем приглашение для авторизованного пользователя {request.UserId}");
            user = await _userDatabase.GetByIdAsync(request.UserId);
            
            if (user == null)
            {
                return new AcceptInvitationResponse 
                { 
                    Success = false, 
                    Message = "Пользователь не найден" 
                };
            }
            
            // Проверяем, что email приглашения совпадает с email пользователя
            if (user.Email != invitation.Email)
            {
                return new AcceptInvitationResponse 
                { 
                    Success = false, 
                    Message = "Приглашение предназначено для другого email адреса" 
                };
            }
        }
        else
        {
            // Пользователь не авторизован - ищем по email или создаем нового
            var existingUsers = await _userDatabase.FindAsync(u => u.Email == invitation.Email && u.IsActive);
            user = existingUsers.FirstOrDefault();
        }
        
        string? jwtToken = null;

        if (user == null)
        {
            // Пользователь не существует - нужна регистрация
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return new AcceptInvitationResponse 
                { 
                    Success = false, 
                    Message = "Для принятия приглашения необходимо указать имя пользователя и пароль" 
                };
            }

            // Создаем нового пользователя (здесь нужно подключить UserService для хеширования пароля)
            user = new User
            {
                Username = request.Username,
                Email = invitation.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password), // Временно прямое хеширование
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _userDatabase.CreateAsync(user);
            Console.WriteLine($"✅ ORG: Создан новый пользователь {user.Username}");
        }

        // Добавляем пользователя в организацию
        var userOrganization = new UserOrganization
        {
            UserId = user.Id,
            OrganizationId = invitation.OrganizationId,
            Role = invitation.Role,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _userOrganizationDatabase.CreateAsync(userOrganization);

        // Обновляем приглашение
        invitation.Status = InvitationStatus.Accepted;
        invitation.AcceptedAt = DateTime.UtcNow;
        invitation.AcceptedBy = user.Id;
        await _invitationDatabase.UpdateAsync(invitation.Id, invitation);

        // Отправляем приветственное письмо
        await _emailService.SendWelcomeEmailAsync(user.Email, organization.Name);

        Console.WriteLine($"✅ ORG: Пользователь {user.Username} успешно присоединился к организации {organization.Name}");

        return new AcceptInvitationResponse
        {
            Success = true,
            Message = "Вы успешно присоединились к организации",
            UserId = user.Id,
            Token = jwtToken,
            Organization = await MapToResponseAsync(organization)
        };
    }

    public async Task<InvitationInfoResponse> GetInvitationInfoAsync(string token)
    {
        Console.WriteLine($"📋 ORG: Получение информации о приглашении {token}");

        var invitations = await _invitationDatabase.FindAsync(i => i.Token == token);
        var invitation = invitations.FirstOrDefault();

        if (invitation == null)
        {
            return new InvitationInfoResponse
            {
                IsValid = false,
                Message = "Приглашение не найдено"
            };
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            return new InvitationInfoResponse
            {
                IsValid = false,
                Message = "Приглашение уже использовано или отозвано"
            };
        }

        if (invitation.ExpiresAt < DateTime.UtcNow)
        {
            return new InvitationInfoResponse
            {
                IsValid = false,
                Message = "Срок действия приглашения истек"
            };
        }

        var organization = await _organizationDatabase.GetByIdAsync(invitation.OrganizationId);
        var invitedByUser = await _userDatabase.GetByIdAsync(invitation.InvitedBy);
        
        // Проверяем, нужна ли регистрация
        var existingUsers = await _userDatabase.FindAsync(u => u.Email == invitation.Email && u.IsActive);
        var requiresRegistration = !existingUsers.Any();

        return new InvitationInfoResponse
        {
            IsValid = true,
            Message = "Приглашение действительно",
            OrganizationName = organization?.Name,
            InvitedByName = invitedByUser?.Username,
            Role = invitation.Role,
            ExpiresAt = invitation.ExpiresAt,
            RequiresRegistration = requiresRegistration
        };
    }

    public async Task<bool> DeclineInvitationAsync(DeclineInvitationRequest request)
    {
        Console.WriteLine($"❌ ORG: Отклонение приглашения {request.Token}");

        var invitations = await _invitationDatabase.FindAsync(i => i.Token == request.Token);
        var invitation = invitations.FirstOrDefault();

        if (invitation == null || invitation.Status != InvitationStatus.Pending)
        {
            Console.WriteLine($"❌ ORG: Приглашение не найдено или уже обработано");
            return false;
        }

        invitation.Status = InvitationStatus.Declined;
        await _invitationDatabase.UpdateAsync(invitation.Id, invitation);

        Console.WriteLine($"✅ ORG: Приглашение {request.Token} отклонено");
        return true;
    }

    // Вспомогательные методы

    public async Task<bool> HasOrganizationPermissionAsync(string organizationId, string userId, OrganizationRole minimumRole)
    {
        var userOrganizations = await _userOrganizationDatabase.FindAsync(
            uo => uo.OrganizationId == organizationId && uo.UserId == userId && uo.IsActive);
        
        var userOrganization = userOrganizations.FirstOrDefault();
        if (userOrganization == null)
            return false;

        // Owner > Admin > Member
        return minimumRole switch
        {
            OrganizationRole.Member => true,
            OrganizationRole.Admin => userOrganization.Role == OrganizationRole.Owner || userOrganization.Role == OrganizationRole.Admin,
            OrganizationRole.Owner => userOrganization.Role == OrganizationRole.Owner,
            _ => false
        };
    }

    // НОВЫЙ МЕТОД: Получить приглашения пользователя
    public async Task<List<InvitationResponse>> GetUserInvitationsAsync(string userId)
    {
        Console.WriteLine($"📋 ORG: Получение приглашений для пользователя {userId}");

        // Получаем пользователя чтобы узнать его email
        var user = await _userDatabase.GetByIdAsync(userId);
        if (user == null)
        {
            Console.WriteLine($"❌ ORG: Пользователь {userId} не найден");
            return new List<InvitationResponse>();
        }

        // Ищем все активные приглашения для этого email
        var invitations = await _invitationDatabase.FindAsync(
            i => i.Email == user.Email && 
                 i.Status == InvitationStatus.Pending &&
                 i.ExpiresAt > DateTime.UtcNow);

        var responses = new List<InvitationResponse>();

        foreach (var invitation in invitations.OrderByDescending(i => i.CreatedAt))
        {
            var response = await MapInvitationToResponseAsync(invitation);
            responses.Add(response);
        }

        Console.WriteLine($"✅ ORG: Найдено {responses.Count} активных приглашений для пользователя {user.Email}");
        return responses;
    }

    private async Task<InvitationResponse> MapInvitationToResponseAsync(OrganizationInvitation invitation)
    {
        var organization = await _organizationDatabase.GetByIdAsync(invitation.OrganizationId);
        var invitedByUser = await _userDatabase.GetByIdAsync(invitation.InvitedBy);

        return new InvitationResponse
        {
            Id = invitation.Id,
            OrganizationId = invitation.OrganizationId,
            OrganizationName = organization?.Name ?? "Неизвестная организация",
            Email = invitation.Email,
            InvitedBy = invitation.InvitedBy,
            InvitedByName = invitedByUser?.Username ?? "Неизвестный пользователь",
            Role = invitation.Role,
            Status = invitation.Status,
            CreatedAt = invitation.CreatedAt,
            ExpiresAt = invitation.ExpiresAt,
            AcceptedAt = invitation.AcceptedAt,
            Token = invitation.Token,
            EmailSent = invitation.EmailSent,
            UserWasRegistered = invitation.UserWasRegistered
        };
    }
} 