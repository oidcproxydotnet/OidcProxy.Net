using Microsoft.AspNetCore.Http;

namespace GoCloudNative.Bff.Authentication.Tests;

public class TestSession : ISession
{
    private Dictionary<string, byte[]> _values = new();

    public Task LoadAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task CommitAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public bool TryGetValue(string key, out byte[]? value)
    {
        var isSuccess = _values.TryGetValue(key, out byte[] result);
        value = result;
        return isSuccess;
    }

    public void Set(string key, byte[] value)
    {
        _values[key] = value;
    }

    public void Remove(string key) =>_values.Remove(key);

    public void Clear() => _values.Clear();
    
    
    public bool IsAvailable { get; }
    public string Id { get; }
    public IEnumerable<string> Keys { get; }
}