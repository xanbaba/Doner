# MarkdownHub Documentation
The MarkdownHub is a SignalR Hub that provides real-time collaborative editing capabilities for markdown documents. It enables multiple users to simultaneously edit, view, and interact within a shared markdown document.
## Authentication
The MarkdownHub requires authentication. Users must include a valid JWT token with their connection.
### Connection Authentication
To connect to the hub, include your authentication token in the connection:
``` javascript
// JavaScript client example
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/markdown", { 
        accessTokenFactory: () => localStorage.getItem("auth_token") 
    })
    .build();
```
## Error Handling
The hub uses a standardized error format for all error responses:
``` json
{
  "Code": 404,
  "Message": "Document not found"
}
```
### Error Codes
- `400` - Bad Request: Invalid parameters or state
- `401` - Unauthorized: Not authenticated or insufficient permissions
- `404` - Not Found: Resource (document) not found
- `500` - Internal Server Error: Unexpected server error

## Client Methods
These methods can be called by clients connected to the hub:
### `JoinDocument(string documentId)`
Joins a collaborative editing session for a specific document.
**Parameters:**
- (string, required): The unique identifier of the document to join. `documentId`

**Behavior:**
1. Verifies that the document exists and the user has permission to access it.
2. Adds the user to the document's SignalR group.
3. Returns the current document state, including content and version.
4. Notifies other users of the new participant.

**Possible Errors:**
- `404`: Document with the specified ID doesn't exist.
- `401`: User doesn't have permission to access the document.

### `LeaveDocument()`
Leaves the current document editing session.
**Parameters:** None
**Behavior:**
1. Removes the user from the document's SignalR group.
2. Notifies other users that the user has left.

### `SendOperation(OperationRequest operation)`
Sends an editing operation to be applied to the document.
**Parameters:**
- (OperationRequest, required): The operation to apply.
    - (Guid, required): Unique identifier for this operation. `OperationId`
    - (int, required): The document version this operation is based on. `BaseVersion`
    - (List , required): List of operation components (retain, insert, delete).  `Components`

`operation`

**Behavior:**
1. Processes the operation using operational transformation if needed.
2. Applies the operation to the document.
3. Broadcasts the operation to other users editing the document.

**Possible Errors:**
- `400`: Not joined to a document or invalid operation format.
- `404`: Document not found.
- `500`: Error processing the operation.

### `RequestSync(int clientVersion)`
Requests synchronization of document state based on client version.
**Parameters:**
- (int, required): The client's current document version. `clientVersion`

**Behavior:**
1. If the client is not too far behind, sends only the missing operations.
2. If the client is significantly behind (>100 operations), sends the complete document state.

**Possible Errors:**
- `404`: Document no longer exists.

### `UpdateCursorPosition(CursorPositionRequest position)`
Updates the user's cursor position and selection state.
**Parameters:**
- (CursorPositionRequest, required): The cursor position information.
    - (int, required): Cursor position in the document. `Position`
    - (bool, required): Whether text is selected. `HasSelection`
    - (int, nullable): Start of selection, if any. `SelectionStart`
    - (int, nullable): End of selection, if any. `SelectionEnd`

`position`

**Behavior:** Broadcasts the cursor position to other users in the document.
### `StartTyping()`
Indicates that the user has started typing.
**Parameters:** None
**Behavior:** Broadcasts typing status to other users in the document.
### `StopTyping()`
Indicates that the user has stopped typing.
**Parameters:** None
**Behavior:** Broadcasts typing status to other users in the document.
## Server Events
These events are sent from the server to clients:
### `DocumentState`
Provides the complete state of a document.
**Parameters:**
- `state` (DocumentStateResponse): The document's current state.
    - (string): Full content of the document. `Content`
    - (int): Current version number of the document. `Version`

**Triggered By:**
- Joining a document
- Requesting sync when client is too far behind

### `ActiveUsers`
Provides a list of all users currently editing the document.
**Parameters:**
- `users` (IEnumerable ): Array of user information objects.
    - (string): Unique identifier for the user. `UserId`
    - (string): User's display name. `DisplayName`
    - (string): Color assigned to the user for UI highlighting. `Color`
    - (bool): Whether the user is currently typing. `IsTyping`

