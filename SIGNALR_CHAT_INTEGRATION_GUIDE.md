# 🚀 Hướng dẫn tích hợp SignalR Chat Realtime

## 📋 Mục lục
1. [Backend Integration](#backend-integration)
2. [Frontend Integration](#frontend-integration)
3. [Testing Guide](#testing-guide)
4. [Advanced Features](#advanced-features)

---

## 🔧 Backend Integration

### Bước 1: Tạo ChatHub Infrastructure

#### 1.1. Tạo Interface `IChatHub`

**File:** `BLL/Interfaces/Infrastructure/IChatHub.cs`

```csharp
namespace BLL.Interfaces.Infrastructure;

/// <summary>
/// Interface cho ChatHub Service để gửi tin nhắn real-time
/// </summary>
public interface IChatHub
{
    /// <summary>
    /// Gửi tin nhắn cho một user cụ thể
    /// </summary>
    Task SendMessageToUser(ulong userId, object message);
    
    /// <summary>
    /// Gửi tin nhắn cho cả customer và vendor trong conversation
    /// </summary>
    Task SendMessageToConversation(ulong customerId, ulong vendorId, object message);
    
    /// <summary>
    /// Gửi typing indicator cho conversation
    /// </summary>
    Task SendTypingIndicator(ulong conversationId, ulong senderId, string senderName);
}
```

#### 1.2. Tạo ChatHub

**File:** `Infrastructure/SignalR/ChatHub.cs`

```csharp
using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.SignalR;

/// <summary>
/// SignalR Hub để xử lý chat real-time giữa Customer và Vendor
/// </summary>
[Authorize]
public class ChatHub : BaseHub
{
    /// <summary>
    /// Khi client kết nối tới Hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = TryGetCurrentUserId();
        
        if (userId.HasValue)
        {
            // Add user vào group riêng của họ
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId.Value}");
            
            var role = GetCurrentUserRole();
            if (!string.IsNullOrEmpty(role))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Role_{role}");
            }
        }
        
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Khi client ngắt kết nối
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = TryGetCurrentUserId();
        
        if (userId.HasValue)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId.Value}");
            
            var role = GetCurrentUserRole();
            if (!string.IsNullOrEmpty(role))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Role_{role}");
            }
        }
        
        await base.OnDisconnectedAsync(exception);
    }
    
    /// <summary>
    /// Client gọi để gửi typing indicator
    /// </summary>
    public async Task SendTypingIndicator(ulong conversationId, string recipientUserId)
    {
        var userId = GetCurrentUserId();
        var userName = Context.User?.FindFirst("FullName")?.Value ?? "User";
        
        await Clients.Group($"User_{recipientUserId}")
            .SendCoreAsync("ReceiveTypingIndicator", new object[] 
            { 
                conversationId, 
                userId, 
                userName 
            });
    }

    /// <summary>
    /// Test connection - client có thể gọi để kiểm tra kết nối
    /// </summary>
    public async Task<string> Ping()
    {
        var userId = TryGetCurrentUserId();
        var role = GetCurrentUserRole();
        return $"Chat Hub - Pong from User {userId} (Role: {role})";
    }
}
```

#### 1.3. Tạo ChatHubService Implementation

**File:** `Infrastructure/SignalR/ChatHubService.cs`

```csharp
using BLL.Interfaces.Infrastructure;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.SignalR;

/// <summary>
/// Service để gửi tin nhắn real-time qua SignalR
/// </summary>
public class ChatHubService : IChatHub
{
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatHubService(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    /// <summary>
    /// Gửi tin nhắn cho 1 user cụ thể
    /// </summary>
    public async Task SendMessageToUser(ulong userId, object message)
    {
        await _hubContext.Clients
            .Group($"User_{userId}")
            .SendCoreAsync("ReceiveMessage", new object[] { message });
    }

    /// <summary>
    /// Gửi tin nhắn cho cả customer và vendor trong conversation
    /// </summary>
    public async Task SendMessageToConversation(ulong customerId, ulong vendorId, object message)
    {
        var groupNames = new[] { $"User_{customerId}", $"User_{vendorId}" };
        
        await _hubContext.Clients
            .Groups(groupNames)
            .SendCoreAsync("ReceiveMessage", new object[] { message });
    }

    /// <summary>
    /// Gửi typing indicator
    /// </summary>
    public async Task SendTypingIndicator(ulong conversationId, ulong senderId, string senderName)
    {
        await _hubContext.Clients
            .Group($"Conversation_{conversationId}")
            .SendCoreAsync("ReceiveTypingIndicator", new object[] 
            { 
                conversationId, 
                senderId, 
                senderName 
            });
    }
}
```

### Bước 2: Register Services và Hub

#### 2.1. Register ChatHub Service

**File:** `Infrastructure/Extensions/ServiceCollectionExtensions.cs`

Thêm method mới:

```csharp
public static IServiceCollection AddSignalRChat(this IServiceCollection services)
{
    services.AddScoped<IChatHub, ChatHubService>();
    return services;
}
```

Và gọi nó trong method `AddInfrastructure`:

```csharp
public static IServiceCollection AddInfrastructure(this IServiceCollection services)
{
    services.AddEmail();
    services.AddWeather();
    services.AddSoilGrids();
    services.AddCourier();
    services.AddAddress();
    services.AddPayOS();
    services.AddSignalRNotification();
    services.AddSignalRChat();  // 🔥 THÊM DÒNG NÀY
    services.AddCloudinary();
    return services;
}
```

> **Lưu ý:** Service đã được tự động register qua `builder.Services.AddInfrastructure()` trong Program.cs rồi.

#### 2.2. Map ChatHub Endpoint

**File:** `Controller/Program.cs`

Thêm vào phần **Hub Mapping** (sau `MapHub<NotificationHub>`):

```csharp
// Map SignalR Hubs
app.MapHub<NotificationHub>("/hubs/notification");
app.MapHub<ChatHub>("/hubs/chat");  // 🔥 THÊM DÒNG NÀY
```

### Bước 3: Integrate vào CustomerVendorConversationsService

**File:** `BLL/Services/CustomerVendorConversationsService.cs`

#### 3.1. Inject IChatHub vào Constructor

```csharp
public class CustomerVendorConversationsService : ICustomerVendorConversationsService
{
    private readonly ICustomerVendorConversationsRepository _customerVendorConversationsRepository;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    private readonly IChatHub _chatHub;  // 🔥 THÊM DÒNG NÀY
    
    public CustomerVendorConversationsService(
        ICustomerVendorConversationsRepository customerVendorConversationsRepository,
        ICloudinaryService cloudinaryService, 
        IMapper mapper, 
        IUserRepository userRepository,
        IChatHub chatHub)  // 🔥 THÊM PARAMETER
    {
        _customerVendorConversationsRepository = customerVendorConversationsRepository;
        _cloudinaryService = cloudinaryService;
        _mapper = mapper;
        _userRepository = userRepository;
        _chatHub = chatHub;  // 🔥 THÊM DÒNG NÀY
    }
    
    // ... rest of the code
}
```

#### 3.2. Gửi SignalR notification trong SendNewMessageAsync

Tìm method `SendNewMessageAsync` và thêm sau khi tạo message thành công:

```csharp
public async Task<CustomerVendorMessageResponseDTO> SendNewMessageAsync(
    ulong userId, UserRole role, 
    ulong conversationId, 
    CustomerVendorMessageCreateDTO dto, 
    CancellationToken cancellationToken = default)
{
    // ... existing code ...
    
    await _customerVendorConversationsRepository.SendNewMessageAsync(
        conversation, message, mediaLinks, cancellationToken);
    
    var response = _mapper.Map<CustomerVendorMessageResponseDTO>(
        await _customerVendorConversationsRepository.GetNewestMessageByConversationIdAsync(
            conversationId, cancellationToken));
    
    response.Images = _mapper.Map<List<MediaLinkItemDTO>>(
        await _customerVendorConversationsRepository.GetAllMessageImagesByIdAsync(
            response.Id, cancellationToken));
    
    // 🔥 GỬI TIN NHẮN REALTIME
    await _chatHub.SendMessageToConversation(
        conversation.CustomerId, 
        conversation.VendorId, 
        response);
    
    return response;
}
```

#### 3.3. Gửi SignalR notification khi tạo conversation mới (Optional)

Trong method `CreateConversationAsync`, sau khi tạo conversation:

```csharp
// ... existing code ...
return response;

// 🔥 (OPTIONAL) Notify vendor có conversation mới
await _chatHub.SendMessageToUser(
    dto.VendorId, 
    new 
    { 
        Type = "NewConversation", 
        ConversationId = response.Id,
        Message = response.CustomerVendorMessages.FirstOrDefault()
    });

return response;
```

---

## 💻 Frontend Integration (TypeScript/React)

### Bước 1: Install SignalR Client

```bash
npm install @microsoft/signalr
# or
yarn add @microsoft/signalr
```

### Bước 2: Tạo Chat Hub Connection Service

**File:** `src/services/chatHub.ts`

```typescript
import * as signalR from '@microsoft/signalr';

export interface ChatMessage {
  id: number;
  senderType: 'Customer' | 'Vendor';
  messageText: string;
  isRead: boolean;
  createdAt: string;
  images: Array<{
    id: number;
    imageUrl: string;
    sortOrder: number;
  }>;
}

export interface TypingIndicator {
  conversationId: number;
  senderId: number;
  senderName: string;
}

class ChatHubService {
  private connection: signalR.HubConnection | null = null;
  private messageHandlers: Array<(message: ChatMessage) => void> = [];
  private typingHandlers: Array<(indicator: TypingIndicator) => void> = [];

  /**
   * Khởi tạo kết nối đến ChatHub
   */
  async connect(accessToken: string): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      console.log('ChatHub already connected');
      return;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${process.env.REACT_APP_API_URL}/hubs/chat`, {
        accessTokenFactory: () => accessToken,
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets,
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          // Retry strategy: 0s, 2s, 10s, 30s
          if (retryContext.previousRetryCount === 0) return 0;
          if (retryContext.previousRetryCount === 1) return 2000;
          if (retryContext.previousRetryCount === 2) return 10000;
          return 30000;
        },
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // Event: Nhận tin nhắn mới
    this.connection.on('ReceiveMessage', (message: ChatMessage) => {
      console.log('📨 Received message:', message);
      this.messageHandlers.forEach((handler) => handler(message));
    });

    // Event: Nhận typing indicator
    this.connection.on('ReceiveTypingIndicator', (indicator: TypingIndicator) => {
      console.log('⌨️ Typing indicator:', indicator);
      this.typingHandlers.forEach((handler) => handler(indicator));
    });

    // Event: Kết nối lại thành công
    this.connection.onreconnected(() => {
      console.log('✅ ChatHub reconnected');
    });

    // Event: Đang kết nối lại
    this.connection.onreconnecting(() => {
      console.warn('🔄 ChatHub reconnecting...');
    });

    // Event: Ngắt kết nối
    this.connection.onclose((error) => {
      console.error('❌ ChatHub connection closed:', error);
    });

    try {
      await this.connection.start();
      console.log('✅ ChatHub connected successfully');

      // Test connection
      const pong = await this.connection.invoke('Ping');
      console.log('🏓', pong);
    } catch (error) {
      console.error('❌ ChatHub connection error:', error);
      throw error;
    }
  }

  /**
   * Ngắt kết nối
   */
  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
      this.messageHandlers = [];
      this.typingHandlers = [];
      console.log('🔌 ChatHub disconnected');
    }
  }

  /**
   * Đăng ký handler nhận tin nhắn mới
   */
  onMessageReceived(handler: (message: ChatMessage) => void): () => void {
    this.messageHandlers.push(handler);
    
    // Return unsubscribe function
    return () => {
      this.messageHandlers = this.messageHandlers.filter((h) => h !== handler);
    };
  }

  /**
   * Đăng ký handler nhận typing indicator
   */
  onTypingIndicator(handler: (indicator: TypingIndicator) => void): () => void {
    this.typingHandlers.push(handler);
    
    return () => {
      this.typingHandlers = this.typingHandlers.filter((h) => h !== handler);
    };
  }

  /**
   * Gửi typing indicator
   */
  async sendTypingIndicator(conversationId: number, recipientUserId: string): Promise<void> {
    if (this.connection?.state !== signalR.HubConnectionState.Connected) {
      console.warn('ChatHub not connected');
      return;
    }

    try {
      await this.connection.invoke('SendTypingIndicator', conversationId, recipientUserId);
    } catch (error) {
      console.error('Error sending typing indicator:', error);
    }
  }

  /**
   * Kiểm tra trạng thái kết nối
   */
  isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected;
  }
}

