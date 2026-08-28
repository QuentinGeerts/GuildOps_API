namespace GuildOps.Domain.Players;

public sealed class Character
{
    public Character() { }

    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public string Name { get; set; }
    public string Realm { get; set; }

    public int Level { get; set; }
}
