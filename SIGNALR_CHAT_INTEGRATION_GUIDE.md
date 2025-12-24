# 🚀 Hướng dẫn tích hợp SignalR Chat Realtime - Frontend (Next.js)

> **Lưu ý:** Backend đã được implement xong. File này chỉ hướng dẫn tích hợp Frontend với Next.js App Router.

---

## 📋 Yêu cầu

- Next.js 15+ (App Router)
- TypeScript
- Redux Toolkit (đã có sẵn trong project)
- `@microsoft/signalr` package

---

## 💻 Frontend Integration (Next.js + TypeScript)

### Bước 1: Install SignalR Client

Mở terminal trong thư mục FE và chạy:

```bash
npm install @microsoft/signalr
# or
yarn add @microsoft/signalr
```

### Bước 2: Tạo Chat Hub Service

#### 2.1. Tạo types cho Chat

**File:** `src/types/chat.ts` (tạo file mới)

```typescript
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

export interface Conversation {
  id: number;
  vendor: {
    id: number;
    fullName: string;
    email: string;
    avatarUrl?: string;
  };
  customer: {
    id: number;
    fullName: string;
    email: string;
  };
  startedAt: string;
  lastMessageAt?: string;
}
```

#### 2.2. Tạo Chat Hub Connection Service

**File:** `src/lib/chatHub.ts` (tạo file mới)

```typescript
import * as signalR from '@microsoft/signalr';
import { ChatMessage } from '@/types/chat';

class ChatHubService {
  private connection: signalR.HubConnection | null = null;
  private messageHandlers: Array<(message: ChatMessage) => void> = [];

  /**
   * Khởi tạo kết nối đến ChatHub
   */
  async connect(accessToken: string): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      console.log('ChatHub already connected');
      return;
    }

    const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8386';

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${apiUrl}/hubs/chat`, {
        accessTokenFactory: () => accessToken,
        skipNegotiation: false, // Để SignalR tự negotiate protocol
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents,
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
      const pong = await this.connection.invoke<string>('Ping');
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
   * Kiểm tra trạng thái kết nối
   */
  isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected;
  }
}

// Export singleton instance
export const chatHubService = new ChatHubService();
```

### Bước 3: Tạo Custom Hook cho Chat

**File:** `src/hooks/useChatHub.ts` (tạo file mới)

```typescript
'use client';

import { useEffect } from 'react';
import { chatHubService } from '@/lib/chatHub';

/**
 * Hook để quản lý ChatHub connection
 * Tự động connect khi có token và disconnect khi unmount
 */
export function useChatHub(token?: string) {
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
  };
}
```

### Bước 4: Tích hợp vào Layout (Auto-connect)

Để ChatHub tự động connect khi user login, thêm vào layout của parent hoặc vendor:

**File:** `src/app/parent/layout.tsx` hoặc `src/app/vendor/layout.tsx` (hoặc tạo component riêng)

Thêm vào component layout:

```typescript
'use client';

import { useChatHub } from '@/hooks/useChatHub';
import { useAppSelector } from '@/redux/hook';

export default function ParentLayout({ children }: { children: React.ReactNode }) {
  // Lấy token từ Redux store hoặc localStorage
  const user = useAppSelector(state => state.auth.user);
  
  // Giả sử token được lưu trong user object hoặc localStorage
  const token = typeof window !== 'undefined' 
    ? localStorage.getItem('accessToken') 
    : undefined;

  // Auto-connect to ChatHub
  useChatHub(token);

  return (
    <div>
      {/* Your existing layout code */}
      {children}
    </div>
  );
}
```

**Lưu ý:** Nếu bạn lưu token ở chỗ khác (cookie, Redux store), hãy điều chỉnh cách lấy token cho phù hợp.

### Bước 5: Tạo Chat Component

#### 5.1. Chat Conversation Component

**File:** `src/components/chat/ChatConversation.tsx` (tạo file mới)

```typescript
'use client';

import React, { useState, useEffect, useCallback, useRef } from 'react';
import { useChatHub } from '@/hooks/useChatHub';
import { ChatMessage } from '@/types/chat';
import { useAppSelector } from '@/redux/hook';

interface ChatConversationProps {
  conversationId: number;
  vendorId: number;
  vendorName: string;
}