// Export singleton instance
export const chatHubService = new ChatHubService();
```

### Bước 3: Tích hợp vào React Component

**File:** `src/hooks/useChatHub.ts`

```typescript
import { useEffect, useCallback } from 'react';
import { chatHubService, ChatMessage } from '../services/chatHub';
import { useAuth } from './useAuth'; // Your auth hook

export function useChatHub() {
  const { token } = useAuth();

  useEffect(() => {
    if (token) {
      // Connect khi có token
      chatHubService.connect(token).catch((error) => {
        console.error('Failed to connect to ChatHub:', error);
      });
    }

    // Cleanup: Disconnect khi unmount
    return () => {
      chatHubService.disconnect();
    };
  }, [token]);

  return {
    isConnected: chatHubService.isConnected(),
    onMessageReceived: chatHubService.onMessageReceived.bind(chatHubService),
    onTypingIndicator: chatHubService.onTypingIndicator.bind(chatHubService),
    sendTypingIndicator: chatHubService.sendTypingIndicator.bind(chatHubService),
  };
}
```

### Bước 4: Sử dụng trong Chat Component

**File:** `src/components/ChatConversation.tsx`

```typescript
import React, { useState, useEffect, useCallback } from 'react';
import { useChatHub } from '../hooks/useChatHub';
import { ChatMessage } from '../services/chatHub';