**Triggered By:**
- Joining a document

### `UserJoined`
Notifies when a new user joins the document session.
**Parameters:**
- (string): The ID of the user who joined. `userId`
- (UserInfoResponse): Information about the joining user.
    - (string): Unique identifier for the user. `UserId`
    - (string): User's display name. `DisplayName`
    - (string): Color assigned to the user for UI highlighting. `Color`
    - (bool): Always false initially. `IsTyping`

`userInfo`

**Triggered By:**
- A user joining the document.

### `UserLeft`
Notifies when a user leaves the document session.
**Parameters:**
- (string): The ID of the user who left. `userId`

**Triggered By:**
- A user explicitly leaving the document
- A user disconnecting (connection closed)

### `ReceiveOperation`
Delivers an operation to be applied to the document.
**Parameters:**
- (OperationResponse): The operation to apply.
    - (string): Unique identifier for this operation. `OperationId`
    - (List ): List of operation components.  `Components`
    - (int): The document version this operation is based on. `BaseVersion`
    - (string): The ID of the user who made the change. `UserId`

`operation`

**Component Types:**
- : Keep a number of characters unchanged.
    - Contains property specifying how many characters to retain. `Count`

`Retain`
- : Insert text at the current position.
    - Contains property with the text to insert. `Text`

`Insert`
- : Delete a number of characters at the current position.
    - Contains property specifying how many characters to delete. `Count`

`Delete`

**Triggered By:**
- Another user sending an operation
- During synchronization to catch up client state

### `CursorPositionChanged`
Notifies of a user's cursor position change.
**Parameters:**
- (string): The ID of the user whose cursor moved. `userId`
- (CursorPositionResponse): The cursor's new position.
    - (int): The character position in the document. `Position`
    - (bool): Whether the user has text selected. `HasSelection`
    - (int, nullable): Start of selection, if any. `SelectionStart`
    - (int, nullable): End of selection, if any. `SelectionEnd`

`position`

**Triggered By:**
- Another user moving their cursor or changing selection.

### `UserIsTyping`
Indicates whether a user is currently typing.
**Parameters:**
- (string): The ID of the user whose typing status changed. `userId`
- `isTyping` (bool): Whether the user is typing (true) or stopped typing (false).

**Triggered By:**
- Another user starting or stopping typing.

### `SyncRequired`
Indicates that the client needs to synchronize with the server.
**Parameters:**
- `serverVersion` (int): The current version on the server.

**Triggered By:**
- The client attempting to join a non-existent document connection.

## Data Models
### Operation Components
Operations are composed of three component types:
1. : Preserves characters (moves cursor) **Retain**
    - Type: `ComponentType.Retain`
    - Count: Number of characters to retain

2. : Adds new text **Insert**
    - Type: `ComponentType.Insert`
    - Text: The text to insert

3. : Removes characters **Delete**
    - Type: `ComponentType.Delete`
    - Count: Number of characters to delete

Together these components form a complete operation that can transform a document from one state to another.

## JavaScript Client Example

Here's a complete JavaScript example demonstrating how to use the MarkdownHub:

