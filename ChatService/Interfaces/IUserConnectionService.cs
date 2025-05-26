namespace ChatService.Interfaces
{
    public interface IUserConnectionService
    {
        Task AddConnectionAsync(string connectionId, string userId);
        Task RemoveConnectionAsync(string connectionId);
        Task<bool> IsUserOnlineAsync(string userId);
        Task<List<string>> GetUserConnectionsAsync(string userId);
        Task<List<string>> GetOnlineUsersAsync(List<string> userIds);
        Task<DateTime?> GetLastActiveTimeAsync(string userId);
        Task UpdateUserActivityAsync(string userId);

    }
}