interface ChatConversationProps {
  conversationId: number;
  currentUserId: number;
}

export const ChatConversation: React.FC<ChatConversationProps> = ({
  conversationId,
  currentUserId,
}) => {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [isTyping, setIsTyping] = useState(false);
  const { onMessageReceived, onTypingIndicator } = useChatHub();

  // Xử lý khi nhận tin nhắn mới
  useEffect(() => {
    const unsubscribe = onMessageReceived((message) => {
      // Chỉ add tin nhắn thuộc conversation hiện tại
      // (Bạn có thể thêm conversationId vào message response để filter)
      setMessages((prev) => [message, ...prev]);
      
      // Play notification sound
      playNotificationSound();
      
      // Show browser notification nếu tab không focus
      if (document.hidden) {
        showBrowserNotification(message);
      }
    });

    return unsubscribe;
  }, [onMessageReceived]);

  // Xử lý typing indicator
  useEffect(() => {
    const unsubscribe = onTypingIndicator((indicator) => {
      if (indicator.conversationId === conversationId) {
        setIsTyping(true);
        
        // Clear typing indicator sau 3s
        setTimeout(() => setIsTyping(false), 3000);
      }
    });

    return unsubscribe;
  }, [onTypingIndicator, conversationId]);

  const playNotificationSound = () => {
    const audio = new Audio('/sounds/message.mp3');
    audio.play().catch(console.error);
  };

  const showBrowserNotification = (message: ChatMessage) => {
    if ('Notification' in window && Notification.permission === 'granted') {
      new Notification('Tin nhắn mới', {
        body: message.messageText,
        icon: '/logo192.png',
      });
    }
  };

  return (
    <div className="chat-conversation">
      <div className="messages">
        {messages.map((msg) => (
          <div key={msg.id} className={`message ${msg.senderType}`}>
            <p>{msg.messageText}</p>
            {msg.images.map((img) => (
              <img key={img.id} src={img.imageUrl} alt="" />
            ))}
            <span className="time">{new Date(msg.createdAt).toLocaleTimeString()}</span>
          </div>
        ))}
      </div>
      
      {isTyping && (
        <div className="typing-indicator">
          <span>Đang nhập...</span>
        </div>
      )}
      
      {/* Message input component */}
    </div>
  );
};
```

### Bước 5: Request Browser Notification Permission

**File:** `src/App.tsx` hoặc `src/components/Layout.tsx`

```typescript
useEffect(() => {
  // Request notification permission khi app load
  if ('Notification' in window && Notification.permission === 'default') {
    Notification.requestPermission().then((permission) => {
      console.log('Notification permission:', permission);
    });
  }
}, []);
```

---

## 🧪 Testing Guide

### Test Backend

#### 1. Test ChatHub Connection (Postman/Insomnia)

Sử dụng WebSocket connection test:
```
ws://localhost:5000/hubs/chat?access_token=YOUR_JWT_TOKEN
```

#### 2. Test từ Browser Console

```javascript
// Connect to hub
const connection = new signalR.HubConnectionBuilder()
    .withUrl('http://localhost:5000/hubs/chat', {
        accessTokenFactory: () => 'YOUR_JWT_TOKEN'
    })
    .build();

