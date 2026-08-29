using GuildOps.Application.Abstractions;
using GuildOps.Application.Games;
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
        return services;
    }
}
