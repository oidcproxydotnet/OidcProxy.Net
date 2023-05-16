namespace TheCloudNativeWebApp.Bff.Authentication.OpenIdConnect;

public static class HttpClientExtensions
{
    public static void SetBaseAddressIfNotSet(this HttpClient httpClient, string baseAddress)
    {
        if(httpClient.BaseAddress != null) 
        {
            return;
        }

        httpClient.BaseAddress = new Uri(baseAddress);
    }
}