export default function ChatConversation({ 
  conversationId, 
  vendorId,
  vendorName 
}: ChatConversationProps) {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [newMessage, setNewMessage] = useState('');
  const [loading, setLoading] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  
  const user = useAppSelector(state => state.auth.user);
  const { onMessageReceived } = useChatHub();

  // Load messages history
  useEffect(() => {
    loadMessages();
  }, [conversationId]);

  const loadMessages = async () => {
    try {
      setLoading(true);
      const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8386';
      const token = localStorage.getItem('accessToken');
      
      const response = await fetch(
        `${apiUrl}/api/CustomerVendorConversation/${conversationId}/messages?page=1&pageSize=50`,
        {
          headers: {
            'Authorization': `Bearer ${token}`,
          },
        }
      );
      
      if (response.ok) {
        const result = await response.json();
        setMessages(result.data.data.reverse()); // Reverse để tin nhắn mới nhất ở dưới
      }
    } catch (error) {
      console.error('Error loading messages:', error);
    } finally {
      setLoading(false);
    }
  };

  // Xử lý khi nhận tin nhắn mới từ SignalR
  useEffect(() => {
    const unsubscribe = onMessageReceived((message) => {
      console.log('Received message:', message);
      
      // Thêm tin nhắn vào cuối list
      setMessages((prev) => [...prev, message]);
      
      // Play notification sound
      playNotificationSound();
      
      // Show browser notification nếu tab không focus
      if (document.hidden) {
        showBrowserNotification(message);
      }
      
      // Scroll to bottom
      scrollToBottom();
    });

    return unsubscribe;
  }, [onMessageReceived]);

  // Auto scroll to bottom khi có tin nhắn mới
  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const playNotificationSound = () => {
    try {
      const audio = new Audio('/sounds/message.mp3');
      audio.play().catch(console.error);
    } catch (error) {
      console.error('Error playing sound:', error);
    }
  };

  const showBrowserNotification = (message: ChatMessage) => {
    if ('Notification' in window && Notification.permission === 'granted') {
      new Notification('Tin nhắn mới từ ' + vendorName, {
        body: message.messageText,
        icon: '/logo.png',
        tag: `chat-${conversationId}`,
      });
    }
  };

  const handleSendMessage = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!newMessage.trim()) return;
    
    try {
      const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8386';
      const token = localStorage.getItem('accessToken');
      
      const formData = new FormData();
      formData.append('customerId', user?.id?.toString() || '');
      formData.append('vendorId', vendorId.toString());
      formData.append('messageText', newMessage);
      
      const response = await fetch(
        `${apiUrl}/api/CustomerVendorConversation/send-message`,
        {
          method: 'POST',
          headers: {
            'Authorization': `Bearer ${token}`,
          },
          body: formData,
        }
      );
      
      if (response.ok) {
        setNewMessage('');
        // Message sẽ được nhận qua SignalR, không cần thêm thủ công
      } else {
        const error = await response.text();
        alert('Lỗi gửi tin nhắn: ' + error);
      }
    } catch (error) {
      console.error('Error sending message:', error);
      alert('Có lỗi xảy ra khi gửi tin nhắn');
    }
  };

  return (
    <div className="flex flex-col h-[600px] border rounded-lg">
      {/* Header */}
      <div className="p-4 border-b bg-gray-50">
        <h3 className="font-semibold">{vendorName}</h3>
      </div>

      {/* Messages */}
      <div className="flex-1 overflow-y-auto p-4 space-y-3">
        {loading ? (
          <div className="text-center text-gray-500">Đang tải tin nhắn...</div>
        ) : (
          <>
            {messages.map((msg) => {
              const isOwnMessage = msg.senderType === 'Customer';
              
              return (
                <div
                  key={msg.id}
                  className={`flex ${isOwnMessage ? 'justify-end' : 'justify-start'}`}
                >
                  <div
                    className={`max-w-[70%] rounded-lg p-3 ${
                      isOwnMessage
                        ? 'bg-blue-500 text-white'
                        : 'bg-gray-200 text-gray-900'
                    }`}
                  >
                    <p className="text-sm">{msg.messageText}</p>
                    
                    {/* Images */}
                    {msg.images && msg.images.length > 0 && (
                      <div className="mt-2 space-y-2">
                        {msg.images.map((img) => (
                          <img
                            key={img.id}
                            src={img.imageUrl}
                            alt=""
                            className="rounded max-w-full"
                          />
                        ))}
                      </div>
                    )}
                    
                    <span className="text-xs opacity-70 mt-1 block">
                      {new Date(msg.createdAt).toLocaleTimeString('vi-VN', {
                        hour: '2-digit',
                        minute: '2-digit',
                      })}
                    </span>
                  </div>
                </div>
              );
            })}
            <div ref={messagesEndRef} />
          </>
        )}
      </div>

      {/* Input */}
      <form onSubmit={handleSendMessage} className="p-4 border-t">
        <div className="flex gap-2">
          <input
            type="text"
            value={newMessage}
            onChange={(e) => setNewMessage(e.target.value)}
            placeholder="Nhập tin nhắn..."
            className="flex-1 px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
          <button
            type="submit"
            disabled={!newMessage.trim()}
            className="px-6 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Gửi
          </button>
        </div>
      </form>
    </div>
  );
}
```

#### 5.2. Conversations List Component

**File:** `src/components/chat/ConversationsList.tsx` (tạo file mới)

```typescript
'use client';

