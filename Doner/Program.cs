using Doner.DataBase;
using Doner.Features.AuthFeature;
using Doner.Features.MarkdownFeature;
using Doner.Features.ReelsFeature;
using Doner.Features.WorkspaceFeature;
using Doner.Swagger;
using DotNetEnv;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Doner;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Env.Load("../.env");
        builder.Configuration.AddEnvironmentVariables();

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        builder.Services.AddSwaggerGen();

        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.AddFeature<AuthFeature>();
        builder.AddFeature<ReelsFeature>();
        builder.AddFeature<WorkspaceFeature>();
        builder.AddFeature<MarkdownFeature>();

        builder.Services.AddSignalR();

        builder.Services.AddDbContextFactory<AppDbContext>(optionsBuilder =>
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(connectionString);
        });
        builder.Services.AddSingleton(_ =>
            new MongoClient(builder.Configuration.GetConnectionString("MongoDb")).GetDatabase("Doner"));
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();

        var app = builder.Build();

        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.MigrateDatabase<AppDbContext>();
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseHttpsRedirection();
        app.UseCors();
        app.UseFeature<AuthFeature>();
        app.UseFeature<ReelsFeature>();
        app.UseFeature<WorkspaceFeature>();
        app.UseFeature<MarkdownFeature>();

        app.Run();
    }
}