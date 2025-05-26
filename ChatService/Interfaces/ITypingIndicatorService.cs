namespace ChatService.Interfaces
{
    public interface ITypingIndicatorService
    {
        Task SetUserTypingAsync(string userId, string recipientId, string groupId = null);
        Task<bool> IsUserTypingAsync(string userId, string recipientId, string groupId = null);
        Task<List<string>> GetTypingUsersAsync(string recipientId, string groupId = null);
    }
}
