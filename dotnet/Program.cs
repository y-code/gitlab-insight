using GitLabInsight.Domain;
using GitLabInsight.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerBuilder) => loggerBuilder.ReadFrom.Configuration(context.Configuration));
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();
builder.Services.AddLogging();

// Add services to the container.
builder.Services.Configure<GitLabInsightOptions>(
    builder.Configuration.GetSection(GitLabInsightOptions.ConfigSectionName));
builder.Services.AddSingleton<YouTrackClientService>();

builder.Services.AddControllersWithViews();

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


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");;

app.Run();
