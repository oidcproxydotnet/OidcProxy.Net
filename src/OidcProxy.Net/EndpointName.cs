namespace OidcProxy.Net;

internal class EndpointName(string value)
{
    public override string ToString() => $"/{value}";
}