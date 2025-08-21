using System.Text.Json;
using AuthService.Domain;
using AuthService.Domain.ValueObjects;
using AuthService.Infrastructure.Postgres.IdentityManagers;
using AuthService.Infrastructure.Postgres.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthService.Infrastructure.Postgres.Seeding;

public sealed class AccountsSeederService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly AccountsManager _accountsManager;
    private readonly PermissionManager _permissionManager;
    private readonly RolePermissionManager _rolePermissionManager;
    private readonly AdminOptions _adminOptions;
    private readonly ILogger<AccountsSeederService> _logger;

    public AccountsSeederService(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        AccountsManager accountsManager,
        PermissionManager permissionManager,
        RolePermissionManager rolePermissionManager,
        IOptions<AdminOptions> adminOptions,
        ILogger<AccountsSeederService> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _accountsManager = accountsManager ?? throw new ArgumentNullException(nameof(accountsManager));
        _permissionManager = permissionManager ?? throw new ArgumentNullException(nameof(permissionManager));
        _rolePermissionManager = rolePermissionManager ?? throw new ArgumentNullException(nameof(rolePermissionManager));
        ArgumentNullException.ThrowIfNull(adminOptions);
        _adminOptions = adminOptions.Value ?? throw new ArgumentNullException(nameof(adminOptions.Value));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Инициализация учётных записей...");

        // Используем etc/accounts.json
        RolePermissionOptions seedData = await LoadSeedDataAsync("etc/accounts.json", cancellationToken);

        await SeedPermissions(seedData, cancellationToken);
        await SeedRoles(seedData, cancellationToken);
        await SeedRolePermissions(seedData, cancellationToken);
        await EnsureAdminAsync(cancellationToken);

        _logger.LogInformation("Инициализация учётных записей завершена.");
    }

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static async Task<RolePermissionOptions> LoadSeedDataAsync(string relativePath, CancellationToken ct)
    {
        var candidatePaths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar)),
            Path.Combine(Directory.GetCurrentDirectory(), relativePath.Replace('/', Path.DirectorySeparatorChar))
        };

        var path = candidatePaths.FirstOrDefault(File.Exists);
        if (path is null)
            throw new ApplicationException($"Файл сидов не найден: {relativePath} (пробовали: {string.Join(" | ", candidatePaths)})");

        string json = await File.ReadAllTextAsync(path, ct);
        RolePermissionOptions? seed =
            JsonSerializer.Deserialize<RolePermissionOptions>(json, s_jsonOptions);

        if (seed is null)
            throw new ApplicationException("Не удалось десериализовать конфигурацию ролей и прав.");

        return seed;
    }

    private async Task SeedPermissions(RolePermissionOptions seedData, CancellationToken ct)
    {
        IEnumerable<string> permissionsToAdd = seedData.Permissions
            .SelectMany(kv => kv.Value)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase);

        await _permissionManager.AddRangeIfNotExists(permissionsToAdd, ct);
        _logger.LogInformation("Права добавлены в базу данных.");
    }

    private async Task SeedRoles(RolePermissionOptions seedData, CancellationToken ct)
    {
        IEnumerable<string> roleNames = seedData.Roles.Keys
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (string roleName in roleNames)
        {
            Role? role = await _roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                IdentityResult created = await _roleManager.CreateAsync(new Role { Name = roleName });
                if (!created.Succeeded)
                {
                    string errors = string.Join("; ", created.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    throw new ApplicationException($"Не удалось создать роль '{roleName}': {errors}");
                }
            }
        }

        _logger.LogInformation("Роли добавлены в базу данных.");
    }

    private async Task SeedRolePermissions(RolePermissionOptions seedData, CancellationToken ct)
    {
        foreach (string roleName in seedData.Roles.Keys)
        {
            Role? role = await _roleManager.FindByNameAsync(roleName)
                         ?? throw new ApplicationException($"Роль '{roleName}' не найдена при назначении прав.");

            IEnumerable<string> rolePermissions = seedData.Roles[roleName]
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase);

            await _rolePermissionManager.AddRangeIfExist(role.Id, rolePermissions, ct);
        }

        _logger.LogInformation("Права для ролей добавлены в базу данных.");
    }

    private async Task EnsureAdminAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_adminOptions.UserName) ||
            string.IsNullOrWhiteSpace(_adminOptions.Email) ||
            string.IsNullOrWhiteSpace(_adminOptions.Password))
        {
            throw new ApplicationException("Параметры администратора не настроены (UserName/Email/Password).");
        }

        Role? adminRole = await _roleManager.FindByNameAsync(AdminAccount.ADMIN);
        if (adminRole is null)
        {
            IdentityResult createRole = await _roleManager.CreateAsync(new Role { Name = AdminAccount.ADMIN });
            if (!createRole.Succeeded)
            {
                string errors = string.Join("; ", createRole.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new ApplicationException($"Не удалось создать роль администратора: {errors}");
            }
            adminRole = await _roleManager.FindByNameAsync(AdminAccount.ADMIN)
                        ?? throw new ApplicationException("Не удалось найти роль администратора после создания.");
        }

        User? existingByEmail = await _userManager.FindByEmailAsync(_adminOptions.Email);
        if (existingByEmail is not null)
        {
            bool inRole = await _userManager.IsInRoleAsync(existingByEmail, AdminAccount.ADMIN);
            if (!inRole)
            {
                IdentityResult addToRoleExisting = await _userManager.AddToRoleAsync(existingByEmail, AdminAccount.ADMIN);
                if (!addToRoleExisting.Succeeded)
                {
                    string errors = string.Join("; ", addToRoleExisting.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    throw new ApplicationException($"Не удалось добавить существующего администратора в роль: {errors}");
                }
            }
            _logger.LogInformation("Администратор уже существует. Создание пропущено.");
            return;
        }

        User adminUser = User.CreateAdmin(_adminOptions.UserName, _adminOptions.Email, adminRole);
        IdentityResult createUser = await _userManager.CreateAsync(adminUser, _adminOptions.Password);
        if (!createUser.Succeeded)
        {
            string errors = string.Join("; ", createUser.Errors.Select(e => $"{e.Code}: {e.Description}"));
            throw new ApplicationException($"Не удалось создать пользователя-администратора: {errors}");
        }

        if (!await _userManager.IsInRoleAsync(adminUser, AdminAccount.ADMIN))
        {
            IdentityResult addRole = await _userManager.AddToRoleAsync(adminUser, AdminAccount.ADMIN);
            if (!addRole.Succeeded)
            {
                string errors = string.Join("; ", addRole.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new ApplicationException($"Не удалось назначить роль администратора: {errors}");
            }
        }

        var fullNameResult = FullName.Create(_adminOptions.UserName, _adminOptions.UserName);
        if (fullNameResult.IsFailure)
        {
            throw new ApplicationException($"Некорректное ФИО администратора: {fullNameResult.Error.Message}");
        }

        AdminAccount adminAccount = new(fullNameResult.Value, adminUser);
        var adminAccountCreated = await _accountsManager.CreateAdminAccount(adminAccount, ct);
        if (adminAccountCreated.IsFailure)
        {
            throw new ApplicationException($"Не удалось создать профиль администратора: {adminAccountCreated.Error.Message}");
        }

        _logger.LogInformation("Администратор создан и привязан к профилю AdminAccount.");
    }
}