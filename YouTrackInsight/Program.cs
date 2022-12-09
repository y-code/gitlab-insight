using System.Text.Json.Serialization;
using YouTrackInsight.Domain;
using YouTrackInsight.Services;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using YouTrackInsight.Entity;
using Microsoft.EntityFrameworkCore;
using Bakhoo.Entity;
using Bakhoo;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerBuilder) => loggerBuilder.ReadFrom.Configuration(context.Configuration));
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();
builder.Services.AddLogging();

builder.Services.AddSignalR();

builder.Services.AddDbContext<YTInsightDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("yt_insight_db"),
        x => x.MigrationsAssembly("YouTrackInsight"));
});

builder.Services.AddBakhoo<YouTrackInsightHubClients>(
    builder.Configuration.GetSection("Bakhoo"),
    dbOptions =>
    {
        dbOptions.UseNpgsql(builder.Configuration.GetConnectionString("yt_insight_db"),
            x => x.MigrationsAssembly("YouTrackInsight"));
    });

builder.Services.Configure<YouTrackInsightOptions>(
    builder.Configuration.GetSection(YouTrackInsightOptions.ConfigSectionName));
builder.Services.AddSingleton<YouTrackClientService>();
builder.Services.AddScoped<YTIssueImportService>();
builder.Services.AddScoped<YouTrackInsightHubClients>();
builder.Services.AddScoped<IBakhooJobHandler>(provider
    => provider.GetRequiredService<YTIssueImportService>());

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters
            .Add(new JsonStringEnumConverter());
    });

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "YouTrack Insight API V1", Version = "v1" });
    options.CustomOperationIds(apiDesc =>
    {
        return apiDesc.TryGetMethodInfo(out var methodInfo) ? methodInfo.Name : null;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

void UpdateDatabaseFor<TDbContext>() where TDbContext : DbContext
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        //context.Database.EnsureCreated();
        var applied = context.Database.GetAppliedMigrations().ToArray();
        var pendingMigrations = context.Database.GetPendingMigrations();
        if (pendingMigrations.Any())
        {
            Console.Write($"Database has pending migrations for {typeof(TDbContext).Name}. Do you want to apply them? (FALSE/true): ");
            var input = Console.ReadLine();
            if (Boolean.TryParse(input, out var isMigrating) && isMigrating)
            {
                context.Database.Migrate();
            }
            else
                throw new InvalidOperationException($"Database has pending migrations for {typeof(TDbContext).Name}.");
        }
    }
}

UpdateDatabaseFor<YTInsightDbContext>();
UpdateDatabaseFor<BakhooDbContext>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSwagger(options =>
{
    options.RouteTemplate = "api-doc/{documentName}/swagger.json";
});
app.UseSwaggerUI(options =>
{
});

app.MapHub<YouTrackInsightHub>("/hub");
app.MapSwagger();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();
