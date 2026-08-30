using GuildOps.Domain.Guilds;

namespace GuildOps.Application.Guilds;

public sealed record GuildMemberDto(
    Guid CharacterId,
    string CharacterName,
    int Level,
    Guid RankId,
    string RankName,
    string? Note,
    DateTimeOffset JoinedAt)
{
    public static GuildMemberDto From(GuildMembership membership)
        => new(membership.CharacterId,
               membership.Character!.Name,
               membership.Character.Level,
               membership.GuildRankId,
               membership.Rank!.Name,
               membership.Note,
               membership.JoinedAt);
}
