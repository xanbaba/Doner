using System.Security.Claims;
using Doner.Features.MarkdownFeature.Hubs.Models;
using Doner.Features.MarkdownFeature.OT;
using Doner.Features.MarkdownFeature.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Doner.Features.MarkdownFeature.Hubs;

[Authorize]
public class MarkdownHub : Hub
{
    private readonly IConnectionTracker _connectionTracker;
    private readonly IOTService _otService;
    private readonly IMarkdownRepository _markdownRepository;
    private readonly IOperationRepository _operationRepository;
    private readonly ILogger<MarkdownHub> _logger;

    public MarkdownHub(
        IConnectionTracker connectionTracker,
        IOTService otService,
        IMarkdownRepository markdownRepository,
        IOperationRepository operationRepository,
        ILogger<MarkdownHub> logger)
    {
        _connectionTracker = connectionTracker;
        _otService = otService;
        _markdownRepository = markdownRepository;
        _operationRepository = operationRepository;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            // Get document this connection was editing before removing
            var documentId = await _connectionTracker.GetDocumentForConnectionAsync(Context.ConnectionId);
            
            if (!string.IsNullOrEmpty(documentId))
            {
                // Remove from tracking
                await _connectionTracker.RemoveConnectionAsync(Context.ConnectionId);
                
                var userId = Context.User!.GetUserId();
                
                // Notify other users that this user has left
                await Clients.Group(GetDocumentGroupName(documentId))
                    .SendAsync("UserLeft", userId.ToString());
                
                _logger.LogInformation("User {UserId} disconnected from document {DocumentId}", userId, documentId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling disconnection for {ConnectionId}", Context.ConnectionId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
    
    // Document Session Management
    
    public async Task JoinDocument(string documentId)
    {
        try
        {
            // TODO: Add authorization check to make sure user has access to this document
            
            // Get document content and version first to check if document exists
            var documentState = await _markdownRepository.GetDocumentStateAsync(documentId);
            
            if (documentState == null)
            {
                // Document doesn't exist, notify the client
                await Clients.Caller.SendAsync("OperationError", 
                    Guid.NewGuid().ToString(),
                    $"Document with ID {documentId} does not exist.");
                _logger.LogWarning("User attempted to join non-existent document {DocumentId}", documentId);
                return;
            }
            
            var userId = Context.User!.GetUserId();
            var userInfo = new UserInfo
            {
                UserId = userId,
                DisplayName = Context.User!.Identity?.Name ?? "Unknown User",
                Email = Context.User.FindFirstValue(ClaimTypes.Email),
                ConnectedAt = DateTime.UtcNow
            };
            
            // Associate this connection with the document
            await _connectionTracker.TrackConnectionAsync(Context.ConnectionId, documentId, userId);
            
            // Add to SignalR group for this document
            await Groups.AddToGroupAsync(Context.ConnectionId, GetDocumentGroupName(documentId));
            
            // Get all active users for this document
            var connections = await _connectionTracker.GetConnectionsForDocumentAsync(documentId);
            var activeUsers = new List<UserInfoResponse>();
            
            foreach (var connectionId in connections)
            {
                var info = await _connectionTracker.GetUserInfoAsync(connectionId);
                if (info != null)
                {
                    activeUsers.Add(new UserInfoResponse
                    {
                        UserId = info.UserId.ToString(),
                        DisplayName = info.DisplayName,
                        Color = info.Color,
                        IsTyping = info.IsTyping
                    });
                }
            }
            
            // Send current document state to the joining user
            await Clients.Caller.SendAsync("DocumentState", 
                new DocumentStateResponse 
                { 
                    Content = documentState.Content, 
                    Version = documentState.Version 
                });
            
            // Send active users list to the joining user
            await Clients.Caller.SendAsync("ActiveUsers", activeUsers);
            
            // Notify other users that this user has joined
            await Clients.OthersInGroup(GetDocumentGroupName(documentId)).SendAsync("UserJoined", 
                userId.ToString(),
                new UserInfoResponse
                {
                    UserId = userInfo.UserId.ToString(),
                    DisplayName = userInfo.DisplayName,
                    Color = userInfo.Color,
                    IsTyping = false
                });
            
            _logger.LogInformation("User {UserId} joined document {DocumentId}", userId, documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining document {DocumentId}", documentId);
            throw;
        }
    }
    
    public async Task LeaveDocument()
    {
        try
        {
            var documentId = await _connectionTracker.GetDocumentForConnectionAsync(Context.ConnectionId);
            
            if (string.IsNullOrEmpty(documentId))
                return;
                
            var userId = Context.User!.GetUserId();
            
            // Remove connection from tracking
            await _connectionTracker.RemoveConnectionAsync(Context.ConnectionId);
            
            // Remove from the SignalR group
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetDocumentGroupName(documentId));
            
            // Notify others that this user has left
            await Clients.OthersInGroup(GetDocumentGroupName(documentId))
                .SendAsync("UserLeft", userId.ToString());
                
            _logger.LogInformation("User {UserId} left document {DocumentId}", userId, documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving document");
            throw;
        }
    }
    
    // Operational Transformation
    
    public async Task SendOperation(OperationRequest request)
    {
        try
        {
            var documentId = await _connectionTracker.GetDocumentForConnectionAsync(Context.ConnectionId);
            
            if (string.IsNullOrEmpty(documentId))
            {
                await Clients.Caller.SendAsync("OperationError", 
                    request.OperationId,
                    "No active document for this connection. Join a document first.");
                return;
            }
            
            var userId = Context.User!.GetUserId();
            
            // Convert from DTO to domain model
            var operation = new Operation
            {
                Id = request.OperationId,
                MarkdownId = documentId,
                UserId = userId,
                BaseVersion = request.BaseVersion,
                Components = request.Components.Select(MapToOperationComponent).ToList(),
                Timestamp = DateTime.UtcNow
            };
            
            // Process the operation (apply OT, persist, etc.)
            var processedOperation = await _otService.ProcessOperationAsync(operation);
            
            if (processedOperation == null)
            {
                await Clients.Caller.SendAsync("OperationError", 
                    request.OperationId, 
                    "Operation could not be processed. Try syncing first.");
                return;
            }
            
            // Convert back to DTO for response
            var response = new OperationResponse
            {
                OperationId = processedOperation.Id.ToString(),
                Components = processedOperation.Components.Select(MapToComponentDto).ToList(),
                BaseVersion = processedOperation.BaseVersion,
                UserId = processedOperation.UserId.ToString()
            };
            
            // Broadcast to all other users in this document
            await Clients.OthersInGroup(GetDocumentGroupName(documentId))
                .SendAsync("ReceiveOperation", response);
                
            _logger.LogInformation("Operation {OperationId} processed for document {DocumentId}", 
                request.OperationId, documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing operation {OperationId}", request.OperationId);
            await Clients.Caller.SendAsync("OperationError", 
                request.OperationId, 
                "An unexpected error occurred while processing the operation.");
        }
    }
    
    public async Task RequestSync(int clientVersion)
    {
        try
        {
            var documentId = await _connectionTracker.GetDocumentForConnectionAsync(Context.ConnectionId);
            
            if (string.IsNullOrEmpty(documentId))
            {
                await Clients.Caller.SendAsync("SyncRequired", 0);
                return;
            }
            
            // Get operations needed to sync client
            var operations = await _operationRepository.GetOperationsAsync(documentId, clientVersion);
            
            // If there are no operations to sync, or if the client is too far behind,
            // send the complete document state instead
            var operationsArray = operations as Operation[] ?? operations.ToArray();
            if (operationsArray.Length is 0 or > 100) // Arbitrary threshold
            {
                var documentState = await _markdownRepository.GetDocumentStateAsync(documentId);
                
                if (documentState == null)
                {
                    // Document may have been deleted
                    await Clients.Caller.SendAsync("OperationError", 
                        Guid.NewGuid().ToString(),
                        $"Document with ID {documentId} no longer exists.");
                    return;
                }
                
                await Clients.Caller.SendAsync("DocumentState", 
                    new DocumentStateResponse 
                    { 
                        Content = documentState.Content, 
                        Version = documentState.Version 
                    });
                return;
            }
            
            // Send operations in order
            foreach (var operation in operationsArray)
            {
                await Clients.Caller.SendAsync("ReceiveOperation", new OperationResponse
                {
                    OperationId = operation.Id.ToString(),
                    Components = operation.Components.Select(MapToComponentDto).ToList(),
                    BaseVersion = operation.BaseVersion,
                    UserId = operation.UserId.ToString()
                });
            }
            
            _logger.LogInformation("Sent {OperationCount} operations to sync client to version {ClientVersion} for document {DocumentId}",
                operationsArray.Count(), operationsArray.Last().BaseVersion + 1, documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing client to version {ClientVersion}", clientVersion);
            throw;
        }
    }
    
    // User Interaction
    
    public async Task UpdateCursorPosition(CursorPositionRequest position)
    {
        try
        {
            var documentId = await _connectionTracker.GetDocumentForConnectionAsync(Context.ConnectionId);
            
            if (string.IsNullOrEmpty(documentId))
                return;
                
            var userId = Context.User!.GetUserId();
            
            // Broadcast cursor position to other users in this document
            await Clients.OthersInGroup(GetDocumentGroupName(documentId))
                .SendAsync("CursorPositionChanged", 
                    userId.ToString(), 
                    new CursorPositionResponse
                    {
                        Position = position.Position,
                        HasSelection = position.HasSelection,
                        SelectionStart = position.SelectionStart,
                        SelectionEnd = position.SelectionEnd
                    });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cursor position");
        }
    }
    
    public async Task StartTyping()
    {
        try
        {
            var documentId = await _connectionTracker.GetDocumentForConnectionAsync(Context.ConnectionId);
            
            if (string.IsNullOrEmpty(documentId))
                return;
                
            var userId = Context.User!.GetUserId();
            var userInfo = await _connectionTracker.GetUserInfoAsync(Context.ConnectionId);
            
            if (userInfo != null)
            {
                userInfo.IsTyping = true;
                
                // Broadcast typing status to other users in this document
                await Clients.OthersInGroup(GetDocumentGroupName(documentId))
                    .SendAsync("UserIsTyping", userId.ToString(), true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting typing indicator");
        }
    }
    
    public async Task StopTyping()
    {
        try
        {
            var documentId = await _connectionTracker.GetDocumentForConnectionAsync(Context.ConnectionId);
            
            if (string.IsNullOrEmpty(documentId))
                return;
                
            var userId = Context.User!.GetUserId();
            var userInfo = await _connectionTracker.GetUserInfoAsync(Context.ConnectionId);
            
            if (userInfo != null)
            {
                userInfo.IsTyping = false;
                
                // Broadcast typing status to other users in this document
                await Clients.OthersInGroup(GetDocumentGroupName(documentId))
                    .SendAsync("UserIsTyping", userId.ToString(), false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping typing indicator");
        }
    }
    
    // Helper methods
    
    private static string GetDocumentGroupName(string documentId) => $"document:{documentId}";
    
    private static OperationComponent MapToOperationComponent(ComponentDto component)
    {
        return component.Type switch
        {
            ComponentType.Retain => new RetainComponent { Count = component.Count ?? 0 },
            ComponentType.Insert => new InsertComponent { Text = component.Text ?? string.Empty },
            ComponentType.Delete => new DeleteComponent { Count = component.Count ?? 0 },
            _ => throw new ArgumentException($"Unknown component type: {component.Type}")
        };
    }
    
    private static ComponentDto MapToComponentDto(OperationComponent component)
    {
        return component switch
        {
            RetainComponent retain => new ComponentDto { Type = ComponentType.Retain, Count = retain.Count },
            InsertComponent insert => new ComponentDto { Type = ComponentType.Insert, Text = insert.Text },
            DeleteComponent delete => new ComponentDto { Type = ComponentType.Delete, Count = delete.Count },
            _ => throw new ArgumentException($"Unknown component type: {component.GetType().Name}")
        };
    }
}

// Extension method to get user ID from claims
public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null)
            throw new InvalidOperationException("User ID claim not found");
            
        return Guid.Parse(claim.Value);
    }
}
