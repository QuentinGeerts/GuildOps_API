using GuildOps.Application.Abstractions;
using GuildOps.Domain.Guilds;

namespace GuildOps.Application.Guilds;

internal sealed class ApplyToGuildCommandHandler(
    IPlayerRepository players,
    IGuildRepository guilds,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ApplyToGuildCommand, ApplyToGuildResult>
{
    private const string ApplicationIndex = "IX_GuildApplications_GuildId_CharacterId";

    public async Task<ApplyToGuildResult> HandleAsync(ApplyToGuildCommand command, CancellationToken cancellationToken = default)
    {
        var character = await players.GetCharacterAsync(command.CharacterId, cancellationToken);

        if (character is null || character.PlayerId != command.PlayerId)
        {
            return ApplyToGuildResult.Rejected(ApplyToGuildOutcome.CharacterNotFound);
        }

        var guild = await guilds.GetAsync(command.GuildId, cancellationToken);
        if (guild is null)
        {
            return ApplyToGuildResult.Rejected(ApplyToGuildOutcome.GuildNotFound);
        }

        if (guild.GameId != character.GameId || guild.Server != character.Server)
        {
            return ApplyToGuildResult.Rejected(ApplyToGuildOutcome.DifferentGameOrServer);
        }

        if (await guilds.CharacterHasMembershipAsync(character.Id, cancellationToken))
        {
            return ApplyToGuildResult.Rejected(ApplyToGuildOutcome.CharacterAlreadyInGuild);
        }

        if (await guilds.ApplicationExistsAsync(guild.Id, character.Id, cancellationToken))
        {
            return ApplyToGuildResult.Rejected(ApplyToGuildOutcome.AlreadyApplied);
        }

        var application = new GuildApplication(guild.Id, character.Id, command.Message);
        guilds.AddApplication(application);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintException exception) when (exception.ConstraintName == ApplicationIndex)
        {
            return ApplyToGuildResult.Rejected(ApplyToGuildOutcome.AlreadyApplied);
        }

        return ApplyToGuildResult.Created(GuildApplicationDto.From(application, character));
    }
}