// Listen for messages
connection.on('ReceiveMessage', (message) => {
    console.log('Received:', message);
});

// Start connection
await connection.start();
console.log('Connected!');

// Test ping
const pong = await connection.invoke('Ping');
console.log(pong);
```

#### 3. Test Send Message Flow

1. User A gửi tin nhắn qua API endpoint
2. Check console của User B → phải nhận được event `ReceiveMessage`
3. Verify data structure của message

### Test Frontend

#### 1. Test với 2 Browser/Tabs

- Tab 1: Login as Customer
- Tab 2: Login as Vendor
- Gửi tin nhắn từ Tab 1 → Tab 2 phải nhận realtime

#### 2. Test Reconnection

- Ngắt internet → Bật lại
- Check console log: "ChatHub reconnected"
- Gửi tin nhắn → vẫn hoạt động

#### 3. Test Typing Indicator

- User A typing → User B thấy "Đang nhập..."
- Stop typing 3s → indicator biến mất

---

## 🚀 Advanced Features (Optional)

### 1. Message Read Status

**Backend:**
```csharp
// ICustomerVendorConversationsRepository
Task<CustomerVendorMessage> MarkMessageAsReadAsync(ulong messageId, CancellationToken ct);

// Service
public async Task MarkMessageAsReadAsync(ulong messageId, ulong userId)
{
    var message = await _repo.MarkMessageAsReadAsync(messageId, ct);
    
    // Notify sender that message was read
    await _chatHub.SendMessageReadStatus(message.SenderId, messageId);
}
```

**Frontend:**
```typescript
connection.on('MessageReadStatusUpdated', (messageId: number) => {
    // Update UI: show double check mark
    updateMessageReadStatus(messageId);
});
```

### 2. Online Status

**Backend (ChatHub):**
```csharp
private static readonly Dictionary<ulong, int> OnlineUsers = new();