```javascript
// Set up the SignalR connection
const setupMarkdownCollaboration = (documentId) => {
    // 1. Create and configure the connection
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/markdown", {
            accessTokenFactory: () => localStorage.getItem("auth_token")
        })
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();

    // Document state tracking
    let documentContent = "";
    let documentVersion = 0;
    let pendingOperations = [];
    let activeUsers = [];
    let userCursors = {};
    
    // 2. Set up event handlers for server messages
    
    // Document loaded or full sync
    connection.on("DocumentState", (state) => {
        console.log("Received document state:", state);
        documentContent = state.Content;
        documentVersion = state.Version;
        
        // Update the editor with new content
        editor.setValue(documentContent);
        
        // Clear pending operations as we have a new base state
        pendingOperations = [];
    });
    
    // List of all users in the document
    connection.on("ActiveUsers", (users) => {
        console.log("Active users:", users);
        activeUsers = users;
        
        // Update UI to show active users
        updateActiveUsersUI(users);
    });
    
    // New user joined
    connection.on("UserJoined", (userId, userInfo) => {
        console.log("User joined:", userInfo);
        
        // Add user to active users list if not already there
        if (!activeUsers.some(u => u.UserId === userId)) {
            activeUsers.push(userInfo);
            updateActiveUsersUI(activeUsers);
        }
        
        // Show notification
        showNotification(`${userInfo.DisplayName} joined the document`);
    });
    
    // User left
    connection.on("UserLeft", (userId) => {
        console.log("User left:", userId);
        
        // Remove user from active users list
        activeUsers = activeUsers.filter(u => u.UserId !== userId);
        
        // Remove user's cursor
        delete userCursors[userId];
        
        // Update UI
        updateActiveUsersUI(activeUsers);
        updateRemoteCursors();
    });
    
    // Receive operation from another user
    connection.on("ReceiveOperation", (operation) => {
        console.log("Received operation:", operation);
        
        // Apply the operation to our document
        if (operation.BaseVersion === documentVersion) {
            // Simple case: operation is based on our current version
            applyOperation(operation.Components);
            documentVersion++;
        } else {
            // Handle conflict with operational transformation
            // This would require transforming any pending local operations
            console.log("Version mismatch, requesting sync");
            connection.invoke("RequestSync", documentVersion);
        }
    });
    
    // Cursor position changed for another user
    connection.on("CursorPositionChanged", (userId, position) => {
        console.log("Cursor moved for user:", userId, position);
        
        // Store user's cursor position
        userCursors[userId] = position;
        
        // Update cursor display in editor
        updateRemoteCursors();
    });
    
    // Typing indicator
    connection.on("UserIsTyping", (userId, isTyping) => {
        console.log("User typing status:", userId, isTyping);
        
        // Update user's typing status
        const user = activeUsers.find(u => u.UserId === userId);
        if (user) {
            user.IsTyping = isTyping;
            updateActiveUsersUI(activeUsers);
        }
    });
    
    // Server requesting sync
    connection.on("SyncRequired", (serverVersion) => {
        console.log("Sync required to version:", serverVersion);
        connection.invoke("RequestSync", documentVersion);
    });
    
    // Error handling
    connection.on("HubError", (error) => {
        console.error("Hub error:", error);
        showErrorNotification(`Error: ${error.Message} (${error.Code})`);
    });
    
    // 3. Connect and join the document
    connection.start()
        .then(() => {
            console.log("Connected to hub");
            return connection.invoke("JoinDocument", documentId);
        })
        .then(() => {
            console.log("Joined document:", documentId);
        })
        .catch(err => {
            console.error("Connection error:", err);
            if (err.message) {
                try {
                    const error = JSON.parse(err.message);
                    showErrorNotification(`Error: ${error.Message} (${error.Code})`);
                } catch {
                    showErrorNotification(`Connection error: ${err.message}`);
                }
            }
        });
    
    // 4. Editor integration functions
    
    // Handle local text changes
    const handleEditorChange = (changeEvent) => {
        // Construct an operation from the change
        const operation = createOperationFromChange(changeEvent, documentVersion);
        pendingOperations.push(operation);
        
        // Send the operation to the server
        connection.invoke("SendOperation", operation)
            .catch(err => {
                console.error("Error sending operation:", err);
                // Handle error - maybe revert the change or try to sync
            });
        
        // Start typing indicator
        connection.invoke("StartTyping");
        
        // Clear the typing timeout if it exists
        if (window.typingTimeout) {
            clearTimeout(window.typingTimeout);
        }
        
        // Set a timeout to stop the typing indicator
        window.typingTimeout = setTimeout(() => {
            connection.invoke("StopTyping");
        }, 2000);
    };
    
    // Create an operation from editor change
    const createOperationFromChange = (change, baseVersion) => {
        const components = [];
        
        // Calculate retain before change
        if (change.start > 0) {
            components.push({
                Type: 0, // ComponentType.Retain
                Count: change.start
            });
        }
        
        if (change.removedText && change.removedText.length > 0) {
            // Add delete component
            components.push({
                Type: 2, // ComponentType.Delete
                Count: change.removedText.length
            });
        }
        
        if (change.insertedText && change.insertedText.length > 0) {
            // Add insert component
            components.push({
                Type: 1, // ComponentType.Insert
                Text: change.insertedText
            });
        }
        
        // Calculate retain after change
        const remainingChars = documentContent.length - (change.start + (change.removedText?.length || 0));
        if (remainingChars > 0) {
            components.push({
                Type: 0, // ComponentType.Retain
                Count: remainingChars
            });
        }
        
        return {
            OperationId: generateGuid(),
            BaseVersion: baseVersion,
            Components: components
        };
    };
    
    // Apply an operation to the local document
    const applyOperation = (components) => {
        let newContent = "";
        let currentPos = 0;
        
        for (const component of components) {
            switch (component.Type) {
                case 0: // Retain
                    newContent += documentContent.substring(currentPos, currentPos + component.Count);
                    currentPos += component.Count;
                    break;
                case 1: // Insert
                    newContent += component.Text;
                    break;
                case 2: // Delete
                    currentPos += component.Count;
                    break;
            }
        }
        
        // Update the document content
        documentContent = newContent;
        
        // Update the editor (avoiding infinite loop by disabling change handler temporarily)
        disableChangeHandler();
        editor.setValue(documentContent);
        enableChangeHandler();
    };
    
    // Handle cursor movement
    const handleCursorMove = (cursorEvent) => {
        const position = {
            Position: cursorEvent.position,
            HasSelection: cursorEvent.hasSelection,
            SelectionStart: cursorEvent.selectionStart,
            SelectionEnd: cursorEvent.selectionEnd
        };
        
        // Send cursor position to server
        connection.invoke("UpdateCursorPosition", position);
    };
    
    // Helper functions for UI updates
    
    const updateActiveUsersUI = (users) => {
        const userList = document.getElementById('active-users');
        userList.innerHTML = '';
        
        users.forEach(user => {
            const userElement = document.createElement('div');
            userElement.className = 'user-item';
            userElement.style.borderLeftColor = user.Color;
            
            userElement.innerHTML = `
                <span class="user-name">${user.DisplayName}</span>
                ${user.IsTyping ? '<span class="typing-indicator">typing...</span>' : ''}
            `;
            
            userList.appendChild(userElement);
        });
    };
    
    const updateRemoteCursors = () => {
        // Clear existing cursor markers
        clearRemoteCursors();
        
        // Add cursor for each user
        Object.entries(userCursors).forEach(([userId, position]) => {
            const user = activeUsers.find(u => u.UserId === userId);
            if (user) {
                addRemoteCursor(position, user);
            }
        });
    };
    
    // Utility function for generating a GUID for operation IDs
    const generateGuid = () => {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
            const r = Math.random() * 16 | 0;
            const v = c === 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    };
    
    // Cleanup function to be called when leaving the document
    const cleanup = () => {
        // Leave the document explicitly
        if (connection.state === signalR.HubConnectionState.Connected) {
            connection.invoke("LeaveDocument")
                .catch(err => console.error("Error leaving document:", err));
        }
        
        // Stop the connection
        connection.stop();
    };
    
    // Return public API
    return {
        cleanup,
        // Additional methods could be exposed as needed
    };
};

// Example usage:
document.addEventListener('DOMContentLoaded', () => {
    const documentId = getDocumentIdFromUrl(); // Implement this function
    const collaboration = setupMarkdownCollaboration(documentId);
    
    // Handle page unload
    window.addEventListener('beforeunload', () => {
        collaboration.cleanup();
    });
});
```


This example demonstrates:

1. **Connection Setup**:
    - Creating and configuring the SignalR connection
    - Handling authentication via JWT token

2. **Event Handling**:
    - Processing all server events (DocumentState, ActiveUsers, etc.)
    - Error handling for hub exceptions

3. **Document Collaboration**:
    - Sending and receiving operations
    - Handling version conflicts
    - Integrating with a text editor

4. **User Awareness**:
    - Displaying active users
    - Showing cursor positions
    - Indicating when users are typing

5. **State Management**:
    - Tracking document content and version
    - Managing pending operations

This is a comprehensive example that can be adapted to work with any JavaScript editor component (CodeMirror, Monaco, etc.) by implementing the specific editor integration functions like `disableChangeHandler`, `enableChangeHandler`, and `editor.setValue()`.