using Doner;
using Doner.DataBase;
using Doner.Features.AuthFeature;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.AddFeature<AuthFeature>();
builder.Services.AddDbContextFactory<AppDbContext>();

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
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseFeature<AuthFeature>();
app.Run();