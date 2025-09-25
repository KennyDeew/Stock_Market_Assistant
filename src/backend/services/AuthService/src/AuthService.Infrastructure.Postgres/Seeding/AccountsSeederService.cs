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
    private readonly IOptionsMonitor<AdminOptions> _adminOptions;
    private readonly IOptions<RolePermissionOptions> _seedOptions;
    private readonly IOptions<IdentityOptions> _identityOptions;
    private readonly ILogger<AccountsSeederService> _logger;

    public AccountsSeederService(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        AccountsManager accountsManager,
        PermissionManager permissionManager,
        RolePermissionManager rolePermissionManager,
        IOptionsMonitor<AdminOptions> adminOptions,
        IOptions<RolePermissionOptions> seedOptions,
        IOptions<IdentityOptions> identityOptions,
        ILogger<AccountsSeederService> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _accountsManager = accountsManager ?? throw new ArgumentNullException(nameof(accountsManager));
        _permissionManager = permissionManager ?? throw new ArgumentNullException(nameof(permissionManager));
        _rolePermissionManager = rolePermissionManager ?? throw new ArgumentNullException(nameof(rolePermissionManager));
        _adminOptions = adminOptions ?? throw new ArgumentNullException(nameof(adminOptions));
        _seedOptions = seedOptions ?? throw new ArgumentNullException(nameof(seedOptions));
        _identityOptions = identityOptions ?? throw new ArgumentNullException(nameof(identityOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Инициализация учётных записей...");

        var addedPerms = await SeedPermissions(_seedOptions.Value, ct);
        var (createdRoles, totalRoles) = await SeedRoles(_seedOptions.Value, ct);
        var addedLinks = await SeedRolePermissions(_seedOptions.Value, ct);
        await EnsureAdminAsync(ct);

        _logger.LogInformation(
            "Инициализация завершена: +{Perms} прав, ролей {Created}/{Total}, связок роль-право +{Links}.",
            addedPerms, createdRoles, totalRoles, addedLinks);
    }

    private async Task<int> SeedPermissions(RolePermissionOptions cfg, CancellationToken ct)
    {
        var permissionsToAdd = (cfg.Permissions ?? [])
            .SelectMany(kv => kv.Value ?? Array.Empty<string>())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (permissionsToAdd.Length == 0)
        {
            _logger.LogInformation("В конфиге сидов нет прав для добавления.");
            return 0;
        }

        await _permissionManager.AddRangeIfNotExists(permissionsToAdd, ct);
        _logger.LogInformation("Права добавлены в БД (уникальных в конфиге: {Count}).", permissionsToAdd.Length);
        return permissionsToAdd.Length;
    }

    private async Task<(int created, int total)> SeedRoles(RolePermissionOptions cfg, CancellationToken ct)
    {
        var roleNames = (cfg.Roles ?? [])
            .Keys
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (roleNames.Length == 0)
        {
            _logger.LogInformation("В конфиге сидов нет ролей для добавления.");
            return (0, 0);
        }

        var existing = _roleManager.Roles.Select(r => r.Name!).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missing = roleNames.Where(r => !existing.Contains(r)).ToArray();

        foreach (var roleName in missing)
        {
            var res = await _roleManager.CreateAsync(new Role { Name = roleName });
            if (!res.Succeeded)
            {
                var errors = string.Join("; ", res.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new ApplicationException($"Не удалось создать роль '{roleName}': {errors}");
            }
        }

        _logger.LogInformation("Роли добавлены в БД: создано {Created} из {Total}.", missing.Length, roleNames.Length);
        return (missing.Length, roleNames.Length);
    }

    private async Task<int> SeedRolePermissions(RolePermissionOptions cfg, CancellationToken ct)
    {
        if (cfg.Roles is null || cfg.Roles.Count == 0)
        {
            _logger.LogInformation("В конфиге сидов нет связок ролей и прав.");
            return 0;
        }

        var count = 0;
        foreach (var (roleName, permsRaw) in cfg.Roles)
        {
            var role = await _roleManager.FindByNameAsync(roleName)
                       ?? throw new ApplicationException($"Роль '{roleName}' не найдена при назначении прав.");

            var rolePermissions = (permsRaw ?? Array.Empty<string>())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (rolePermissions.Length == 0)
                continue;

            await _rolePermissionManager.AddRangeIfExist(role.Id, rolePermissions, ct);
            count += rolePermissions.Length;
        }

        _logger.LogInformation("Права для ролей добавлены в БД (суммарно связок в конфиге: {Count}).", count);
        return count;
    }

    /// <summary>
    /// Обеспечивает наличие администратора.
    /// Проверяем строго по Email. Если пользователь с таким Email уже есть — пропускаем создание и просто логируем.
    /// </summary>
    private async Task EnsureAdminAsync(CancellationToken ct)
    {
        var cfg = _adminOptions.CurrentValue;
        var policy = _identityOptions.Value.Password;

        if (string.IsNullOrWhiteSpace(cfg.UserName) ||
            string.IsNullOrWhiteSpace(cfg.Email) ||
            string.IsNullOrWhiteSpace(cfg.Password))
        {
            throw new ApplicationException("Параметры администратора не настроены (UserName/Email/Password).");
        }

        if (await _roleManager.FindByNameAsync(AdminAccount.ADMIN) is null)
        {
            var createRole = await _roleManager.CreateAsync(new Role { Name = AdminAccount.ADMIN });
            if (!createRole.Succeeded)
            {
                var roleErrors = string.Join("; ", createRole.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new ApplicationException($"Не удалось создать роль администратора: {roleErrors}");
            }
        }

        var user = await _userManager.FindByEmailAsync(cfg.Email);

        var createdNew = false;
        var roleAdded = false;
        var profileCreated = false;

        if (user is null)
        {
            var probeUser = User.CreateAdmin(cfg.UserName, cfg.Email);
            var errors = new List<string>();
            foreach (var validator in _userManager.PasswordValidators)
            {
                var result = await validator.ValidateAsync(_userManager, probeUser, cfg.Password);
                if (!result.Succeeded)
                    errors.AddRange(result.Errors.Select(e => $"{e.Code}: {e.Description}"));
            }

            if (errors.Count > 0)
                throw new ApplicationException("Пароль администратора из конфига не соответствует политике Identity: " + string.Join("; ", errors));

            user = probeUser;
            user.EmailConfirmed = true;
            var create = await _userManager.CreateAsync(user, cfg.Password);
            if (!create.Succeeded)
            {
                var createErrors = string.Join("; ", create.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new ApplicationException($"Не удалось создать пользователя-администратора: {createErrors}");
            }

            createdNew = true;
            _logger.LogInformation("Создан пользователь-администратор: user='{UserName}', email='{Email}'.", user.UserName, user.Email);
        }
        else
        {
            _logger.LogInformation("Пользователь с email '{Email}' уже существует. Пропускаю создание.", cfg.Email);
        }

        if (!await _userManager.IsInRoleAsync(user, AdminAccount.ADMIN))
        {
            var addRole = await _userManager.AddToRoleAsync(user, AdminAccount.ADMIN);
            if (!addRole.Succeeded)
            {
                var roleErrors = string.Join("; ", addRole.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new ApplicationException($"Не удалось назначить роль администратора: {roleErrors}");
            }

            roleAdded = true;
        }

        var fullNameResult = FullName.Create(cfg.UserName, cfg.UserName);
        if (fullNameResult.IsFailure)
            throw new ApplicationException($"Некорректное ФИО администратора: {fullNameResult.Error}");

        var adminAccount = new AdminAccount(fullNameResult.Value, user);
        var profileResult = await _accountsManager.CreateAdminAccount(adminAccount, ct);
        if (profileResult.IsFailure)
        {
            _logger.LogInformation("Профиль администратора уже существует или не создан: {Error}", profileResult.Error);
        }
        else
        {
            profileCreated = true;
        }

        _logger.LogInformation(
            "Администратор: созданНовым={CreatedNew}, рольНазначенаСейчас={RoleAdded}, профильСозданСейчас={ProfileCreated}.",
            createdNew, roleAdded, profileCreated);
    }
}