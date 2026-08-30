namespace GuildOps.Application.Players;

public enum CreateCharacterOutcome
{
    Created = 1,
    PlayerNotFound = 2,
    GameNotFound = 3,
    ClassNotInGame = 4,
    LevelOutOfRange = 5,
    NameTakenOnServer = 6
}

public sealed record CreateCharacterResult(CreateCharacterOutcome Outcome, CharacterDto? Character)
{
    public static CreateCharacterResult Created(CharacterDto character)
        => new(CreateCharacterOutcome.Created, character);

    public static CreateCharacterResult Rejected(CreateCharacterOutcome outcome)
        => new(outcome, null);
}
