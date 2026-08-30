using GuildOps.Domain.Guilds;

namespace GuildOps.Application.Guilds;

internal static class DefaultGuildRanks
{
    public static GuildRank Leader(Guid guildId) => new(
        guildId,
        "Chef de guilde",
        sortOrder: 0,
        permissions: Enum.GetValues<GuildPermission>().ToList(),
        isLeader: true);

    public static GuildRank Officer(Guid guildId) => new(
        guildId,
        "Officier",
        sortOrder: 1,
        permissions:
        [
            GuildPermission.ViewMembers,
            GuildPermission.InviteMember,
            GuildPermission.ReviewApplications,
            GuildPermission.KickMember,
            GuildPermission.WriteMemberNote
        ]);

    public static GuildRank Member(Guid guildId) => new(
        guildId,
        "Membre",
        sortOrder: 2,
        permissions: [GuildPermission.ViewMembers],
        isDefault: true);
}
