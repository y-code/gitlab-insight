using Bakhoo;
using Bakhoo.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

    public static void AddBakhoo<TMonitor>(
        this IServiceCollection services,
        IConfigurationSection config,
        Action<DbContextOptionsBuilder> optionsAction)
        where TMonitor : class, IBakhooJobMonitor
    {
        services.AddDbContext<BakhooDbContext>(optionsAction);
        services.Configure<BakhooOptions>(config);
        services.AddScoped<IBakhooJobRepository, BakhooJobStateService>();
        services.TryAddSingleton<IBakhooJobSequencer, BakhooJobSequencerSlim>();
        services.AddHostedService<BakhooLord>();
        services.AddScoped<IBakhooVassal, BakhooVassal>();
        services.AddScoped<IBakhooJobWindow, BakhooJobWindow>();
        services.AddScoped<IBakhooJobMonitor, TMonitor>();
    }
}
