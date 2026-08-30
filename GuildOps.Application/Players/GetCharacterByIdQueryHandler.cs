using GuildOps.Application.Abstractions;

namespace GuildOps.Application.Players;

internal sealed class GetCharacterByIdQueryHandler(IPlayerRepository players)
    : IQueryHandler<GetCharacterByIdQuery, CharacterDto?>
{
    public async Task<CharacterDto?> HandleAsync(GetCharacterByIdQuery query, CancellationToken cancellationToken = default)
    {
        var character = await players.GetCharacterAsync(query.Id, cancellationToken);
        return character is null ? null : CharacterDto.From(character);
    }
}
