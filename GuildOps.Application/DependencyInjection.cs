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
        services.AddScoped<IQueryHandler<GetCharacterByIdQuery, CharacterDto?>, GetCharacterByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetGuildByIdQuery, GuildDetailsDto?>, GetGuildByIdQueryHandler>();

        services.AddScoped<ICommandHandler<RegisterPlayerCommand, RegisterPlayerResult>, RegisterPlayerCommandHandler>();
        services.AddScoped<ICommandHandler<LoginCommand, LoginResult>, LoginCommandHandler>();
        services.AddScoped<ICommandHandler<CreateCharacterCommand, CreateCharacterResult>, CreateCharacterCommandHandler>();
        services.AddScoped<ICommandHandler<CreateGuildCommand, CreateGuildResult>, CreateGuildCommandHandler>();

        return services;
    }
}
