using System;
using System.Collections.Concurrent;

namespace TheCloud
{
    public class RateLimiter
    {
        private readonly TimeSpan _cooldown;
        private readonly ConcurrentDictionary<ulong, DateTime> _userTimestamps;

        public RateLimiter(TimeSpan cooldown)
        {
            _cooldown = cooldown;
            _userTimestamps = new ConcurrentDictionary<ulong, DateTime>();
        }

        public bool CanRespond(ulong userId)
        {
            var now = DateTime.UtcNow;

            if (_userTimestamps.TryGetValue(userId, out var lastTime))
            {
                if (now - lastTime < _cooldown)
                    return false;
            }

            _userTimestamps[userId] = now;
            return true;
        }
    }
}