using System.Globalization;
using Doner;
using Doner.DataBase;
using Doner.Features.AuthFeature;
using Doner.Features.WorkspaceFeature;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load("../.env");
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.AddFeature<AuthFeature>();
builder.AddFeature<WorkspaceFeature>();
builder.Services.AddDbContextFactory<AppDbContext>(optionsBuilder =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    optionsBuilder.UseSqlServer(connectionString);
});

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var app = builder.Build();

// app.MigrateDatabase<AppDbContext>();

app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureDeleted();
app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("ru")
};
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
    RequestCultureProviders =
    [
        new QueryStringRequestCultureProvider { QueryStringKey = "lang" }
    ]
});


app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseFeature<AuthFeature>();
app.UseFeature<WorkspaceFeature>();
app.Run();