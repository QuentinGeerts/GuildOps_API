namespace GuildOps.Application.Players;

public sealed record CreateCharacterCommand(
    Guid PlayerId,
    Guid GameId,
    Guid CharacterClassId,
    string Name,
    string Server,
    int Level)
{
    public static CreateCharacterCommand From(Guid playerId, CreateCharacterRequest request)
        => new(playerId, request.GameId, request.CharacterClassId, request.Name, request.Server, request.Level);
}
