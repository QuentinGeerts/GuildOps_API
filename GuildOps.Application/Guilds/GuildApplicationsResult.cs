namespace GuildOps.Application.Guilds;

public enum GuildApplicationsOutcome
{
    Retrieved = 1,
    Forbidden = 2
}

public sealed record GuildApplicationsResult(
    GuildApplicationsOutcome Outcome,
    IReadOnlyList<GuildApplicationDto> Applications)
{
    public static GuildApplicationsResult Retrieved(IReadOnlyList<GuildApplicationDto> applications)
        => new(GuildApplicationsOutcome.Retrieved, applications);

    public static readonly GuildApplicationsResult Forbidden = new(GuildApplicationsOutcome.Forbidden, []);
}