import React, { useState, useEffect } from 'react';
import { Conversation } from '@/types/chat';

export default function ConversationsList({ 
  onSelectConversation 
}: { 
  onSelectConversation: (conversation: Conversation) => void 
}) {
  const [conversations, setConversations] = useState<Conversation[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadConversations();
  }, []);

  const loadConversations = async () => {
    try {
      setLoading(true);
      const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8386';
      const token = localStorage.getItem('accessToken');
      
      const response = await fetch(
        `${apiUrl}/api/CustomerVendorConversation/my-conversations?page=1&pageSize=20`,
        {
          headers: {
            'Authorization': `Bearer ${token}`,
          },
        }
      );
      
      if (response.ok) {
        const result = await response.json();
        setConversations(result.data.data);
      }
    } catch (error) {
      console.error('Error loading conversations:', error);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return <div className="p-4 text-center">Đang tải...</div>;
  }

  return (
    <div className="divide-y">
      {conversations.length === 0 ? (
        <div className="p-4 text-center text-gray-500">
          Chưa có cuộc hội thoại nào
        </div>
      ) : (
        conversations.map((conversation) => (
          <div
            key={conversation.id}
            onClick={() => onSelectConversation(conversation)}
            className="p-4 hover:bg-gray-50 cursor-pointer transition"
          >
            <div className="flex items-center gap-3">
              <div className="w-12 h-12 bg-blue-500 rounded-full flex items-center justify-center text-white font-semibold">
                {conversation.vendor.fullName.charAt(0).toUpperCase()}
              </div>
              <div className="flex-1">
                <h4 className="font-semibold">{conversation.vendor.fullName}</h4>
                <p className="text-sm text-gray-500">{conversation.vendor.email}</p>
              </div>
              {conversation.lastMessageAt && (
                <span className="text-xs text-gray-400">
                  {new Date(conversation.lastMessageAt).toLocaleDateString('vi-VN')}
                </span>
              )}
            </div>
          </div>
        ))
      )}
    </div>
  );
}
```

### Bước 6: Tạo Chat Page

**File:** `src/app/parent/messages/page.tsx` (hoặc tạo route mới)

```typescript
'use client';

import { useState } from 'react';
import ConversationsList from '@/components/chat/ConversationsList';
import ChatConversation from '@/components/chat/ChatConversation';
import { Conversation } from '@/types/chat';

