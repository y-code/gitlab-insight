using Bakhoo.Entity;
using Bakhoo.Entity.Test;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var dbName = $"test_{Guid.NewGuid()}";
Console.WriteLine($"Database: {dbName}");

await Host.CreateDefaultBuilder(args)
    .ConfigureServices(x => x.AddBakhooEntity(dbName))
    .Build().RunAsync();
