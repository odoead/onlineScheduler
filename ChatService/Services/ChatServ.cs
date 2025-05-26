using ChatService.DB;
using ChatService.Entities;
using ChatService.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Shared.Events.Chat;

namespace ChatService.Services
{
    public class ChatServ : IChatService
    {
        private readonly Context dbcontext;
        private readonly IDistributedCache redisCache;
        private readonly IPublishEndpoint publishEndpoint;

        public ChatServ(Context dbContext, IDistributedCache redisCache, IPublishEndpoint eventPublisher)
        {
            dbcontext = dbContext;
            this.redisCache = redisCache;
            this.publishEndpoint = eventPublisher;
        }

        public async Task<ChatMessage> SaveMessageAsync(ChatMessage message)
        {
            dbcontext.Messages.Add(message);
            await dbcontext.SaveChangesAsync();

            // Update unread count in Redis
            if (!string.IsNullOrEmpty(message.RecipientId))
            {
                // For direct messages
                await IncrementUnreadCountAsync(message.RecipientId, message.SenderId);

                await publishEndpoint.Publish(new ChatMessageSent
                {
                    MessageId = message.Id,
                    SenderId = message.SenderId,
                    RecipientId = message.RecipientId,
                    SentAt = message.SentAt
                }
                    );
            }
            else if (!string.IsNullOrEmpty(message.CompanyGroupId))
            {
                // For group messages
                var members = await dbcontext.GroupMembers
                    .Where(m => m.GroupId == message.CompanyGroupId && m.UserId != message.SenderId)
                    .Select(m => m.UserId)
                    .ToListAsync();

                foreach (var memberId in members)
                {
                    await IncrementUnreadCountAsync(memberId, null, message.CompanyGroupId);
                }

                await publishEndpoint.Publish(new GroupChatMessageSent
                {
                    MessageId = message.Id,
                    SenderId = message.SenderId,
                    GroupId = message.CompanyGroupId,
                    SentAt = message.SentAt
                }
                   );
            }

            return message;
        }

        public async Task<List<ChatMessage>> GetChatHistoryAsync(string userId1, string userId2, int limit = 50, int offset = 0)
        {
            return await dbcontext.Messages.Where(m => (m.SenderId == userId1 && m.RecipientId == userId2) || (m.SenderId == userId2 && m.RecipientId == userId1))
                .OrderByDescending(m => m.SentAt).Skip(offset).Take(limit).ToListAsync();
        }

        public async Task<List<ChatMessage>> GetGroupChatHistoryAsync(string groupId, int limit = 50, int offset = 0)
        {
            return await dbcontext.Messages.Where(m => m.CompanyGroupId == groupId).OrderByDescending(m => m.SentAt).Skip(offset).Take(limit)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(Guid messageId)
        {
            var message = await dbcontext.Messages.FindAsync(messageId);
            if (message != null && !message.IsRead)
            {
                message.IsRead = true;
                message.ReadAt = DateTime.UtcNow;
                await dbcontext.SaveChangesAsync();

                // Decrement unread count in Redis
                if (!string.IsNullOrEmpty(message.RecipientId))
                {
                    await DecrementUnreadCountAsync(message.RecipientId, message.SenderId);
                }
            }
        }

        public async Task MarkAllAsReadAsync(string recipientId, string senderId)
        {
            var unreadMessages = await dbcontext.Messages.Where(m => m.RecipientId == recipientId && m.SenderId == senderId && !m.IsRead)
                .ToListAsync();

            if (unreadMessages.Any())
            {
                var now = DateTime.UtcNow;
                foreach (var message in unreadMessages)
                {
                    message.IsRead = true;
                    message.ReadAt = now;
                }

                await dbcontext.SaveChangesAsync();
                await ResetUnreadCountAsync(recipientId, senderId);
            }
        }

        public async Task MarkAllGroupMessagesAsReadAsync(string userId, string groupId)
        {
            var unreadGroupMessages = await dbcontext.Messages.Where(m => m.CompanyGroupId == groupId && m.SenderId != userId && !m.IsRead)
                .ToListAsync();

            if (unreadGroupMessages.Any())
            {
                var now = DateTime.UtcNow;
                foreach (var message in unreadGroupMessages)
                {
                    message.IsRead = true;
                    message.ReadAt = now;
                }

                await dbcontext.SaveChangesAsync();
                await ResetUnreadCountAsync(userId, null, groupId);
            }
        }

        public async Task<int> GetUnreadCountAsync(string userId, string chatWithUserId)
        {
            // Try to get from Redis first
            string redisKey = $"unread:{userId}:{chatWithUserId}";
            string cachedCount = await redisCache.GetStringAsync(redisKey);

            if (!string.IsNullOrEmpty(cachedCount) && int.TryParse(cachedCount, out int count))
            {
                return count;
            }

            // If not in Redis, calculate from database
            count = await dbcontext.Messages.CountAsync(m => m.RecipientId == userId && m.SenderId == chatWithUserId && !m.IsRead);

            // Cache to Redis
            await redisCache.SetStringAsync(redisKey, count.ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });

            return count;
        }

