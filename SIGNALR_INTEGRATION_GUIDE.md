# 🔔 SyncVerse - SignalR Real-time Integration Guide

## 📡 SignalR Hub Overview

SyncVerse uses **SignalR** for real-time notifications across web, mobile, and desktop applications.

---

## 🌐 Connection Details

### Hub Endpoint
```
ws://localhost:5131/hubs/notifications
wss://localhost:7001/hubs/notifications (HTTPS)
```

### Authentication
SignalR requires JWT token in the connection:
```
?access_token={your_jwt_token}
```

---

## 📨 Events (Server → Client)

### 1. **ReceiveNotification**
Triggered when a new notification is sent to the user.

**Event Name:** `ReceiveNotification`

**Payload:**
```json
{
  "id": "notification-id",
  "userId": "user-id",
  "type": "TaskAssigned",
  "title": "New Task Assigned",
  "message": "You have been assigned to task 'Implement Authentication'",
  "relatedEntityId": "task-id",
  "relatedEntityType": "Task",
  "isRead": false,
  "createdAt": "2024-01-15T10:30:00Z"
}
```

**Notification Types:**
- `TaskAssigned` - New task assigned to you
- `TaskStatusChanged` - Task status updated
- `TaskCommentAdded` - New comment on your task
- `ProjectInvitation` - Invited to a project
- `TaskDueSoon` - Task due date approaching
- `MilestoneCompleted` - Milestone reached
- `TimeLogReminder` - Reminder to log time

---

## 💻 Integration Examples

### 🌐 JavaScript/TypeScript (Web)

#### Installation
```bash
npm install @microsoft/signalr
```

#### Connection Setup
```typescript
import * as signalR from "@microsoft/signalr";

// Get JWT token from your auth system
const token = localStorage.getItem("jwt_token");

// Create connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5131/hubs/notifications", {
        accessTokenFactory: () => token
    })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

// Listen for notifications
connection.on("ReceiveNotification", (notification) => {
    console.log("New notification:", notification);
    
    // Show notification in UI
    showNotification(notification.title, notification.message);
    
    // Update notification badge
    updateNotificationBadge();
});

// Start connection
connection.start()
    .then(() => console.log("✅ SignalR Connected"))
    .catch(err => console.error("❌ SignalR Error:", err));

// Handle disconnection
connection.onclose(() => {
    console.log("⚠️ SignalR Disconnected");
});
```

#### React Hook Example
```typescript
import { useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';

export const useNotifications = (token: string) => {
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
    const [notifications, setNotifications] = useState<any[]>([]);

    useEffect(() => {
        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl("http://localhost:5131/hubs/notifications", {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect()
            .build();

        newConnection.on("ReceiveNotification", (notification) => {
            setNotifications(prev => [notification, ...prev]);
        });

        newConnection.start()
            .then(() => console.log("SignalR Connected"))
            .catch(err => console.error(err));

        setConnection(newConnection);

        return () => {
            newConnection.stop();
        };
    }, [token]);

    return { connection, notifications };
};
```

---

### 📱 Flutter/Dart (Mobile)

#### Installation
```yaml
dependencies:
  signalr_netcore: ^1.3.6
```

#### Connection Setup
```dart
import 'package:signalr_netcore/signalr_client.dart';

class NotificationService {
  HubConnection? _hubConnection;
  
  Future<void> connect(String token) async {
    _hubConnection = HubConnectionBuilder()
        .withUrl(
          "http://localhost:5131/hubs/notifications",
          HttpConnectionOptions(
            accessTokenFactory: () => Future.value(token),
          ),
        )
        .withAutomaticReconnect()
        .build();

    // Listen for notifications
    _hubConnection!.on("ReceiveNotification", (arguments) {
      final notification = arguments?[0];
      print("New notification: $notification");
      
      // Show local notification
      _showLocalNotification(notification);
    });

    // Start connection
    await _hubConnection!.start();
    print("✅ SignalR Connected");
  }

  void _showLocalNotification(dynamic notification) {
    // Use flutter_local_notifications package
    // to show notification in system tray
  }

  Future<void> disconnect() async {
    await _hubConnection?.stop();
  }
}
```

#### Usage in Flutter App
```dart
class NotificationProvider extends ChangeNotifier {
  final NotificationService _notificationService = NotificationService();
  List<dynamic> notifications = [];

  Future<void> initialize(String token) async {
    await _notificationService.connect(token);
  }

  void addNotification(dynamic notification) {
    notifications.insert(0, notification);
    notifyListeners();
  }
}
```

