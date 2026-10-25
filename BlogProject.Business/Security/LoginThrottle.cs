using System.Collections.Concurrent;

namespace BlogProject.Business.Security
{
    /// <summary>
    /// In-memory brute-force guard: after <c>maxFailures</c> failed logins for the same key
    /// the key is locked for <c>lockDuration</c>. State is per process and resets on restart.
    /// </summary>
    public class LoginThrottle
    {
        private readonly int _maxFailures;
        private readonly TimeSpan _lockDuration;
        private readonly TimeProvider _time;
        private readonly ConcurrentDictionary<string, (int Failures, DateTimeOffset? LockedUntil, DateTimeOffset Last)> _state = new();

        public LoginThrottle(int maxFailures = 5, TimeSpan? lockDuration = null, TimeProvider? time = null)
        {
            _maxFailures = maxFailures;
            _lockDuration = lockDuration ?? TimeSpan.FromMinutes(15);
            _time = time ?? TimeProvider.System;
        }

        public static string Key(string? email, string? ip) =>
            $"{(email ?? "").Trim().ToLowerInvariant()}|{ip}";

        public bool IsLocked(string key)
        {
            if (!_state.TryGetValue(key, out var s) || s.LockedUntil is null) return false;
            if (s.LockedUntil > _time.GetUtcNow()) return true;
            _state.TryRemove(key, out _);
            return false;
        }

        public void RegisterFailure(string key)
        {
            var now = _time.GetUtcNow();
            _state.AddOrUpdate(key,
                _ => (1, _maxFailures <= 1 ? now + _lockDuration : null, now),
                (_, s) =>
                {
                    var failures = s.Failures + 1;
                    return (failures, failures >= _maxFailures ? now + _lockDuration : s.LockedUntil, now);
                });
            Prune(now);
        }

        public void Reset(string key) => _state.TryRemove(key, out _);

        private void Prune(DateTimeOffset now)
        {
            if (_state.Count < 1000) return;
            foreach (var kv in _state)
                if (now - kv.Value.Last > _lockDuration) _state.TryRemove(kv.Key, out _);
        }
    }
}
