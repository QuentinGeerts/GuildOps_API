namespace GuildOps.Domain.Guilds;

/// <summary>Les droits qu'un grade peut accorder dans une guilde.</summary>
public enum GuildPermission
{
    ViewMembers = 1,
    InviteMember = 2,
    ReviewApplications = 3,
    KickMember = 4,
    AssignRank = 5,
    ManageRanks = 6,
    EditGuildProfile = 7,
    WriteMemberNote = 8
}