        public async Task<int> GetGroupUnreadCountAsync(string userId, string groupId)
        {
            // Try to get from Redis first
            string redisKey = $"unread:group:{userId}:{groupId}";
            string cachedCount = await redisCache.GetStringAsync(redisKey);

            if (!string.IsNullOrEmpty(cachedCount) && int.TryParse(cachedCount, out int count))
            {
                return count;
            }

            count = await dbcontext.Messages.CountAsync(m => m.CompanyGroupId == groupId && m.SenderId != userId && !m.IsRead);

            await redisCache.SetStringAsync(redisKey, count.ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });

            return count;
        }

        public async Task<Dictionary<string, int>> GetAllUnreadCountsAsync(string userId)
        {
            var result = new Dictionary<string, int>();

            // Get direct chat unread counts
            var directChats = await dbcontext.Messages.Where(m => m.RecipientId == userId && !m.IsRead).GroupBy(m => m.SenderId)
                .Select(g => new { SenderId = g.Key, Count = g.Count() }).ToListAsync();

            foreach (var chat in directChats)
            {
                result[chat.SenderId] = chat.Count;
            }

            // Get group chat unread counts
            var userGroups = await dbcontext.GroupMembers.Where(gm => gm.UserId == userId).Select(gm => gm.GroupId).ToListAsync();
            foreach (var groupId in userGroups)
            {
                int unreadCount = await GetGroupUnreadCountAsync(userId, groupId);
                result[$"group:{groupId}"] = unreadCount;
            }

            return result;
        }

        private async Task IncrementUnreadCountAsync(string userId, string chatWithUserId, string groupId = null)
        {
            string redisKey;

            if (groupId != null)
            {
                redisKey = $"unread:group:{userId}:{groupId}";
            }
            else
            {
                redisKey = $"unread:{userId}:{chatWithUserId}";
            }

            string currentCountString = await redisCache.GetStringAsync(redisKey);
            int currentCount = 0;

            if (!string.IsNullOrEmpty(currentCountString))
            {
                int.TryParse(currentCountString, out currentCount);
            }

            currentCount++;
            await redisCache.SetStringAsync(redisKey, currentCount.ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
            });
        }

        private async Task DecrementUnreadCountAsync(string userId, string chatWithUserId, string groupId = null)
        {
            string redisKey;

            if (groupId != null)
            {
                redisKey = $"unread:group:{userId}:{groupId}";
            }
            else
            {
                redisKey = $"unread:{userId}:{chatWithUserId}";
            }

            string currentCountStr = await redisCache.GetStringAsync(redisKey);
            int currentCount = 0;

            if (!string.IsNullOrEmpty(currentCountStr))
            {
                int.TryParse(currentCountStr, out currentCount);
            }

            //minus but not below 0
            currentCount = Math.Max(0, currentCount - 1);
            await redisCache.SetStringAsync(redisKey, currentCount.ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
            });
        }

        private async Task ResetUnreadCountAsync(string userId, string chatWithUserId, string groupId = null)
        {
            string redisKey;

            if (groupId != null)
            {
                redisKey = $"unread:group:{userId}:{groupId}";
            }
            else
            {
                redisKey = $"unread:{userId}:{chatWithUserId}";
            }

            await redisCache.SetStringAsync(redisKey, "0", new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
            });
        }
    }
}
