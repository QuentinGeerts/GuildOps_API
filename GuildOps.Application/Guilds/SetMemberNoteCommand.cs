using System.ComponentModel.DataAnnotations;

namespace GuildOps.Application.Guilds;

public sealed record SetMemberNoteRequest([StringLength(1000)] string? Note = null);

public sealed record SetMemberNoteCommand(Guid PlayerId, Guid GuildId, Guid CharacterId, string? Note)
{
    public static SetMemberNoteCommand From(Guid playerId, Guid guildId, Guid characterId, SetMemberNoteRequest request)
        => new(playerId, guildId, characterId, request.Note);
}

public enum SetMemberNoteOutcome
{
    Updated = 1,
    Forbidden = 2,
    MembershipNotFound = 3
}
