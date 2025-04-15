using Doner.DataBase;
using Doner.Features.AuthFeature;
using Doner.Features.MarkdownFeature;
using Doner.Features.ReelsFeature;
using Doner.Features.WorkspaceFeature;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace Doner;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        DotNetEnv.Env.Load("../.env");
        builder.Configuration.AddEnvironmentVariables();

        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.AddFeature<AuthFeature>();
        builder.AddFeature<ReelsFeature>();
        builder.AddFeature<WorkspaceFeature>();
        builder.AddFeature<MarkdownFeature>();
        builder.Services.AddDbContextFactory<AppDbContext>(optionsBuilder =>
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(connectionString);
        });
        builder.Services.AddSingleton(_ => new MongoClient(builder.Configuration.GetConnectionString("MongoDb")).GetDatabase("Doner"));
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();

        var app = builder.Build();

        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.MigrateDatabase<AppDbContext>();
            // app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureDeleted();
            // app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }
        app.UseHttpsRedirection();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseFeature<AuthFeature>();
        app.UseFeature<ReelsFeature>();
        app.UseFeature<WorkspaceFeature>();
        app.UseFeature<MarkdownFeature>();

        app.Run();
    }
}