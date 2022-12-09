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
        where TObserver : class, IBakhooJobMonitor
        => AddBakhoo<TObserver>(services,
            config.GetSection(BakhooOptions.ConfigSectionName),
            optionsAction);

    public static void AddBakhoo<TObserver>(
        this IServiceCollection services,
        IConfigurationSection config,
        Action<DbContextOptionsBuilder> optionsAction)
        where TObserver : class, IBakhooJobMonitor
    {
        services.AddDbContext<BakhooDbContext>(optionsAction);
        services.Configure<BakhooOptions>(config);
        services.AddScoped<IBakhooJobRepository, BakhooJobStateService>();
        services.AddHostedService<BakhooLord>();
        services.AddScoped<IBakhooWorker, BakhooVassal>();
        services.AddScoped<IBakhooJobMonitor, TObserver>();
    }
}
