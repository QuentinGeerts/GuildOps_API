using GuildOps.Domain.Games;

namespace GuildOps.Domain.Guilds;

public class Guild
{
    private Guild() { }
    
    public Guild(Guid gameId, string name, string server, string? description = null, string? chatUrl = null)
    {
        Id = Guid.CreateVersion7();
        CreatedAt = DateTimeOffset.UtcNow;
        GameId = gameId;
        Name = name;
        Server = server;
        Description = description;
        ChatUrl = chatUrl;
    }

    public Guid Id { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public Guid GameId { get; set; }
    public Game? Game { get; set; }

    public string Name { get; set; } = null!;
    public string Server { get; set; } = null!;
    public string? Description { get; set; }
    public string? ChatUrl { get; set; }

    public ICollection<GuildRank> Ranks { get; set; } = [];
    public ICollection<GuildMembership> Memberships { get; set; } = [];
}
