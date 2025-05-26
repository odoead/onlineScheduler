using ChatService.Interfaces;
using StackExchange.Redis;

namespace ChatService.Services
{
    public class TypingIndicatorService : ITypingIndicatorService
    {
        private readonly IConnectionMultiplexer _redis;
        private const string TYPING_PREFIX = "typing:";
        private const int TYPING_EXPIRY_SECONDS = 5; // Typing indicator expires after 5 seconds

        public TypingIndicatorService(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task SetUserTypingAsync(string userId, string recipientId, string groupId = null)
        {
            var db = _redis.GetDatabase();
            string key;

            if (groupId != null)
            {
                key = $"{TYPING_PREFIX}group:{groupId}:{userId}";
            }
            else
            {
                key = $"{TYPING_PREFIX}{recipientId}:{userId}";
            }

            //set with expiry-automatically removes typing indicator after timeout expries
            await db.StringSetAsync(key, "1", TimeSpan.FromSeconds(TYPING_EXPIRY_SECONDS));
        }

        public async Task<bool> IsUserTypingAsync(string userId, string recipientId, string groupId = null)
        {
            var db = _redis.GetDatabase();
            string key;

            if (groupId != null)
            {
                key = $"{TYPING_PREFIX}group:{groupId}:{userId}";
            }
            else
            {
                key = $"{TYPING_PREFIX}{recipientId}:{userId}";
            }

            return await db.KeyExistsAsync(key);
        }

        public async Task<List<string>> GetTypingUsersAsync(string recipientId, string groupId = null)
        {
            var db = _redis.GetDatabase();
            string pattern;

            if (groupId != null)
            {
                pattern = $"{TYPING_PREFIX}group:{groupId}:*";
            }
            else
            {
                pattern = $"{TYPING_PREFIX}{recipientId}:*";
            }

            var typingUsers = new List<string>();

            // Use server-side scanning for efficiency
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: pattern);

            foreach (var key in keys)
            {
                string keyString = key.ToString();
                string userId;
                if (groupId != null)
                {
                    userId = keyString.Substring($"{TYPING_PREFIX}group:{groupId}:".Length);
                }
                else
                {
                    userId = keyString.Substring($"{TYPING_PREFIX}{recipientId}:".Length);
                }

                typingUsers.Add(userId);
            }

            return typingUsers;
        }
    }


}
