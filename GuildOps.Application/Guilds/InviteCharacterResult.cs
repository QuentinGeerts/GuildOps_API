namespace GuildOps.Application.Guilds;

public enum InviteCharacterOutcome
{
    Created = 1,
    Forbidden = 2,
    GuildNotFound = 3,
    CharacterNotFound = 4,
    DifferentGameOrServer = 5,
    CharacterAlreadyInGuild = 6,
    AlreadyInvited = 7
}

public sealed record InviteCharacterResult(InviteCharacterOutcome Outcome, GuildInvitationDto? Invitation)
{
    public static InviteCharacterResult Created(GuildInvitationDto invitation)
        => new(InviteCharacterOutcome.Created, invitation);

    public static InviteCharacterResult Rejected(InviteCharacterOutcome outcome) => new(outcome, null);
}