public override async Task OnConnectedAsync()
{
    var userId = GetCurrentUserId();
    OnlineUsers[userId] = OnlineUsers.GetValueOrDefault(userId) + 1;
    
    await Clients.All.SendCoreAsync("UserOnlineStatusChanged", 
        new object[] { userId, true });
    
    await base.OnConnectedAsync();
}

public override async Task OnDisconnectedAsync(Exception? exception)
{
    var userId = GetCurrentUserId();
    OnlineUsers[userId]--;
    
    if (OnlineUsers[userId] <= 0)
    {
        OnlineUsers.Remove(userId);
        await Clients.All.SendCoreAsync("UserOnlineStatusChanged", 
            new object[] { userId, false });
    }
    
    await base.OnDisconnectedAsync(exception);
}
```

### 3. File Upload Progress (Large Files)

**Backend:**
```csharp
public async Task UpdateUploadProgress(string uploadId, int progress)
{
    var userId = GetCurrentUserId();
    await Clients.User(userId.ToString())
        .SendCoreAsync("UploadProgress", new object[] { uploadId, progress });
}
```

### 4. Group Chat (Multiple Users)

Nếu sau này cần group chat:
```csharp
// Join conversation group
await Groups.AddToGroupAsync(Context.ConnectionId, $"Conversation_{conversationId}");

