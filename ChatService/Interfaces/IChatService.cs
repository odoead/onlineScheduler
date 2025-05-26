using ChatService.Entities;

namespace ChatService.Interfaces
{
    public interface IChatService
    {
        Task<ChatMessage> SaveMessageAsync(ChatMessage message);
        Task<List<ChatMessage>> GetChatHistoryAsync(string userId1, string userId2, int limit = 50, int offset = 0);
        Task<List<ChatMessage>> GetGroupChatHistoryAsync(string groupId, int limit = 50, int offset = 0);
        Task MarkAsReadAsync(Guid messageId);
        Task MarkAllAsReadAsync(string recipientId, string senderId);
        Task<int> GetUnreadCountAsync(string userId, string chatWithUserId);
        Task<Dictionary<string, int>> GetAllUnreadCountsAsync(string userId);
        Task<int> GetGroupUnreadCountAsync(string userId, string groupId);
        Task MarkAllGroupMessagesAsReadAsync(string userId, string groupId);
    }
}
