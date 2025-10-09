using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Application.JWT;
using AuthService.Contracts.Models;
using AuthService.Domain;
using AuthService.Infrastructure.Postgres.IdentityManagers;
using AuthService.Infrastructure.Postgres.Options;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharedKernel;

namespace AuthService.Infrastructure.Postgres;

public class JwtTokenProvider : ITokenProvider
{
    private readonly PermissionManager _permissionManager;
    private readonly PostgresDbContext _accountWriteContext;
    private readonly UserManager<User> _userManager;
    private readonly JwtOptions _jwtOptions;

    public JwtTokenProvider(
        IOptions<JwtOptions> options,
        PermissionManager permissionManager,
        PostgresDbContext accountWriteContext,
        UserManager<User> userManager)
    {
        _permissionManager = permissionManager ?? throw new ArgumentNullException(nameof(permissionManager));
        _accountWriteContext = accountWriteContext ?? throw new ArgumentNullException(nameof(accountWriteContext));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _jwtOptions = (options ?? throw new ArgumentNullException(nameof(options))).Value;
    }

    public async Task<JwtTokenResult> GenerateAccessToken(User user, CancellationToken cancellationToken)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var roleNames = await _userManager.GetRolesAsync(user);
        var roleClaims = roleNames
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(n => new Claim(CustomClaims.Role, n));

        var permissions = await _permissionManager.GetUserPermissionCodes(user.Id, cancellationToken);
        var permissionClaims = permissions
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(p => new Claim(CustomClaims.Permission, p));

        var jti = Guid.NewGuid();

        var claims = new List<Claim>
        {
            new Claim(CustomClaims.Id, user.Id.ToString()),
            new Claim(CustomClaims.Jti, jti.ToString()),
            new Claim(CustomClaims.Email, user.Email ?? string.Empty),
        };

        claims.AddRange(roleClaims);
        claims.AddRange(permissionClaims);

        var jwtToken = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenLifetimeMinutes),
            signingCredentials: signingCredentials,
            claims: claims);

        var jwtStringToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);
        return new JwtTokenResult(jwtStringToken, jti);
    }

    public async Task<Guid> GenerateRefreshToken(User user, Guid accessTokenJti, CancellationToken cancellationToken)
    {
        var refreshSession = new RefreshSession
        {
            Id = Guid.NewGuid(),
            User = user,
            ExpiresIn = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenLifetimeDays),
            CreatedAt = DateTime.UtcNow,
            Jti = accessTokenJti,
            RefreshToken = Guid.NewGuid(),
        };

        _accountWriteContext.Add(refreshSession);
        await _accountWriteContext.SaveChangesAsync(cancellationToken);

        return refreshSession.RefreshToken;
    }

    public async Task<Result<IReadOnlyList<Claim>, Error>> GetUserClaims(
        string jwtToken, CancellationToken cancellationToken)
    {
        var jwtHandler = new JwtSecurityTokenHandler();
        var validationParameters = TokenValidationParametersFactory.CreateWithoutLifeTime(_jwtOptions);

        var validationResult = await jwtHandler.ValidateTokenAsync(jwtToken, validationParameters);
        if (!validationResult.IsValid)
            return Errors.Tokens.InvalidToken();

        return validationResult.ClaimsIdentity.Claims.ToList();
    }
}