namespace EventSub.Services;

class IdPersistance {

    private Dictionary<string, Timer> _timers = new();

    private readonly object _lock = new();

    private TimeSpan _ttl;

    public IdPersistance(TimeSpan ttl) {
        _ttl = ttl;
    }

    ~IdPersistance() {
        foreach (var timer in _timers) {
            timer.Value.Dispose();
        }
    }

    public void Add(string id) {
        lock (_lock) {
            _timers.Add(
                id,
                new Timer(
                    (_state) => {
                        _timers.Remove(id);
                    },
                    null,
                    _ttl,
                    Timeout.InfiniteTimeSpan
                )
            );
        }
    }

    public bool Contains(string id) {
        lock (_lock) {
            return _timers.ContainsKey(id);
        }
    }
}
