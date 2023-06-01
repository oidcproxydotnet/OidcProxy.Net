namespace Bff;

public static class HttpClientExtensions
{
    public static async Task<string> GetAsStringAsync(this HttpClient httpClient, string uri)
    {
        var response = await httpClient.GetAsync(uri);
        return await response.Content.ReadAsStringAsync();
    }
}