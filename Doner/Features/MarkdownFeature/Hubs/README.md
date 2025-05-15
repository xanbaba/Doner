# MarkdownHub Documentation

## Table of Contents

1. [Introduction](#introduction)
2. [Authentication](#authentication)
3. [Error Handling](#error-handling)
4. [Client Methods](#client-methods)
    - [JoinDocument](#joindocument)
    - [LeaveDocument](#leavedocument)
    - [SendOperation](#sendoperation)
    - [RequestSync](#requestsync)
    - [UpdateCursorPosition](#updatecursorposition)
    - [StartTyping](#starttyping)
    - [StopTyping](#stoptyping)
5. [Server Events](#server-events)
    - [DocumentState](#documentstate)
    - [ActiveUsers](#activeusers)
    - [UserJoined](#userjoined)
    - [UserLeft](#userleft)
    - [ReceiveOperation](#receiveoperation)
    - [SyncRequired](#syncrequired)
    - [CursorPositionChanged](#cursorpositionchanged)
    - [UserIsTyping](#useristyping)
6. [Data Models](#data-models)
7. [Locking Mechanism](#locking-mechanism)
8. [JavaScript Example](#javascript-example)

## Introduction

The MarkdownHub is a SignalR hub that facilitates real-time collaborative editing of markdown documents. It provides
functionality for document joining/leaving, operational transformation for conflict-free editing, cursor position
tracking, and typing indicators.

## Authentication

The MarkdownHub requires authentication. It uses ASP.NET Core's built-in authentication system and expects a valid JWT
token to be passed with each request.

To connect to the hub, you must include an Authorization header with a valid JWT token:

``` javascript
const connection = new signalR.HubConnectionBuilder()
.withUrl("/hubs/markdown", {
accessTokenFactory: () => localStorage.getItem("accessToken")
})
.build();
```

The token must include a `nameidentifier` claim that contains the user's ID (GUID).

## Error Handling

The hub uses a standardized error format for all exceptions. When an error occurs, the hub throws a `HubException`
containing a serialized JSON object with the following structure:

``` json
{
"Code": 400,
"Message": "Error message describing the problem"
}
```

### Error Codes

- `400` - Bad Request: Invalid parameters or state
- `403` - Forbidden: User doesn't have permission to access the document
- `404` - Not Found: Document not found
- `500` - Internal Server Error: Unexpected server error
- `503` - Service Unavailable: Document is currently busy (lock timeout)

The client application can handle these error codes appropriately, such as displaying a permission denied dialog for 403
errors, document not found messages for 404 errors, or retry options for 503 errors.

## Client Methods

These methods can be called from the client to interact with the hub.

### JoinDocument

Joins a collaborative editing session for a specific document.

**Parameters:**

- `documentId` (string, required): The unique identifier of the document to join.

**Behavior:**

1. Verifies that the document exists and the user has permission to access it.
2. Checks if user is the document owner OR has access through workspace membership.
3. Retrieves document content and version.
4. Associates the connection with the document.
5. Adds the connection to the document's SignalR group.
6. Returns document state and active users list.
7. Notifies other users that a new user has joined.

**Possible Errors:**

- `404`: Document with the specified ID doesn't exist.
- `403`: User doesn't have permission to access the document.
- `500`: An error occurred while joining the document.

### LeaveDocument

Leaves the current document editing session.

**Parameters:** None

**Behavior:**

1. Removes connection tracking for the current document.
2. Removes the connection from the document's SignalR group.
3. Notifies other users that the user has left.

**Possible Errors:**

- `500`: An error occurred while leaving the document.

### SendOperation

Sends an editing operation to be applied to the document using Operational Transformation.

**Parameters:**

- `request` (OperationRequest, required): The operation to apply.
    - `OperationId` (Guid, required): Unique identifier for this operation.
    - `BaseVersion` (int, required): The document version this operation is based on.
    - `Components` (Array of ComponentDto, required): List of operation components.

**Behavior:**

1. Acquires a distributed lock on the document to prevent conflicts (30-second timeout).
2. Processes the operation using operational transformation if needed.
3. Applies the operation to the document.
4. Broadcasts the operation to other users editing the document.

**Possible Errors:**

- `400`: Not joined to a document.
- `404`: Document not found.
- `503`: The document is currently busy (lock acquisition timeout).
- `500`: Error processing the operation.

### RequestSync

Requests synchronization of document state based on client version.

**Parameters:**

- `clientVersion` (int, required): The client's current document version.

**Behavior:**

1. If the client is not too far behind (<100 operations), sends only the missing operations.
2. If the client is significantly behind (>100 operations), sends the complete document state.
3. If no document connection exists, sends a SyncRequired event with version 0.

**Possible Errors:**

- `404`: Document no longer exists.
- `500`: Error syncing client.

### UpdateCursorPosition

Updates the user's cursor position and selection in the document.

**Parameters:**

- `position` (CursorPositionRequest, required):
    - `Position` (int, required): Current cursor position (character index).
    - `HasSelection` (bool, required): Whether text is selected.
    - `SelectionStart` (int, optional): Start index of selection (if HasSelection is true).
    - `SelectionEnd` (int, optional): End index of selection (if HasSelection is true).

**Behavior:**
Broadcasts the user's cursor position to other users editing the document.

**Possible Errors:**
No exceptions thrown, errors are silently logged.

### StartTyping

Indicates that the user has started typing.

**Parameters:** None

**Behavior:**
Updates the user's typing status and broadcasts it to other users in the document.

**Possible Errors:**
No exceptions thrown, errors are silently logged.

### StopTyping

Indicates that the user has stopped typing.

**Parameters:** None

**Behavior:**
Updates the user's typing status and broadcasts it to other users in the document.

**Possible Errors:**

- `500`: Failed to update typing status.

## Server Events

These events are sent from the server to clients.

### DocumentState

Sent when a client joins a document or needs to be synchronized with the current document state.

**Parameters:**

``` json
{
"Content": "Full document content as a string",
"Version": 42
}
```

### ActiveUsers

Sent to a client when joining a document, containing all currently active users.

**Parameters:**

``` json
[
{
"UserId": "user-guid-1",
"DisplayName": "John Doe",
"Color": "#FF5733",
"IsTyping": false
},
{
"UserId": "user-guid-2",
"DisplayName": "Jane Smith",
"Color": "#3366FF",
"IsTyping": true
}
]
```

### UserJoined

Sent to all users in a document when a new user joins.

**Parameters:**

- `userInfo` (UserInfoResponse): Information about the user who joined.

``` json
{
"UserId": "user-guid",
"DisplayName": "John Doe",
"Color": "#FF5733",
"IsTyping": false
}
```

### UserLeft

Sent to all users in a document when a user leaves.

**Parameters:**

- `userId` (string): GUID of the user who left.

### ReceiveOperation

Sent to clients when an operation needs to be applied to their document.

**Parameters:**

``` json
{
"OperationId": "operation-guid",
"Components": [
{ "Type": 0, "Count": 5 },           // Retain 5 characters
{ "Type": 1, "Text": "Hello" },      // Insert "Hello"
{ "Type": 2, "Count": 3 }            // Delete 3 characters
],
"BaseVersion": 42,
"UserId": "user-guid"
}
```

### SyncRequired

Sent to a client when they need to request synchronization.

**Parameters:**

- `version` (int): The version to sync from (usually 0).

### CursorPositionChanged

Sent to all users when a user's cursor position changes.

**Parameters:**

- `userId` (string): GUID of the user whose cursor moved.
- `position` (CursorPositionResponse):

``` json
{
"Position": 42,
"HasSelection": true,
"SelectionStart": 42,
"SelectionEnd": 47
}
```

### UserIsTyping

Sent to all users when a user starts or stops typing.

**Parameters:**

- `userId` (string): GUID of the user.
- `isTyping` (boolean): Whether the user is currently typing.

## Data Models

### UserInfo

Internal class used to track user information during document collaboration:

``` csharp
public class UserInfo
{
public Guid UserId { get; set; }
public string DisplayName { get; set; } = null!;
public string Email { get; set; } = null!;
public string Color { get; set; } = "#" + new Random().Next(0x808080, 0xFFFFFF).ToString("X6");
public bool IsTyping { get; set; }
public DateTime ConnectedAt { get; set; }
}
```

### ComponentType Enum

- `0` - Retain: Keep specified number of characters
- `1` - Insert: Insert specified text
- `2` - Delete: Delete specified number of characters

### ComponentDto

``` json
{
"Type": 0,           // ComponentType enum value
"Count": 5,          // Required for Retain (0) and Delete (2) types
"Text": "Hello"      // Required for Insert (1) type
}
```

Note: Either `Count` or `Text` must be provided depending on the `Type` value:

- For `Type = 0` (Retain) or `Type = 2` (Delete): `Count` is required
- For `Type = 1` (Insert): `Text` is required

### OperationRequest

``` json
{
    "OperationId": "operation-guid",
    "BaseVersion": 42,
    "Components": [
        { "Type": 0, "Count": 5 },           // Retain 5 characters
        { "Type": 1, "Text": "Hello" },      // Insert "Hello"
        { "Type": 2, "Count": 3 }            // Delete 3 characters
    ]
}
```

### CursorPositionRequest

``` json
{
    "Position": 42,
    "HasSelection": true,
    "SelectionStart": 42,
    "SelectionEnd": 47
}
```

### UserInfoResponse

``` json
{
    "UserId": "user-guid",
    "DisplayName": "John Doe",
    "Color": "#FF5733",
    "IsTyping": false
}
```

## Locking Mechanism

The MarkdownHub uses a distributed locking system to prevent conflicts when multiple users edit the same document
concurrently. The system uses a single timeout value:

- Operation Lock: 30 seconds

If a lock cannot be acquired within the timeout period, a `503 Service Unavailable` error is returned.

## JavaScript Example

Here's a complete example of how to use the MarkdownHub in a JavaScript client application:

``` javascript
// Create the connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/markdown", {
        accessTokenFactory: () => localStorage.getItem("accessToken")
    })
    .withAutomaticReconnect()
    .build();

// Track local state
let documentContent = "";
let documentVersion = 0;
let activeUsers = [];
let isConnected = false;
let currentDocumentId = null;

// Set up event handlers
connection.on("DocumentState", (state) => {
    documentContent = state.Content;
    documentVersion = state.Version;
    console.log(`Received document state: version ${documentVersion}`);

    // Update UI with the new content
    editor.setValue(documentContent);
});

connection.on("ActiveUsers", (users) => {
    activeUsers = users;
    console.log(`Active users: ${users.length}`);

    // Update UI to show active users
    updateActiveUsersList(users);
});

connection.on("UserJoined", (userInfo) => {
    console.log(`User joined: ${userInfo.DisplayName}`);
    activeUsers.push(userInfo);

    // Update active users UI
    updateActiveUsersList(activeUsers);

    // Show notification
    showNotification(`${userInfo.DisplayName} joined the document`);
});

connection.on("UserLeft", (userId) => {
    console.log(`User left: ${userId}`);
    const userWhoLeft = activeUsers.find(u => u.UserId === userId);
    activeUsers = activeUsers.filter(u => u.UserId !== userId);

    // Update active users UI
    updateActiveUsersList(activeUsers);

    // Use the display name if we have it
    const displayName = userWhoLeft ? userWhoLeft.DisplayName : "A user";
    showNotification(`${displayName} left the document`);
});

connection.on("ReceiveOperation", (operation) => {
    console.log(`Received operation: ${operation.OperationId}`);

    // Apply the operation to the local document
    applyOperation(operation.Components);
    documentVersion = operation.BaseVersion + 1;
});

connection.on("SyncRequired", (version) => {
    console.log(`Sync required from version ${version}`);

    // Request sync
    connection.invoke("RequestSync", version || 0)
        .catch(err => console.error("Error requesting sync:", err));
});

connection.on("CursorPositionChanged", (userId, position) => {
    console.log(`User ${userId} cursor position changed`);

    // Update remote cursor in the editor
    updateRemoteCursor(userId, position);
});

connection.on("UserIsTyping", (userId, isTyping) => {
    console.log(`User ${userId} is ${isTyping ? "typing" : "not typing"}`);

    // Update typing indicator in UI
    updateTypingIndicator(userId, isTyping);
});

// Handle connection events
connection.onreconnecting(error => {
    isConnected = false;
    console.log("Connection lost, reconnecting...");
    showConnectionStatus("Reconnecting...");
});

connection.onreconnected(connectionId => {
    isConnected = true;
    console.log("Reconnected, ID:", connectionId);
    showConnectionStatus("Connected");

    // Rejoin the document after reconnection
    if (currentDocumentId) {
        joinDocument(currentDocumentId);
    }
});

connection.onclose(error => {
    isConnected = false;
    console.log("Connection closed");
    showConnectionStatus("Disconnected");
});

// Start the connection
async function start() {
    try {
        await connection.start();
        isConnected = true;
        console.log("Connected to SignalR hub");
        showConnectionStatus("Connected");
    } catch (err) {
        console.error("Error connecting:", err);
        showConnectionStatus("Connection failed");

        // Retry after 5 seconds
        setTimeout(start, 5000);
    }
}

// Join a document
async function joinDocument(documentId) {
    if (!isConnected) {
        console.error("Not connected to hub");
        return;
    }

    try {
        currentDocumentId = documentId;
        await connection.invoke("JoinDocument", documentId);
        console.log(`Joined document ${documentId}`);
    } catch (err) {
        handleHubError(err);
    }
}

// Leave the current document
async function leaveDocument() {
    if (!isConnected || !currentDocumentId) {
        return;
    }

    try {
        await connection.invoke("LeaveDocument");
        console.log("Left document");
        currentDocumentId = null;
    } catch (err) {
        handleHubError(err);
    }
}

// Send an operation
async function sendOperation(components, baseVersion) {
    if (!isConnected || !currentDocumentId) {
        console.error("Not connected or no document");
        return;
    }

    try {
        const operationId = generateUuid();
        await connection.invoke("SendOperation", {
            OperationId: operationId
            , BaseVersion: baseVersion
            , Components: components
        });
        console.log(`Sent operation ${operationId}`);
    } catch (err) {
        handleHubError(err);
    }
}

// Update cursor position
async function updateCursorPosition(position, hasSelection, selectionStart, selectionEnd) {
    if (!isConnected || !currentDocumentId) {
        return;
    }

    try {
        await connection.invoke("UpdateCursorPosition", {
            Position: position
            , HasSelection: hasSelection
            , SelectionStart: selectionStart
            , SelectionEnd: selectionEnd
        });
    } catch (err) {
        console.error("Error updating cursor position:", err);
    }
}

// Start typing indicator
async function startTyping() {
    if (!isConnected || !currentDocumentId) {
        return;
    }

    try {
        await connection.invoke("StartTyping");
    } catch (err) {
        console.error("Error setting typing status:", err);
    }
}

// Stop typing indicator
async function stopTyping() {
    if (!isConnected || !currentDocumentId) {
        return;
    }

    try {
        await connection.invoke("StopTyping");
    } catch (err) {
        console.error("Error setting typing status:", err);
    }
}

// Handle hub errors
function handleHubError(err) {
    console.error("Hub error:", err);

    try {
        // Try to parse the error message
        const errorData = JSON.parse(err.message);

        if (errorData.Code && errorData.Message) {
            console.error(`Error ${errorData.Code}: ${errorData.Message}`);

            // Show appropriate UI notification
            showError(`Error ${errorData.Code}: ${errorData.Message}`);

            // Handle specific error codes
            switch (errorData.Code) {
                case 403:
                    // Permission denied
                    showPermissionDeniedDialog();
                    break;
                case 404:
                    // Document not found
                    showDocumentNotFoundDialog();
                    break;
                case 503:
                    // Document busy
                    showDocumentBusyDialog(() => {
                        // Retry function
                    });
                    break;
            }
        } else {
            showError("An error occurred");
        }
    } catch (e) {
        // Not a JSON error
        showError(err.message || "An error occurred");
    }
}

// Start the connection
start();

// Clean up when page unloads
window.addEventListener("beforeunload", async () => {
    if (currentDocumentId) {
        await leaveDocument();
    }

    await connection.stop();
});

// Helper function to generate UUIDs
function generateUuid() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
        const r = Math.random() * 16 | 0;
        const v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}
```
