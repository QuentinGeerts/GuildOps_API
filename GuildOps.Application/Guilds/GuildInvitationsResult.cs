namespace GuildOps.Application.Guilds;

public enum GuildInvitationsOutcome
{
    Retrieved = 1,
    Forbidden = 2
}

public sealed record GuildInvitationsResult(
    GuildInvitationsOutcome Outcome,
    IReadOnlyList<GuildInvitationDto> Invitations)
{
    public static GuildInvitationsResult Retrieved(IReadOnlyList<GuildInvitationDto> invitations)
        => new(GuildInvitationsOutcome.Retrieved, invitations);

    public static readonly GuildInvitationsResult Forbidden = new(GuildInvitationsOutcome.Forbidden, []);
}