export default function MessagesPage() {
  const [selectedConversation, setSelectedConversation] = useState<Conversation | null>(null);

  return (
    <div className="container mx-auto p-6">
      <h1 className="text-2xl font-bold mb-6">Tin nhắn</h1>
      
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {/* Conversations List */}
        <div className="md:col-span-1 border rounded-lg overflow-hidden">
          <div className="bg-gray-50 p-4 border-b">
            <h2 className="font-semibold">Cuộc hội thoại</h2>
          </div>
          <ConversationsList onSelectConversation={setSelectedConversation} />
        </div>

        {/* Chat Window */}
        <div className="md:col-span-2">
          {selectedConversation ? (
            <ChatConversation
              conversationId={selectedConversation.id}
              vendorId={selectedConversation.vendor.id}
              vendorName={selectedConversation.vendor.fullName}
            />
          ) : (
            <div className="border rounded-lg h-[600px] flex items-center justify-center text-gray-500">
              Chọn một cuộc hội thoại để bắt đầu
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
```

### Bước 7: Request Browser Notification Permission

Thêm vào root layout để request permission khi app load:

**File:** `src/app/layout.tsx`

Thêm vào component (trong useEffect hoặc khi user login):

```typescript
'use client';

import { useEffect } from 'react';

export default function RootLayout({ children }: { children: React.ReactNode }) {
  useEffect(() => {
    // Request notification permission khi app load
    if (typeof window !== 'undefined' && 'Notification' in window) {
      if (Notification.permission === 'default') {
        Notification.requestPermission().then((permission) => {
          console.log('Notification permission:', permission);
        });
      }
    }
  }, []);

  return (
    <html lang="vi">
      <body>{children}</body>
    </html>
  );
}
```

### Bước 8: Thêm Notification Sound (Optional)

Tạo file `public/sounds/message.mp3` (hoặc download file audio bất kỳ) để play sound khi nhận tin nhắn mới.

---

## 🧪 Testing Guide

### Test trên Browser

#### 1. Test Connection

Mở DevTools Console → Xem log:
```
✅ ChatHub connected successfully
🏓 Chat Hub - Pong from User 123 (Role: Customer)
```

#### 2. Test với 2 Users

- **Tab 1:** Login as Customer → Mở messages page
- **Tab 2:** Login as Vendor → Mở messages page
- Gửi tin nhắn từ Tab 1 → Tab 2 phải nhận realtime (không reload)

#### 3. Test Reconnection

- Ngắt internet → Bật lại
- Check console log: "ChatHub reconnected"
- Gửi tin nhắn → vẫn hoạt động

#### 4. Test Browser Notification

- Minimize tab chat → Gửi tin nhắn từ tab khác
- Phải nhận được browser notification

---

## 📚 Troubleshooting

### Lỗi thường gặp:

#### 1. "ChatHub connection error: 401 Unauthorized"
- **Nguyên nhân:** Token invalid hoặc expired
- **Giải pháp:** 
  - Check token trong localStorage: `localStorage.getItem('accessToken')`
  - Verify token chưa expired
  - Re-login để lấy token mới

#### 2. "ChatHub connection error: Failed to complete negotiation"
- **Nguyên nhân:** CORS chưa config đúng ở Backend
- **Giải pháp:** Check backend CORS config

#### 3. Tin nhắn không nhận được
- **Check:**
  - ChatHub có connected không? → Check console log
  - User có trong conversation không?
  - Network tab → Check WebSocket connection

#### 4. "Notification blocked"
- **Giải pháp:** 
  - Check browser settings → Allow notifications
  - User phải manually grant permission

---

## 🎯 Best Practices

### Performance:
- Lazy load chat history khi scroll up (infinite scroll)
- Limit số lượng messages render (virtualization)
- Debounce typing indicator

### UX:
- Show "Đang kết nối..." indicator khi connecting
- Show "Mất kết nối" warning khi disconnected
- Auto-retry connection khi lỗi
- Sound notification có thể tắt được

### Security:
- Validate token trước khi connect
- Không expose sensitive data trong console log (production)
- Sanitize message content trước khi render

---

## 📝 Summary

### Các file cần tạo:

1. ✅ `src/types/chat.ts` - Type definitions
2. ✅ `src/lib/chatHub.ts` - SignalR service
3. ✅ `src/hooks/useChatHub.ts` - React hook
4. ✅ `src/components/chat/ChatConversation.tsx` - Chat UI
5. ✅ `src/components/chat/ConversationsList.tsx` - Conversations list
6. ✅ `src/app/parent/messages/page.tsx` - Messages page

### Các file cần update:

1. ✅ Layout của parent/vendor - Add `useChatHub(token)`
2. ✅ Root layout - Request notification permission

### Testing checklist:

- ✅ npm install @microsoft/signalr
- ✅ Test connection với 2 users
- ✅ Test send/receive messages realtime
- ✅ Test reconnection
- ✅ Test browser notification

---

**🎉 Hoàn thành! Chat realtime đã sẵn sàng sử dụng.**

**Lưu ý quan trọng:**
- Đảm bảo `NEXT_PUBLIC_API_URL` được set đúng trong `.env.local`
- Token phải được lưu trong localStorage với key `accessToken`
- Nếu bạn lưu token ở chỗ khác, hãy điều chỉnh code cho phù hợp

Nếu cần hỗ trợ thêm, tham khảo:
- SignalR JS Client: https://learn.microsoft.com/en-us/javascript/api/@microsoft/signalr/
- Next.js Docs: https://nextjs.org/docs

