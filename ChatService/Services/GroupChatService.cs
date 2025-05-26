using ChatService.DB;
using ChatService.Entities;
using ChatService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Services
{
    public class GroupChatService : IGroupChatService
    {
        private readonly Context dbContext;

        public GroupChatService(Context dbContext)
        {
            dbContext = dbContext;
        }

        public async Task<ChatGroup> CreateGroupAsync(string name, string description, string creatorId)
        {
            var group = new ChatGroup
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Groups.Add(group);

            // Add creator as admin
            var creatorMember = new ChatGroupMember
            {
                Id = Guid.NewGuid(),
                UserId = creatorId,
                GroupId = group.Id,
                IsAdmin = true,
                JoinedAt = DateTime.UtcNow
            };

            dbContext.GroupMembers.Add(creatorMember);
            await dbContext.SaveChangesAsync();

            return group;
        }

        public async Task<ChatGroup> GetGroupAsync(string groupId)
        {
            return await dbContext.Groups.Include(g => g.Members).FirstOrDefaultAsync(g => g.Id == groupId);
        }

        public async Task AddMemberToGroupAsync(string groupId, string userId, bool isAdmin = false)
        {
            var existingMember = await dbContext.GroupMembers.FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);
            if (existingMember != null)
            {
                return;
            }

            var member = new ChatGroupMember
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                GroupId = groupId,
                IsAdmin = isAdmin,
                JoinedAt = DateTime.UtcNow
            };

            dbContext.GroupMembers.Add(member);
            await dbContext.SaveChangesAsync();
        }

        public async Task RemoveMemberFromGroupAsync(string groupId, string userId)
        {
            var member = await dbContext.GroupMembers.FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);
            if (member != null)
            {
                dbContext.GroupMembers.Remove(member);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> IsUserGroupAdminAsync(string groupId, string userId)
        {
            var member = await dbContext.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);

            return member?.IsAdmin ?? false;
        }

        public async Task<List<ChatGroup>> GetUserGroupsAsync(string userId)
        {
            return await dbContext.GroupMembers
                .Where(m => m.UserId == userId).Join(
                    dbContext.Groups,
                    member => member.GroupId,
                    group => group.Id,
                    (member, group) => group).ToListAsync();
        }

        public async Task<List<string>> GetGroupMemberIdsAsync(string groupId)
        {
            return await dbContext.GroupMembers.Where(m => m.GroupId == groupId).Select(m => m.UserId).ToListAsync();
        }
    }
}