---

### 🎮 Unity/C# (Gaming)

#### Installation
```
Install-Package Microsoft.AspNetCore.SignalR.Client
```

#### Connection Setup
```csharp
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;
using System;

public class NotificationManager : MonoBehaviour
{
    private HubConnection _connection;
    private string _jwtToken;

    async void Start()
    {
        // Get JWT token from your auth manager
        _jwtToken = AuthManager.Instance.GetToken();
        
        // Create connection
        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5131/hubs/notifications", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(_jwtToken);
            })
            .WithAutomaticReconnect()
            .Build();

        // Listen for notifications
        _connection.On<Notification>("ReceiveNotification", (notification) =>
        {
            Debug.Log($"New notification: {notification.Title}");
            
            // Show in-game notification
            ShowInGameNotification(notification);
        });

        // Start connection
        try
        {
            await _connection.StartAsync();
            Debug.Log("✅ SignalR Connected");
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ SignalR Error: {ex.Message}");
        }
    }

    void ShowInGameNotification(Notification notification)
    {
        // Display notification in your 3D UI
        UIManager.Instance.ShowNotification(notification.Title, notification.Message);
    }

    async void OnDestroy()
    {
        if (_connection != null)
        {
            await _connection.StopAsync();
        }
    }
}

[Serializable]
public class Notification
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string Type { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string RelatedEntityId { get; set; }
    public string RelatedEntityType { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

## 🔧 Error Handling

### Connection Errors
```typescript
connection.onreconnecting((error) => {
    console.warn("⚠️ Reconnecting...", error);
    showReconnectingUI();
});

connection.onreconnected((connectionId) => {
    console.log("✅ Reconnected!", connectionId);
    hideReconnectingUI();
});

connection.onclose((error) => {
    console.error("❌ Connection closed", error);
    showOfflineUI();
});
```

### Token Expiration
```typescript
// Refresh token before it expires
setInterval(async () => {
    const newToken = await refreshJwtToken();
    
    // Reconnect with new token
    await connection.stop();
    connection = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:5131/hubs/notifications", {
            accessTokenFactory: () => newToken
        })
        .build();
    
    await connection.start();
}, 3600000); // Every hour
```

---

## 🧪 Testing SignalR Connection

### Using Browser Console
```javascript
// Open browser console on http://localhost:5131/swagger

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/notifications?access_token=YOUR_JWT_TOKEN")
    .build();

connection.on("ReceiveNotification", (notification) => {
    console.log("Notification received:", notification);
});

connection.start()
    .then(() => console.log("Connected!"))
    .catch(err => console.error(err));
```

### Using Postman (WebSocket)
1. Create new WebSocket request
2. URL: `ws://localhost:5131/hubs/notifications?access_token={token}`
3. Send handshake message
4. Listen for notifications

---

## 📊 Production Considerations

### 1. **Scalability**
For production with multiple servers, use Azure SignalR Service or Redis backplane:
```csharp
builder.Services.AddSignalR()
    .AddAzureSignalR(builder.Configuration["Azure:SignalR:ConnectionString"]);
```

### 2. **Security**
- Always use HTTPS (WSS) in production
- Validate JWT tokens on every connection
- Implement rate limiting

### 3. **Performance**
- Use `WithAutomaticReconnect()` for network resilience
- Batch notifications when possible
- Implement notification filtering on client-side

---

## 🎯 Use Cases

### 1. **Live Task Updates**
When manager updates task → Employee sees update in real-time

### 2. **Real-time Chat**
Team members can see typing indicators and new messages instantly

### 3. **3D Virtual Office**
See when users move in 3D space, join rooms, or change status

### 4. **Live Dashboard**
Project statistics update without page refresh

---

## 🆘 Troubleshooting

### Issue: "WebSocket connection failed"
**Solution:** Check CORS configuration in Program.cs

### Issue: "Unauthorized" error
**Solution:** Ensure JWT token is valid and passed correctly

### Issue: "Connection keeps dropping"
**Solution:** Implement `WithAutomaticReconnect()` with retry policy

### Issue: "Not receiving notifications"
**Solution:** 
1. Check if connection is active
2. Verify user ID matches in JWT
3. Check server-side notification sending code

---

## 📞 Support

For SignalR integration issues:
- Check Swagger: `http://localhost:5131/swagger`
- Review server logs for connection errors
- Test connection with browser console first

---

**Last Updated:** March 2024
