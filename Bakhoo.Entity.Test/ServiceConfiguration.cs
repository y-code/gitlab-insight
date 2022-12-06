using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bakhoo.Entity.Test;

public static class ServiceConfiguration
{
    private static string GetNamespace()
        => (Assembly.GetAssembly(typeof(ServiceConfiguration))?.FullName
            ?? throw new Exception())
                .Replace($".{nameof(ServiceConfiguration)}", "");

	public static IServiceCollection AddBakhooEntity(
        this IServiceCollection services,
        string dbName)
	{
        services.AddDbContext<BakhooDbContext>(options =>
        {
            options.UseNpgsql();
            options.UseNpgsql(
                "User ID=postgres;" +
                    "Password=test;" +
                    "Host=localhost;" +
                    "Port=5432;" +
                    $"Database={dbName};" +
                    "Pooling=true;" +
                    "MinPoolSize=0;" +
                    "MaxPoolSize=8;" +
                    "Connection Lifetime=0;",
                x => x.MigrationsAssembly(GetNamespace()));
        });
        return services;
	}
}
