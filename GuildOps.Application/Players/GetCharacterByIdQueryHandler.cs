using GuildOps.Application.Abstractions;

namespace GuildOps.Application.Players;

internal sealed class GetCharacterByIdQueryHandler(IPlayerRepository players)
    : IQueryHandler<GetCharacterByIdQuery, CharacterDetailsDto?>
{
    public async Task<CharacterDetailsDto?> HandleAsync(GetCharacterByIdQuery query, CancellationToken cancellationToken = default)
    {
        var character = await players.GetCharacterWithDetailsAsync(query.Id, cancellationToken);
        return character is null ? null : CharacterDetailsDto.From(character);
    }
}
