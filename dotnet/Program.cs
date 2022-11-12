using GitLabInsight.Domain;
using GitLabInsight.Services;
using Microsoft.AspNetCore.Builder;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerBuilder) => loggerBuilder.ReadFrom.Configuration(context.Configuration));
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();
builder.Services.AddLogging();

builder.Services.AddSignalR();

// Add services to the container.
builder.Services.Configure<GitLabInsightOptions>(
    builder.Configuration.GetSection(GitLabInsightOptions.ConfigSectionName));
builder.Services.AddSingleton<YouTrackClientService>();

builder.Services.AddControllersWithViews();

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
