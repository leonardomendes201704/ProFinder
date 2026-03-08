using System.Collections.Concurrent;
using System.Diagnostics;

namespace ProFinder.Infrastructure.Services;

public class CrawlerProcessRegistry
{
    private readonly ConcurrentDictionary<int, Process> _processes = new();

    public void Register(int runId, Process process)
    {
        _processes[runId] = process;
    }

    public bool TryGet(int runId, out Process? process)
    {
        var found = _processes.TryGetValue(runId, out var current);
        process = current;
        return found;
    }

    public void Remove(int runId)
    {
        _processes.TryRemove(runId, out _);
    }

    public IReadOnlyList<KeyValuePair<int, Process>> Snapshot()
    {
        return _processes.ToArray();
    }
}
