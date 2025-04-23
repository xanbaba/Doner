using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Contracts.V1.Requests;
using Contracts.V1.Responses;
using Doner.DataBase;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Mongo2Go;
using MongoDB.Driver;
using Moq;
using StackExchange.Redis;

namespace Doner.Tests;

public class ReelEndpointTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly MongoDbRunner _mongoDbRunner;


    public ReelEndpointTests(WebApplicationFactory<Program> factory)
    {
        _mongoDbRunner = MongoDbRunner.Start();
        var mongoClient = new MongoClient(_mongoDbRunner.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase("DonerTestDb");

        factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((webHostBuilderContext, _) =>
            {
                webHostBuilderContext.HostingEnvironment.EnvironmentName = "Testing";
            });
            builder.ConfigureServices(services =>
            {
                // Remove existing database context registrations
                services.Remove(
                    services.Single(d => d.ServiceType == typeof(IDbContextOptionsConfiguration<AppDbContext>))
                );

                var descriptorMongo = services.SingleOrDefault(s => s.ServiceType == typeof(IMongoDatabase));
                if (descriptorMongo != null)
                {
                    services.Remove(descriptorMongo);
                }
                
                var redisDescriptor = services.SingleOrDefault(s => s.ServiceType == typeof(IConnectionMultiplexer));
                if (redisDescriptor != null)
                {
                    services.Remove(redisDescriptor);
                }
                services.AddSingleton(new Mock<IConnectionMultiplexer>().Object);

                services.AddDbContextFactory<AppDbContext>(optionsBuilder =>
                    optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString()));
                services.AddSingleton(mongoDatabase);
            });
        });

        _httpClient = factory.CreateClient();

        _httpClient.BaseAddress = new Uri(factory.Server.BaseAddress, "/api/v1/");
    }


    private async Task<string> GetJwtTokenAsync()
    {
        var signUpRequest = new SignUpRequest
        {
            FirstName = "Test",
            LastName = "User",
            Login = "testuser",
            Password = "Password123!"
        };

        var signInRequest = new SignInRequest
        {
            Login = "testuser",
            Password = "Password123!"
        };

        await _httpClient.PostAsJsonAsync("sign-up", signUpRequest);
        var response = await _httpClient.PostAsJsonAsync("sign-in", signInRequest);
        response.EnsureSuccessStatusCode();

        var tokens = await response.Content.ReadFromJsonAsync<TokensResponse>();
        return tokens!.AccessToken;
    }

    [Fact]
    public async Task GetReels_ShouldReturnUnauthorized_WhenNoToken()
    {
        var response = await _httpClient.GetAsync($"users/me/workspaces/{Guid.NewGuid()}/reels");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetReels_ShouldReturnOk_WhenValidToken()
    {
        // Authorize
        var token = await GetJwtTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a workspace
        var workspaceRequest = new { name = "Test Workspace" };
        var addWorkspaceResponse = await _httpClient.PostAsJsonAsync("users/me/workspaces", workspaceRequest);
        addWorkspaceResponse.EnsureSuccessStatusCode();
        var addedWorkspace = await addWorkspaceResponse.Content.ReadFromJsonAsync<WorkspaceResponse>();
        Assert.NotNull(addedWorkspace);
        
        // Add a reel
        var addRequest = new AddReelRequest { Name = "Test Reel", Description = "Test Description" };
        var addReelResponse = await _httpClient.PostAsJsonAsync($"users/me/workspaces/{addedWorkspace.Id}/reels", addRequest);
        addReelResponse.EnsureSuccessStatusCode();
        var addedReel = await addReelResponse.Content.ReadFromJsonAsync<ReelResponse>();
        Assert.NotNull(addedReel);
        
        // Get reels of workspace
        var response = await _httpClient.GetAsync($"users/me/workspaces/{addedWorkspace.Id}/reels");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var reels = await response.Content.ReadFromJsonAsync<ReelsResponse>();
        Assert.NotNull(reels);
        reels.Items.Should().ContainSingle().Which.Should().BeEquivalentTo(addedReel);
    }

    [Fact]
    public async Task GetReelById_ShouldReturnUnauthorized_WhenNoToken()
    {
        var response = await _httpClient.GetAsync($"users/me/reels/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetReelById_ShouldReturnOk_WhenValidToken()
    {
        var token = await GetJwtTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a workspace
        var workspaceRequest = new { name = "Test Workspace" };
        var workspaceResponse = await _httpClient.PostAsJsonAsync("users/me/workspaces", workspaceRequest);
        workspaceResponse.EnsureSuccessStatusCode();
        var workspace = await workspaceResponse.Content.ReadFromJsonAsync<WorkspaceResponse>();
        Assert.NotNull(workspace);

        // Add a reel
        var addRequest = new AddReelRequest { Name = "Test Reel", Description = "Test Description" };
        var addResponse = await _httpClient.PostAsJsonAsync($"users/me/workspaces/{workspace.Id}/reels", addRequest);
        addResponse.EnsureSuccessStatusCode();
        var addedReel = await addResponse.Content.ReadFromJsonAsync<ReelResponse>();
        Assert.NotNull(addedReel);

        // Compare added reel with request data
        Assert.Equal(addRequest.Name, addedReel.Name);
        Assert.Equal(addRequest.Description, addedReel.Description);

        // Get the reel by ID
        var response = await _httpClient.GetAsync($"users/me/reels/{addedReel.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Ensure all data is the same
        var retrievedReel = await response.Content.ReadFromJsonAsync<ReelResponse>();
        Assert.NotNull(retrievedReel);
        Assert.Equal(addedReel.Id, retrievedReel.Id);
        Assert.Equal(addedReel.Name, retrievedReel.Name);
        Assert.Equal(addedReel.Description, retrievedReel.Description);
    }

    [Fact]
    public async Task AddReel_ShouldReturnUnauthorized_WhenNoToken()
    {
        var request = new AddReelRequest { Name = "Test Reel", Description = "Test Description" };
        var response = await _httpClient.PostAsJsonAsync($"users/me/workspaces/{Guid.NewGuid()}/reels", request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AddReel_ShouldReturnCreated_WhenValidToken()
    {
        var token = await GetJwtTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a workspace
        var workspaceRequest = new { name = "Test Workspace" };
        var workspaceResponse = await _httpClient.PostAsJsonAsync("users/me/workspaces", workspaceRequest);
        workspaceResponse.EnsureSuccessStatusCode();
        var workspace = await workspaceResponse.Content.ReadFromJsonAsync<WorkspaceResponse>();
        Assert.NotNull(workspace);

        var request = new AddReelRequest { Name = "Test Reel", Description = "Test Description" };
        var response = await _httpClient.PostAsJsonAsync($"users/me/workspaces/{workspace.Id}/reels", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var reel = await response.Content.ReadFromJsonAsync<ReelResponse>();
        Assert.NotNull(reel);
        Assert.Equal("Test Reel", reel.Name);
    }

    [Fact]
    public async Task UpdateReel_ShouldReturnUnauthorized_WhenNoToken()
    {
        var request = new UpdateReelRequest { Name = "Updated Reel", Description = "Updated Description" };
        var response = await _httpClient.PutAsJsonAsync($"users/me/reels/{Guid.NewGuid()}", request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateReel_ShouldReturnNoContent_WhenValidToken()
    {
        var token = await GetJwtTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a workspace
        var workspaceRequest = new { name = "Test Workspace" };
        var workspaceResponse = await _httpClient.PostAsJsonAsync("users/me/workspaces", workspaceRequest);
        workspaceResponse.EnsureSuccessStatusCode();
        var workspace = await workspaceResponse.Content.ReadFromJsonAsync<WorkspaceResponse>();
        Assert.NotNull(workspace);

        // Add a reel
        var addRequest = new AddReelRequest { Name = "Test Reel", Description = "Test Description" };
        var addResponse = await _httpClient.PostAsJsonAsync($"users/me/workspaces/{workspace.Id}/reels", addRequest);
        addResponse.EnsureSuccessStatusCode();
        var addedReel = await addResponse.Content.ReadFromJsonAsync<ReelResponse>();
        Assert.NotNull(addedReel);

        var request = new UpdateReelRequest { Name = "Updated Reel", Description = "Updated Description" };
        var response = await _httpClient.PutAsJsonAsync($"users/me/reels/{addedReel.Id}", request);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Get the reel by ID
        var getResponse = await _httpClient.GetAsync($"users/me/reels/{addedReel.Id}");
        getResponse.EnsureSuccessStatusCode();

        // Ensure all data is the same
        var retrievedReel = await getResponse.Content.ReadFromJsonAsync<ReelResponse>();
        Assert.NotNull(retrievedReel);
        Assert.Equal(addedReel.Id, retrievedReel.Id);
        Assert.Equal("Updated Reel", retrievedReel.Name);
        Assert.Equal("Updated Description", retrievedReel.Description);
    }

    [Fact]
    public async Task DeleteReel_ShouldReturnUnauthorized_WhenNoToken()
    {
        var response = await _httpClient.DeleteAsync($"users/me/reels/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteReel_ShouldReturnNoContent_WhenValidToken()
    {
        var token = await GetJwtTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a workspace
        var workspaceRequest = new { name = "Test Workspace" };
        var workspaceResponse = await _httpClient.PostAsJsonAsync("users/me/workspaces", workspaceRequest);
        workspaceResponse.EnsureSuccessStatusCode();
        var workspace = await workspaceResponse.Content.ReadFromJsonAsync<WorkspaceResponse>();
        Assert.NotNull(workspace);

        // Add a reel
        var addRequest = new AddReelRequest { Name = "Test Reel", Description = "Test Description" };
        var addResponse = await _httpClient.PostAsJsonAsync($"users/me/workspaces/{workspace.Id}/reels", addRequest);
        addResponse.EnsureSuccessStatusCode();
        var addedReel = await addResponse.Content.ReadFromJsonAsync<ReelResponse>();
        Assert.NotNull(addedReel);

        var response = await _httpClient.DeleteAsync($"users/me/reels/{addedReel.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Get the deleted reel by ID and check if status code is 404
        var getDeletedReelResponse = await _httpClient.GetAsync($"users/me/reels/{addedReel.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getDeletedReelResponse.StatusCode);
    }

    [Fact]
    public async Task AddAndGetReel_ShouldReturnCorrectReel()
    {
        var token = await GetJwtTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a workspace
        var workspaceRequest = new { name = "Test Workspace" };
        var workspaceResponse = await _httpClient.PostAsJsonAsync("users/me/workspaces", workspaceRequest);
        workspaceResponse.EnsureSuccessStatusCode();
        var workspace = await workspaceResponse.Content.ReadFromJsonAsync<WorkspaceResponse>();
        Assert.NotNull(workspace);

        var addRequest = new AddReelRequest { Name = "Test Reel", Description = "Test Description" };
        var addResponse = await _httpClient.PostAsJsonAsync($"users/me/workspaces/{workspace.Id}/reels", addRequest);
        addResponse.EnsureSuccessStatusCode();

        var addedReel = await addResponse.Content.ReadFromJsonAsync<ReelResponse>();
        Assert.NotNull(addedReel);
        Assert.Equal("Test Reel", addedReel.Name);

        var getResponse = await _httpClient.GetAsync($"users/me/reels/{addedReel.Id}");
        getResponse.EnsureSuccessStatusCode();

        var retrievedReel = await getResponse.Content.ReadFromJsonAsync<ReelResponse>();
        Assert.NotNull(retrievedReel);
        Assert.Equal(addedReel.Id, retrievedReel.Id);
        Assert.Equal("Test Reel", retrievedReel.Name);
    }

    [Fact]
    public async Task AddUpdateAndGetReel_ShouldReturnUpdatedReel()
    {
        var token = await GetJwtTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a workspace
        var workspaceRequest = new { name = "Test Workspace" };
        var workspaceResponse = await _httpClient.PostAsJsonAsync("users/me/workspaces", workspaceRequest);
        workspaceResponse.EnsureSuccessStatusCode();
        var workspace = await workspaceResponse.Content.ReadFromJsonAsync<WorkspaceResponse>();
        Assert.NotNull(workspace);

        var addRequest = new AddReelRequest { Name = "Test Reel", Description = "Test Description" };
        var addResponse = await _httpClient.PostAsJsonAsync($"users/me/workspaces/{workspace.Id}/reels", addRequest);
        addResponse.EnsureSuccessStatusCode();

        var addedReel = await addResponse.Content.ReadFromJsonAsync<ReelResponse>();
        Assert.NotNull(addedReel);
        Assert.Equal("Test Reel", addedReel.Name);

        var updateRequest = new UpdateReelRequest { Name = "Updated Reel", Description = "Updated Description" };
        var updateResponse = await _httpClient.PutAsJsonAsync($"users/me/reels/{addedReel.Id}", updateRequest);
        updateResponse.EnsureSuccessStatusCode();

        var getResponse = await _httpClient.GetAsync($"users/me/reels/{addedReel.Id}");
        getResponse.EnsureSuccessStatusCode();

        var updatedReel = await getResponse.Content.ReadFromJsonAsync<ReelResponse>();
        Assert.NotNull(updatedReel);
        Assert.Equal(addedReel.Id, updatedReel.Id);
        Assert.Equal("Updated Reel", updatedReel.Name);
    }

    [Fact]
    public async Task AddAndDeleteReel_ShouldReturnNotFound()
    {
        var token = await GetJwtTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a workspace
        var workspaceRequest = new { name = "Test Workspace" };
        var workspaceResponse = await _httpClient.PostAsJsonAsync("users/me/workspaces", workspaceRequest);
        workspaceResponse.EnsureSuccessStatusCode();
        var workspace = await workspaceResponse.Content.ReadFromJsonAsync<WorkspaceResponse>();
        Assert.NotNull(workspace);

        var addRequest = new AddReelRequest { Name = "Test Reel", Description = "Test Description" };
        var addResponse = await _httpClient.PostAsJsonAsync($"users/me/workspaces/{workspace.Id}/reels", addRequest);
        addResponse.EnsureSuccessStatusCode();

        var addedReel = await addResponse.Content.ReadFromJsonAsync<ReelResponse>();
        Assert.NotNull(addedReel);
        Assert.Equal("Test Reel", addedReel.Name);

        var deleteResponse = await _httpClient.DeleteAsync($"users/me/reels/{addedReel.Id}");
        deleteResponse.EnsureSuccessStatusCode();

        var getResponse = await _httpClient.GetAsync($"users/me/reels/{addedReel.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetReels_ShouldReturnReels()
    {
        var token = await GetJwtTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a workspace
        var workspaceRequest = new { name = "Test Workspace" };
        var workspaceResponse = await _httpClient.PostAsJsonAsync("users/me/workspaces", workspaceRequest);
        workspaceResponse.EnsureSuccessStatusCode();
        var workspace = await workspaceResponse.Content.ReadFromJsonAsync<WorkspaceResponse>();
        Assert.NotNull(workspace);

        var addRequest = new AddReelRequest { Name = "Test Reel", Description = "Test Description" };
        var addResponse = await _httpClient.PostAsJsonAsync($"users/me/workspaces/{workspace.Id}/reels", addRequest);
        var addedReel = await addResponse.Content.ReadFromJsonAsync<ReelResponse>();

        var getResponse = await _httpClient.GetAsync($"users/me/workspaces/{workspace.Id}/reels");
        getResponse.EnsureSuccessStatusCode();

        var reels = await getResponse.Content.ReadFromJsonAsync<ReelsResponse>();
        Assert.NotNull(reels);
        
        Assert.NotNull(reels.Items);
        Assert.NotEmpty(reels.Items);
        Assert.NotEmpty(reels.Items);
        reels.Items.Should().ContainSingle().Which.Should().BeEquivalentTo(addedReel);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _mongoDbRunner.Dispose();
    }
}