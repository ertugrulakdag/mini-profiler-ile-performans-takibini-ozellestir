using StackExchange.Profiling.Storage;
using StackExchange.Profiling;
using System.Collections.Concurrent;

namespace WebAppForMiniProfiler.Helpers
{
    public class CustomMiniProfilerMemoryStorage : IAsyncStorage
    {
        private readonly ConcurrentDictionary<string, MiniProfiler> _storage = new();
        private readonly TimeSpan _cacheDuration;
        private readonly int _maxEntries;
        public CustomMiniProfilerMemoryStorage(TimeSpan cacheDuration, int maxEntries = 100)
        {
            _cacheDuration = cacheDuration;
            _maxEntries = maxEntries;
        }
        public Task SaveAsync(MiniProfiler profiler)
        {
            Save(profiler);
            return Task.CompletedTask;
        }
        public void Save(MiniProfiler profiler)
        {
            var key = profiler.Id.ToString();
            if (_storage.Count >= _maxEntries)
            {
                var oldest = _storage.OrderBy(kv => kv.Value.Started).FirstOrDefault();
                if (!string.IsNullOrEmpty(oldest.Key))
                {
                    _storage.TryRemove(oldest.Key, out _);
                }
            }
            _storage[key] = profiler;
        }
        public Task<MiniProfiler?> LoadAsync(Guid id)
        {
            return Task.FromResult(Load(id));
        }
        public MiniProfiler? Load(Guid id)
        {
            var key = id.ToString();
            if (_storage.TryGetValue(key, out var profiler))
            {
                var elapsed = DateTime.UtcNow - profiler.Started;
                if (elapsed <= _cacheDuration)
                {
                    return profiler;
                }

                _storage.TryRemove(key, out _);
            }

            return null;
        }
        public Task<List<Guid>> ListAsync(DateTime start)
        {
            var result = _storage.Values
                .Where(p => p.Started >= start)
                .OrderByDescending(p => p.Started)
                .Select(p => p.Id)
                .ToList();

            return Task.FromResult(result);
        }
        public Task<IEnumerable<Guid>> ListAsync(int maxResults, DateTime? start = null, DateTime? finish = null, ListResultsOrder orderBy = ListResultsOrder.Descending)
        {
            var result = _storage.Values.AsEnumerable();

            if (start.HasValue)
                result = result.Where(p => p.Started >= start.Value);
            if (finish.HasValue)
                result = result.Where(p => p.Started <= finish.Value);

            result = orderBy == ListResultsOrder.Ascending
                ? result.OrderBy(p => p.Started)
                : result.OrderByDescending(p => p.Started);

            return Task.FromResult(result.Take(maxResults).Select(p => p.Id));
        }
        public Task<(List<MiniProfilerListItemDto> items, int totalCount)> CustomListPagedSlimAsync(int skip, int take, string? search, DateTime? start = null, DateTime? finish = null, string? sortColumn = null)
        {
            var query = _storage.Values.AsEnumerable();
            if (start.HasValue)
                query = query.Where(p => p.Started >= start.Value);

            if (finish.HasValue)
                query = query.Where(p => p.Started <= finish.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowered = search.Trim().ToLowerInvariant();
                query = query.Where(p =>
                    (!string.IsNullOrEmpty(p.Name) && p.Name.ToLowerInvariant().Contains(lowered)) ||
                    (!string.IsNullOrEmpty(p.User) && p.User.ToLowerInvariant().Contains(lowered)) ||
                    (!string.IsNullOrEmpty(p.MachineName) && p.MachineName.ToLowerInvariant().Contains(lowered))
                );
            }

            var (columnIndex, direction) = ParseSortColumn(sortColumn);

            query = (columnIndex, direction) switch
            {
                (1, ListResultsOrder.Ascending) => query.OrderBy(p => p.Name),
                (1, ListResultsOrder.Descending) => query.OrderByDescending(p => p.Name),

                (2, ListResultsOrder.Ascending) => query.OrderBy(p => p.User),
                (2, ListResultsOrder.Descending) => query.OrderByDescending(p => p.User),

                (3, ListResultsOrder.Ascending) => query.OrderBy(p => p.MachineName),
                (3, ListResultsOrder.Descending) => query.OrderByDescending(p => p.MachineName),

                (4, ListResultsOrder.Ascending) => query.OrderBy(p => p.Started),
                (4, ListResultsOrder.Descending) => query.OrderByDescending(p => p.Started),

                (5, ListResultsOrder.Ascending) => query.OrderBy(p => p.DurationMilliseconds),
                (5, ListResultsOrder.Descending) => query.OrderByDescending(p => p.DurationMilliseconds),

                _ => query.OrderByDescending(p => p.Started)
            };

            var totalCount = query.Count();

            var items = query
                .Skip(skip)
                .Take(take)
                .Select(p => new MiniProfilerListItemDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Started = p.Started,
                    HasUserViewed = p.HasUserViewed,
                    MachineName = p.MachineName,
                    User = p.User,
                    DurationMilliseconds = p.DurationMilliseconds
                })
                .ToList();

            return Task.FromResult((items, totalCount));
        }
        private static (int columnIndex, ListResultsOrder direction) ParseSortColumn(string? sortColumn)
        {
            if (string.IsNullOrWhiteSpace(sortColumn))
                return (-1, ListResultsOrder.Descending);

            if (int.TryParse(sortColumn, out var value))
            {
                var absIndex = Math.Abs(value);
                var direction = value > 0 ? ListResultsOrder.Ascending : ListResultsOrder.Descending;
                return (absIndex, direction);
            }

            return (-1, ListResultsOrder.Descending);
        }
        public IEnumerable<Guid> List(int maxResults, DateTime? start = null, DateTime? finish = null, ListResultsOrder orderBy = ListResultsOrder.Descending)
        {
            var result = _storage.Values.AsEnumerable();

            if (start.HasValue)
                result = result.Where(p => p.Started >= start.Value);
            if (finish.HasValue)
                result = result.Where(p => p.Started <= finish.Value);

            result = orderBy == ListResultsOrder.Ascending
                ? result.OrderBy(p => p.Started)
                : result.OrderByDescending(p => p.Started);

            return result.Take(maxResults).Select(p => p.Id);
        }
        public Task SetUnviewedAsync(string? user, Guid id) => Task.CompletedTask;
        public Task SetViewedAsync(string? user, Guid id) => Task.CompletedTask;
        public Task<List<Guid>> GetUnviewedIdsAsync(string? user) => Task.FromResult(new List<Guid>());
        public void SetUnviewed(string? user, Guid id) { }
        public void SetViewed(string? user, Guid id) { }
        public List<Guid> GetUnviewedIds(string? user) => new();
        public class MiniProfilerListItemDto
        {
            public Guid Id { get; set; }
            public string? Name { get; set; }
            public DateTime Started { get; set; }
            public bool HasUserViewed { get; set; }
            public string? MachineName { get; set; }
            public string? User { get; set; }
            public decimal DurationMilliseconds { get; set; }
        }
    }

}