// Send to group
await Clients.Group($"Conversation_{conversationId}")
    .SendCoreAsync("ReceiveMessage", new object[] { message });
```

---

## 📚 Troubleshooting

### Lỗi thường gặp:

#### 1. "ChatHub connection error: Failed to complete negotiation"
- **Nguyên nhân:** CORS chưa config đúng
- **Giải pháp:** Check `Program.cs` CORS policy, ensure `.AllowCredentials()` if using cookies

#### 2. "401 Unauthorized"
- **Nguyên nhân:** JWT token invalid hoặc expired
- **Giải pháp:** 
  - Check token format: `Bearer <token>`
  - Verify token chưa expired
  - Check `[Authorize]` attribute on ChatHub

#### 3. Tin nhắn không nhận được
- **Check:**
  - User có join đúng group không? → Check `OnConnectedAsync` logs
  - Group name có đúng format không? → `User_{userId}`
  - Hub có được map trong `Program.cs` không?

#### 4. Reconnection không hoạt động
- **Giải pháp:** Ensure `.withAutomaticReconnect()` đã config ở frontend

---

## 📊 Performance Tips

### Backend:
- Sử dụng `Group` thay vì `Clients.All` để target specific users
- Không gửi quá nhiều data trong một message (optimize DTO)
- Consider caching online users list

### Frontend:
- Debounce typing indicator (chỉ gửi sau 500ms user stop typing)
- Limit số lượng messages render (virtualization cho long chat)
- Lazy load chat history khi scroll up

---

## 🔐 Security Checklist

- [x] ChatHub có `[Authorize]` attribute
- [x] Validate user permission trước khi gửi message
- [x] Không expose sensitive data trong SignalR events
- [x] Rate limiting cho SendTypingIndicator
- [x] Sanitize message content trước khi send

---

## 📝 Summary

### Backend Changes:
1. ✅ Tạo `IChatHub` interface
2. ✅ Tạo `ChatHub` class
3. ✅ Tạo `ChatHubService` implementation
4. ✅ Register services trong `Program.cs`
5. ✅ Map hub endpoint: `/hubs/chat`
6. ✅ Inject `IChatHub` vào `CustomerVendorConversationsService`
7. ✅ Gọi `SendMessageToConversation()` trong `SendNewMessageAsync()`

### Frontend Changes:
1. ✅ Install `@microsoft/signalr`
2. ✅ Tạo `chatHub.ts` service
3. ✅ Tạo `useChatHub` hook
4. ✅ Integrate vào Chat component
5. ✅ Request notification permission

### Testing:
- ✅ Test connection với 2 users
- ✅ Test send/receive messages
- ✅ Test reconnection
- ✅ Test typing indicator

---

**🎉 Hoàn thành! Chat realtime đã sẵn sàng sử dụng.**

Nếu cần hỗ trợ thêm, tham khảo:
- SignalR Docs: https://learn.microsoft.com/en-us/aspnet/core/signalr/
- SignalR JS Client: https://learn.microsoft.com/en-us/javascript/api/@microsoft/signalr/
