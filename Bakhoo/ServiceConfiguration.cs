using Bakhoo;
using Bakhoo.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceConfiguration
{
    public static void AddBakhoo<TObserver>(
        this IServiceCollection services,
        IConfiguration config,
        Action<DbContextOptionsBuilder> optionsAction)
        where TObserver : class, IBakhooJobStateObserver
        => AddBakhoo<TObserver>(services,
            config.GetSection(BakhooOptions.ConfigSectionName),
            optionsAction);

    public static void AddBakhoo<TObserver>(
        this IServiceCollection services,
        IConfigurationSection config,
        Action<DbContextOptionsBuilder> optionsAction)
        where TObserver : class, IBakhooJobStateObserver
    {
        services.AddDbContext<BakhooDbContext>(optionsAction);
        services.Configure<BakhooOptions>(config);
        services.AddScoped<IBakhooJobStateService, BakhooJobStateService>();
        services.AddHostedService<BakhooJobManager>();
        services.AddScoped<IBakhooWorker, BakhooWorker>();
        services.AddScoped<IBakhooJobStateObserver, TObserver>();
    }
}
