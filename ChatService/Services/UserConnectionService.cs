using ChatService.Interfaces;
using StackExchange.Redis;

namespace ChatService.Services
{
    // Redis-based implementation for UserConnectionService
    public class UserConnectionService : IUserConnectionService
    {
        private readonly IConnectionMultiplexer redis;
        private const string CONNECTION_PREFIX = "connection:";
        private const string USER_PREFIX = "user:";

        public UserConnectionService(IConnectionMultiplexer redis)
        {
            this.redis = redis;
        }

        public async Task AddConnectionAsync(string connectionId, string userId)
        {
            var db = redis.GetDatabase();

            // Map connection ID to user ID
            await db.StringSetAsync($"{CONNECTION_PREFIX}{connectionId}", userId);

            // Add connection ID to user's set of connections
            await db.SetAddAsync($"{USER_PREFIX}{userId}", connectionId);

            // Set last active timestamp
            await db.HashSetAsync($"{USER_PREFIX}{userId}:info", "lastActive", DateTime.UtcNow.Ticks.ToString());
        }

        public async Task RemoveConnectionAsync(string connectionId)
        {
            var db = redis.GetDatabase();
            string userId = await db.StringGetAsync($"{CONNECTION_PREFIX}{connectionId}");

            if (!string.IsNullOrEmpty(userId))
            {
                // Remove connection from user's set
                await db.SetRemoveAsync($"{USER_PREFIX}{userId}", connectionId);

                // Remove connection mapping
                await db.KeyDeleteAsync($"{CONNECTION_PREFIX}{connectionId}");

                // Update last seen if this was the last connection
                long remainingConnections = await db.SetLengthAsync($"{USER_PREFIX}{userId}");
                if (remainingConnections == 0)
                {
                    await db.HashSetAsync($"{USER_PREFIX}{userId}:info", "lastSeen", DateTime.UtcNow.Ticks.ToString());
                }
            }
        }

        public async Task<bool> IsUserOnlineAsync(string userId)
        {
            var db = redis.GetDatabase();
            long connectionCount = await db.SetLengthAsync($"{USER_PREFIX}{userId}");
            return connectionCount > 0;
        }

        public async Task<List<string>> GetUserConnectionsAsync(string userId)
        {
            var db = redis.GetDatabase();
            var connections = await db.SetMembersAsync($"{USER_PREFIX}{userId}");
            return connections.Select(c => c.ToString()).ToList();
        }

        public async Task<List<string>> GetOnlineUsersAsync(List<string> userIds)
        {
            var db = redis.GetDatabase();
            var result = new List<string>();

            foreach (var userId in userIds)
            {
                long connectionCount = await db.SetLengthAsync($"{USER_PREFIX}{userId}");
                if (connectionCount > 0)
                {
                    result.Add(userId);
                }
            }

            return result;
        }

        public async Task<DateTime?> GetLastActiveTimeAsync(string userId)
        {
            var db = redis.GetDatabase();

            if (await IsUserOnlineAsync(userId))
            {
                string lastActiveTicks = await db.HashGetAsync($"{USER_PREFIX}{userId}:info", "lastActive");
                if (!string.IsNullOrEmpty(lastActiveTicks) && long.TryParse(lastActiveTicks, out long ticks))
                {
                    return new DateTime(ticks);
                }
            }
            else
            {
                string lastSeenTicks = await db.HashGetAsync($"{USER_PREFIX}{userId}:info", "lastSeen");
                if (!string.IsNullOrEmpty(lastSeenTicks) && long.TryParse(lastSeenTicks, out long ticks))
                {
                    return new DateTime(ticks);
                }
            }

            return null;
        }

        public async Task UpdateUserActivityAsync(string userId)
        {
            var db = redis.GetDatabase();
            await db.HashSetAsync($"{USER_PREFIX}{userId}:info", "lastActive", DateTime.UtcNow.Ticks.ToString());
        }
    }
}
