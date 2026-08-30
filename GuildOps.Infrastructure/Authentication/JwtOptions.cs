namespace GuildOps.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public string Key { get; set; } = null!;
    public int ExpiryMinutes { get; set; } = 60;
    public int RefreshExpiryDays { get; set; } = 14;
}
