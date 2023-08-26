using FluentAssertions;
using GoCloudNative.Bff.Authentication.ModuleInitializers;
using Newtonsoft.Json;

namespace GoCloudNative.Bff.Authentication.OpenIdConnect.Tests;

public class OpenIdConnectBffConfigurationTests
{
    private readonly OidcBffConfig? _deserializedObject;
    
    public OpenIdConnectBffConfigurationTests()
    {
        var json = File.ReadAllText("files/Configuration.json");
        _deserializedObject = JsonConvert.DeserializeObject<OidcBffConfig>(json);
    }
    
    [Fact]
    public void ItShouldReadOidcProperties()
    {
        _deserializedObject?.ClientId.Should().NotBeNullOrEmpty();
        _deserializedObject?.ClientSecret.Should().NotBeNullOrEmpty();
        _deserializedObject?.Authority.Should().NotBeNullOrEmpty();
        _deserializedObject?.DiscoveryEndpoint.Should().NotBeNullOrEmpty();
        _deserializedObject?.Scopes.Should().NotBeNullOrEmpty();
        _deserializedObject?.PostLogoutRedirectEndpoint.Should().NotBeNullOrEmpty();
        _deserializedObject?.EndpointName.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public void ItShouldReadBffProperties()
    {
        _deserializedObject?.ErrorPage.Should().NotBeNullOrEmpty();
        _deserializedObject?.LandingPage.Should().NotBeNullOrEmpty();
        _deserializedObject?.CustomHostName.Should().NotBeNull();
        _deserializedObject?.SessionCookieName.Should().NotBeNullOrEmpty();
        _deserializedObject?.SessionIdleTimeout.Should().NotBe(TimeSpan.Zero);
    }
    
    [Fact]
    public void ItShouldReadClusters()
    {
        var clusters = _deserializedObject?.ReverseProxy.Clusters.ToClusterConfig();
        clusters.Should().NotBeEmpty();

        var cluster = clusters.FirstOrDefault();
        cluster.ClusterId.Should().Be("apicluster");
        cluster.Destinations.Should().NotBeEmpty();
    }

    [Fact]
    public void ItShouldReadRoutes()
    {
        var routes = _deserializedObject?.ReverseProxy.Routes.ToRouteConfig();
        routes.Should().NotBeEmpty();

        var route = routes.FirstOrDefault();
        route.RouteId.Should().Be("apiroute");
        route.Match.Path.Should().Be("/api/{*any}");
        route.ClusterId.Should().Be("apicluster");
    }
}