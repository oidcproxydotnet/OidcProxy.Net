using Microsoft.AspNetCore.Http;

namespace GoCloudNative.Bff.Authentication.Tests;

public class TestSession : ISession
{
    private readonly string _sessionId = Guid.NewGuid().ToString();
    
    private readonly Dictionary<string, byte[]> _values = new();
    
    public Task LoadAsync(CancellationToken cancellationToken = new ())
    {
        return Task.CompletedTask;
    }

    public Task CommitAsync(CancellationToken cancellationToken = new ())
    {
        return Task.CompletedTask;
    }

    public bool TryGetValue(string key, out byte[]? value)
    {
        var isSuccess = _values.TryGetValue(key, out byte[] result);
        value = result;
        return isSuccess;
    }

    public void Set(string key, byte[] value)
    {
        lock (_values)
        {
            _values[key] = value;
        }
    }

    public void Remove(string key)
    {
        lock (_values)
        {
            _values.Remove(key);
        }
    }

    public void Clear() => _values.Clear();
    
    
    public bool IsAvailable { get; }
    public string Id => _sessionId;
    public IEnumerable<string> Keys => _values.Keys;
}