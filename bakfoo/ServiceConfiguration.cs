using Bakfoo;
using Bakfoo.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceConfiguration
{
    public static void AddBakfoo<TObserver>(
        this IServiceCollection services,
        IConfiguration config,
        Action<DbContextOptionsBuilder> optionsAction)
        where TObserver : class, IBakfooObserver
        => AddBakfoo<TObserver>(services,
            config.GetSection(BakfooOptions.ConfigSectionName),
            optionsAction);

    public static void AddBakfoo<TObserver>(
        this IServiceCollection services,
        IConfigurationSection config,
        Action<DbContextOptionsBuilder> optionsAction)
        where TObserver : class, IBakfooObserver
    {
        services.AddDbContext<BakfooDbContext>(optionsAction);
        services.Configure<BakfooOptions>(config);
        services.AddScoped<BakfooService>();
        services.AddHostedService<BakfooJobManager>();
        services.AddScoped<BakfooWorker>();
        services.AddScoped<IBakfooObserver, TObserver>();
    }
}
