namespace GuildOps.Application.Players;

public sealed record DeleteCharacterCommand(Guid PlayerId, Guid CharacterId);

public enum DeleteCharacterOutcome
{
    Deleted = 1,
    CharacterNotFound = 2
}
