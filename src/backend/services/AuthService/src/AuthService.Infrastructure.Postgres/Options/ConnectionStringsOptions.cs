namespace AuthService.Infrastructure.Postgres.Options;

public sealed class ConnectionStringsOptions
{
    public const string SECTION_NAME = "ConnectionStrings";

    public string Database { get; init; } = string.Empty;
}