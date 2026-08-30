using GuildOps.Application.Abstractions;
using GuildOps.Application.Games;
using GuildOps.Application.Guilds;
using GuildOps.Application.Players;
using Microsoft.Extensions.DependencyInjection;

namespace GuildOps.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services.AddHandlers();
    }

    private static IServiceCollection AddHandlers(this IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<GetGamesQuery, IReadOnlyList<GameDto>>, GetGamesQueryHandler>();
        services.AddScoped<IQueryHandler<GetGameByIdQuery, GameDetailsDto?>, GetGameByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetPlayerQuery, PlayerDto?>, GetPlayerQueryHandler>();
        services.AddScoped<IQueryHandler<GetCharacterByIdQuery, CharacterDetailsDto?>, GetCharacterByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetGuildByIdQuery, GuildDetailsDto?>, GetGuildByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetGuildApplicationsQuery, GuildApplicationsResult>, GetGuildApplicationsQueryHandler>();
        services.AddScoped<IQueryHandler<GetGuildInvitationsQuery, GuildInvitationsResult>, GetGuildInvitationsQueryHandler>();
        services.AddScoped<IQueryHandler<GetPlayerInvitationsQuery, IReadOnlyList<PlayerInvitationDto>>, GetPlayerInvitationsQueryHandler>();

        services.AddScoped<ICommandHandler<RegisterPlayerCommand, RegisterPlayerResult>, RegisterPlayerCommandHandler>();
        services.AddScoped<ICommandHandler<LoginCommand, LoginResult>, LoginCommandHandler>();
        services.AddScoped<ICommandHandler<CreateCharacterCommand, CreateCharacterResult>, CreateCharacterCommandHandler>();
        services.AddScoped<ICommandHandler<SetCharacterRolesCommand, SetCharacterRolesOutcome>, SetCharacterRolesCommandHandler>();
        services.AddScoped<ICommandHandler<SetCharacterAvailabilitiesCommand, SetCharacterAvailabilitiesOutcome>, SetCharacterAvailabilitiesCommandHandler>();
        services.AddScoped<ICommandHandler<CreateGuildCommand, CreateGuildResult>, CreateGuildCommandHandler>();
        services.AddScoped<ICommandHandler<ApplyToGuildCommand, ApplyToGuildResult>, ApplyToGuildCommandHandler>();
        services.AddScoped<ICommandHandler<AcceptGuildApplicationCommand, AcceptGuildApplicationOutcome>, AcceptGuildApplicationCommandHandler>();
        services.AddScoped<ICommandHandler<RejectGuildApplicationCommand, RejectGuildApplicationOutcome>, RejectGuildApplicationCommandHandler>();
        services.AddScoped<ICommandHandler<InviteCharacterCommand, InviteCharacterResult>, InviteCharacterCommandHandler>();
        services.AddScoped<ICommandHandler<AcceptGuildInvitationCommand, AcceptGuildInvitationOutcome>, AcceptGuildInvitationCommandHandler>();
        services.AddScoped<ICommandHandler<DeclineGuildInvitationCommand, DeclineGuildInvitationOutcome>, DeclineGuildInvitationCommandHandler>();

        return services;
    }
}
