using System;
using Bakhoo.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Moq;
using Serilog;
using Xunit.Abstractions;

namespace Bakhoo.Test;

public static class Utilities
{
    public static IServiceCollection AddTestLogging(this IServiceCollection services, ITestOutputHelper output)
    {
        services.AddLogging(options =>
            options.AddSerilog(new TestLogger(output) as ILogger));
        return services;
    }

    public static T GetMocked<T>(this IServiceProvider provider)
        where T : class
    {
        return provider.GetRequiredService<Mock<T>>().Object;
    }

    public static IServiceCollection AddMockedScoped<T>(this IServiceCollection services, Action<IServiceProvider, Mock<T>> setup)
        where T : class
    {
        services.AddScoped<Mock<T>>(provider =>
        {
            var mock = new Mock<T>();
            setup(provider, mock);
            return mock;
        });
        services.AddScoped<T>(provider => provider.GetMocked<T>());
        return services;
    }

    public static IServiceCollection AddMockedScoped<T>(this IServiceCollection services, Action<Mock<T>> setup)
        where T : class
        => AddMockedScoped<T>(services, (_, mock) => setup(mock));

    public static string CreateTestDbName()
        => $"test_{Guid.NewGuid().ToString().Substring(0, 8)}";

    public static void CreateTestDb<TDbContext>(this IServiceProvider provider)
        where TDbContext : DbContext
    {
        using (var scope = provider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
            dbContext.Database.EnsureCreated();
        }
    }

    public static void DeleteTestDb<TDbContext>(this IServiceProvider provider)
        where TDbContext : DbContext
    {
        using (var scope = provider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
            dbContext.Database.EnsureDeleted();
        }
    }
}

public static class ItIs
{
    public static CancellationToken CT()
        => It.IsAny<CancellationToken>();
}

public static class KeepAsserting
{
    public static async Task UntilCancelled(
        CancellationToken ct,
        Action assert,
        bool isDebugging = false)
    {
        if (isDebugging)
            await Task.Delay(TimeSpan.FromDays(1));

        while (InnerKeepAssertingUntilCancelled(ct, assert))
            await Task.Delay(TimeSpan.FromMicroseconds(50));
    }

    private static bool InnerKeepAssertingUntilCancelled(CancellationToken ct, Action assert)
    {
        try
        {
            assert();
        }
        catch
        {
            if (!ct.IsCancellationRequested)
                return true;
            throw;
        }
        return false;
    }
}
