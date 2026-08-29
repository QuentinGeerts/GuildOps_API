namespace GuildOps.Domain.Guilds;

public class GuildRank
{
    private GuildRank() { }

    public GuildRank(Guid guildId, string name, int sortOrder, List<GuildPermission> permissions, bool isLeader = false, bool isDefault = false)
    {
        Id = Guid.CreateVersion7();
        CreatedAt = DateTimeOffset.UtcNow;
        GuildId = guildId;
        Name = name;
        SortOrder = sortOrder;
        Permissions = permissions;
        IsLeader = isLeader;
        IsDefault = isDefault;
    }

    public Guid Id { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public Guid GuildId { get; set; }
    public Guild? Guild { get; set; }

    public string Name { get; set; } = null!;
    public int SortOrder { get; set; }
    public List<GuildPermission> Permissions { get; set; } = [];
    public bool IsLeader { get; set; }
    public bool IsDefault { get; set; }
}