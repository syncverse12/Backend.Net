# Real-time Notifications with SignalR - Frontend Integration Guide

## 📡 SignalR Connection Setup

### Installation (JavaScript/TypeScript)
```bash
npm install @microsoft/signalr
```

### Connection Setup

```typescript
import * as signalR from "@microsoft/signalr";

class NotificationService {
  private connection: signalR.HubConnection;

  constructor(token: string) {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl("https://your-api-url/hubs/notifications", {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    this.setupEventHandlers();
  }

  private setupEventHandlers() {
    // Receive new notification
    this.connection.on("ReceiveNotification", (notification) => {
      console.log("New notification:", notification);
      this.showNotification(notification);
      this.updateUnreadCount();
    });

    // Notification marked as read
    this.connection.on("NotificationMarkedAsRead", (notificationId) => {
      console.log("Notification marked as read:", notificationId);
      this.updateNotificationUI(notificationId);
    });
  }

  async start() {
    try {
      await this.connection.start();
      console.log("✅ SignalR Connected");
    } catch (err) {
      console.error("❌ SignalR Connection Error:", err);
      setTimeout(() => this.start(), 5000); // Retry after 5 seconds
    }
  }

  async stop() {
    await this.connection.stop();
  }

  private showNotification(notification: any) {
    // Show browser notification or toast
    if ("Notification" in window && Notification.permission === "granted") {
      new Notification(notification.title, {
        body: notification.message,
        icon: "/notification-icon.png"
      });
    }
  }

  private updateUnreadCount() {
    // Update unread count badge in UI
    // Call API: GET /api/notifications/unread-count
  }

  private updateNotificationUI(notificationId: string) {
    // Update specific notification in the list
  }
}

// Usage
const token = "your-jwt-token";
const notificationService = new NotificationService(token);
notificationService.start();
```

---

## 📱 React Example

```tsx
import React, { useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';

interface Notification {
  notificationId: string;
  type: number;
  title: string;
  message: string;
  taskId?: string;
  taskTitle?: string;
  isRead: boolean;
  createdAt: string;
}

export const NotificationProvider: React.FC = ({ children }) => {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);

  useEffect(() => {
    const token = localStorage.getItem('authToken');
    
    if (!token) return;

    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7080/hubs/notifications', {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    setConnection(newConnection);
  }, []);

  useEffect(() => {
    if (connection) {
      connection.start()
        .then(() => {
          console.log('✅ Connected to SignalR');

          connection.on('ReceiveNotification', (notification: Notification) => {
            setNotifications(prev => [notification, ...prev]);
            setUnreadCount(prev => prev + 1);
            
            // Show toast notification
            showToast(notification);
          });

          connection.on('NotificationMarkedAsRead', (notificationId: string) => {
            setNotifications(prev =>
              prev.map(n =>
                n.notificationId === notificationId
                  ? { ...n, isRead: true }
                  : n
              )
            );
            setUnreadCount(prev => Math.max(0, prev - 1));
          });
        })
        .catch(err => console.error('❌ SignalR Connection Error:', err));

      return () => {
        connection.stop();
      };
    }
  }, [connection]);

  const showToast = (notification: Notification) => {
    // Your toast implementation
    alert(`${notification.title}: ${notification.message}`);
  };

  return (
    <NotificationContext.Provider value={{ notifications, unreadCount }}>
      {children}
    </NotificationContext.Provider>
  );
};
```

---

## 🔧 API Endpoints

### REST APIs (Existing)
```typescript
// Get all notifications
GET /api/notifications?unreadOnly=true
Authorization: Bearer {token}

// Get unread count
GET /api/notifications/unread-count
Authorization: Bearer {token}

// Mark as read
PUT /api/notifications/{notificationId}/read
Authorization: Bearer {token}

// Mark all as read
PUT /api/notifications/mark-all-read
Authorization: Bearer {token}

// Delete notification
DELETE /api/notifications/{notificationId}
Authorization: Bearer {token}
```

### SignalR Hub URL
```
wss://your-api-url/hubs/notifications
```

---

## 📨 Notification Types

```typescript
enum NotificationType {
  TaskAssigned = 1,
  TaskCommented = 2,
  TaskStatusChanged = 3,
  TaskSubmitted = 4,
  TaskReviewed = 5,
  TaskRejected = 6,
  TaskApproved = 7,
  TaskDueSoon = 8,
  TaskOverdue = 9,
  CommentReply = 10
}
```

---

