using ChatService.Entities;
using ChatService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatService
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService chatService;
        private readonly IGroupChatService groupChatService;
        private readonly ITypingIndicatorService typingIndicatorService;
        private readonly IUserConnectionService userConnectionService;

        public ChatHub(IChatService chatService, IGroupChatService groupChatService, ITypingIndicatorService typingIndicatorService,
            IUserConnectionService userConnectionService)
        {
            this.chatService = chatService;
            this.groupChatService = groupChatService;
            this.typingIndicatorService = typingIndicatorService;
            this.userConnectionService = userConnectionService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId))
            {
                // If user connected with no valid identifier 
                Context.Abort();
                return;
            }

            await userConnectionService.AddConnectionAsync(Context.ConnectionId, userId);

            // Join user's personal channel for receiving messages
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");

            // Join all chat groups this user is a member of
            var userGroups = await groupChatService.GetUserGroupsAsync(userId);
            foreach (var group in userGroups)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"group:{group.Id}");
            }

            await Clients.Others.SendAsync("UserOnline", userId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.UserIdentifier;

            await userConnectionService.RemoveConnectionAsync(Context.ConnectionId);

            // Check if user has other active connections
            bool isStillOnline = await userConnectionService.IsUserOnlineAsync(userId);

            if (!isStillOnline)
            {
                // If user has no other connections, notify others they went offline
                await Clients.Others.SendAsync("UserOffline", userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Direct messaging
        public async Task SendDirectMessage(string recipientId, string messageText)
        {
            var senderId = Context.UserIdentifier;

            var message = new ChatMessage
            {
                Id = Guid.NewGuid(),
                SenderId = senderId,
                RecipientId = recipientId,
                Text = messageText,
                IsRead = false,
                SentAt = DateTime.UtcNow,
            };

            await chatService.SaveMessageAsync(message);

            // Send to recipient through their user group
            await Clients.Group($"user:{recipientId}").SendAsync("ReceiveMessage", message);

            // Send back to sender for confirmation
            await Clients.Caller.SendAsync("MessageSent", message);
        }

        // Group messaging
        public async Task SendGroupMessage(string groupId, string messageText)
        {
            var senderId = Context.UserIdentifier;

            var members = await groupChatService.GetGroupMemberIdsAsync(groupId);
            if (!members.Contains(senderId))
            {
                throw new HubException("You are not a member of this group");
            }

            var message = new ChatMessage
            {
                Id = Guid.NewGuid(),
                SenderId = senderId,
                CompanyGroupId = groupId,
                Text = messageText,
                IsRead = false,
                SentAt = DateTime.UtcNow
            };

            await chatService.SaveMessageAsync(message);

            // Send to all members of the group
            await Clients.Group($"group:{groupId}").SendAsync("ReceiveGroupMessage", message);

        }


        public async Task JoinGroup(string groupId)
        {
            var userId = Context.UserIdentifier;

            // Check if user is in the group
            var members = await groupChatService.GetGroupMemberIdsAsync(groupId);
            if (!members.Contains(userId))
            {
                throw new HubException("You are not a member of this group.");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
        }

        // Typing indicators
        public async Task StartTyping(string recipientId, string groupId = null)
        {
            var userId = Context.UserIdentifier;

            if (recipientId != null)
            {
                // Direct chat typing indicator
                await typingIndicatorService.SetUserTypingAsync(userId, recipientId);
                await Clients.Group($"user:{recipientId}").SendAsync("UserTyping", userId, null);
            }
            else if (groupId != null)
            {
                // Group chat typing indicator
                await typingIndicatorService.SetUserTypingAsync(userId, null, groupId);
                await Clients.Group($"group:{groupId}").SendAsync("UserTyping", userId, groupId);
            }
        }


        // Get typing users for direct or group chat
        public async Task<List<string>> GetTypingUsers(string recipientId = null, string groupId = null)
        {
            if (recipientId != null)
            {
                return await typingIndicatorService.GetTypingUsersAsync(recipientId);
            }
            else if (groupId != null)
            {
                return await typingIndicatorService.GetTypingUsersAsync(null, groupId);
            }

            return new List<string>();
        }


        public async Task MarkMessageAsRead(Guid messageId)
        {
            await chatService.MarkAsReadAsync(messageId);
        }

        public async Task MarkAllMessagesAsRead(string chatWithUserId)
        {
            var userId = Context.UserIdentifier;
            await chatService.MarkAllAsReadAsync(userId, chatWithUserId);

            // Notify the other user their messages were read
            await Clients.Group($"user:{chatWithUserId}").SendAsync("MessagesRead", userId);

        }

        public async Task MarkAllGroupMessagesAsRead(string groupId)
        {
            var userId = Context.UserIdentifier;
            await chatService.MarkAllGroupMessagesAsReadAsync(userId, groupId);

            // Notify the group this user has read all messages
            await Clients.Group($"group:{groupId}").SendAsync("GroupMessagesRead", userId, groupId);


        }

        public async Task<bool> IsUserOnline(string userId)
        {
            return await userConnectionService.IsUserOnlineAsync(userId);
        }

        public async Task<int> GetUnreadCount(string chatWithUserId)
        {
            var userId = Context.UserIdentifier;
            return await chatService.GetUnreadCountAsync(userId, chatWithUserId);
        }

        public async Task<Dictionary<string, int>> GetAllUnreadCounts()
        {
            var userId = Context.UserIdentifier;
            return await chatService.GetAllUnreadCountsAsync(userId);
        }

        public async Task<int> GetGroupUnreadCount(string groupId)
        {

            var userId = Context.UserIdentifier;
            return await chatService.GetGroupUnreadCountAsync(userId, groupId);
        }
    }

}
