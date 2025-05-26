using ChatService.Entities;

namespace ChatService.Interfaces
{
    public interface IGroupChatService
    {
        Task<ChatGroup> CreateGroupAsync(string name, string description, string creatorId);
        Task<ChatGroup> GetGroupAsync(string groupId);
        Task AddMemberToGroupAsync(string groupId, string userId, bool isAdmin = false);
        Task RemoveMemberFromGroupAsync(string groupId, string userId);
        Task<List<ChatGroup>> GetUserGroupsAsync(string userId);
        Task<List<string>> GetGroupMemberIdsAsync(string groupId);
        Task<bool> IsUserGroupAdminAsync(string groupId, string userId);

    }
}