## 🎯 Complete Angular Example

```typescript
import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class NotificationHubService {
  private hubConnection: signalR.HubConnection;
  private notificationsSubject = new BehaviorSubject<any[]>([]);
  private unreadCountSubject = new BehaviorSubject<number>(0);

  public notifications$ = this.notificationsSubject.asObservable();
  public unreadCount$ = this.unreadCountSubject.asObservable();

  constructor() {}

  public startConnection(token: string): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7080/hubs/notifications', {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.hubConnection
      .start()
      .then(() => {
        console.log('✅ SignalR Connected');
        this.registerHandlers();
      })
      .catch(err => console.error('❌ SignalR Error:', err));

    this.hubConnection.onreconnecting(() => {
      console.log('🔄 Reconnecting...');
    });

    this.hubConnection.onreconnected(() => {
      console.log('✅ Reconnected');
    });
  }

  private registerHandlers(): void {
    this.hubConnection.on('ReceiveNotification', (notification) => {
      const current = this.notificationsSubject.value;
      this.notificationsSubject.next([notification, ...current]);
      this.unreadCountSubject.next(this.unreadCountSubject.value + 1);
    });

    this.hubConnection.on('NotificationMarkedAsRead', (notificationId) => {
      const current = this.notificationsSubject.value;
      const updated = current.map(n =>
        n.notificationId === notificationId ? { ...n, isRead: true } : n
      );
      this.notificationsSubject.next(updated);
      this.unreadCountSubject.next(Math.max(0, this.unreadCountSubject.value - 1));
    });
  }

  public stopConnection(): void {
    this.hubConnection?.stop();
  }
}
```

---

## 🔔 Browser Notifications

```typescript
// Request permission
if ("Notification" in window) {
  Notification.requestPermission();
}

// Show notification
function showBrowserNotification(notification: any) {
  if (Notification.permission === "granted") {
    new Notification(notification.title, {
      body: notification.message,
      icon: "/icon.png",
      badge: "/badge.png",
      tag: notification.notificationId,
      requireInteraction: true,
      actions: [
        { action: "view", title: "View" },
        { action: "dismiss", title: "Dismiss" }
      ]
    });
  }
}
```

---

## 🎨 UI Components Examples

### Notification Bell Icon
```tsx
<div className="notification-bell">
  <BellIcon />
  {unreadCount > 0 && (
    <span className="badge">{unreadCount}</span>
  )}
</div>
```

### Notification Dropdown
```tsx
<div className="notifications-dropdown">
  {notifications.map(notification => (
    <div
      key={notification.notificationId}
      className={notification.isRead ? 'read' : 'unread'}
    >
      <h4>{notification.title}</h4>
      <p>{notification.message}</p>
      <small>{formatDate(notification.createdAt)}</small>
    </div>
  ))}
</div>
```

---

## ⚙️ Configuration

### appsettings.json
```json
{
  "SignalR": {
    "ClientTimeoutInterval": "00:01:00",
    "KeepAliveInterval": "00:00:15"
  }
}
```

---

## 🧪 Testing with Postman

Unfortunately, Postman doesn't support WebSocket/SignalR directly. Use these tools instead:

1. **Browser Console**
   - Open Developer Tools
   - Use the JavaScript code above

2. **SignalR Test Tool**
   - https://github.com/dotnet/aspnetcore/tree/main/src/SignalR

3. **Postman WebSocket** (for WebSocket connections)
   - Use the WebSocket feature in Postman

---

## 🚀 Production Considerations

1. **CORS Configuration**
   ```csharp
   builder.Services.AddCors(options =>
   {
       options.AddPolicy("CorsPolicy", builder =>
       {
           builder.WithOrigins("https://your-frontend-url")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
       });
   });
   ```

2. **Scaling with Redis Backplane**
   ```bash
   dotnet add package Microsoft.AspNetCore.SignalR.StackExchangeRedis
   ```

   ```csharp
   builder.Services.AddSignalR()
       .AddStackExchangeRedis("your-redis-connection-string");
   ```

3. **Azure SignalR Service** (for cloud deployment)
   ```bash
   dotnet add package Microsoft.Azure.SignalR
   ```

---

## 📚 Resources

- [SignalR JavaScript Client](https://docs.microsoft.com/en-us/aspnet/core/signalr/javascript-client)
- [SignalR Authentication](https://docs.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz)
- [SignalR Troubleshooting](https://docs.microsoft.com/en-us/aspnet/core/signalr/troubleshoot)
