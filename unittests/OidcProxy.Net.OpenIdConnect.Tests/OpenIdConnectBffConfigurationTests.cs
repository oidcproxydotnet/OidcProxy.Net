using FluentAssertions;
using OidcProxy.Net.ModuleInitializers;
using Newtonsoft.Json;
using OidcProxy.Net.OpenIdConnect;

namespace OidcProxy.Net.OpenIdConnect.Tests;

public class OpenIdConnectBffConfigurationTests
{
    private const string json = @"{
        ""Oidc"": {
            ""ClientId"": ""clientId"",
            ""ClientSecret"": ""clientSecret"",
            ""Authority"": ""https://test.com"",
            ""Scopes"": [
            ""openid"",
            ""profile"",
            ""offline_access""
            ],
            ""DiscoveryEndpoint"": ""https://disco.com/.well-known/openid-configuration"",
            ""DisablePushedAuthorization"": true
        },
        ""ErrorPage"": ""/error.aspx"",
        ""LandingPage"": ""/welcome.aspx"",
        ""RoleClaim"": ""sub"",
        ""NameClaim"": ""role"",
        ""Hostname"": ""https://oidcproxy.foo.bar"",
        ""CustomHostName"": ""www.foobar.org"",
        ""CookieName"": ""bff.custom.cookie"",
        ""SessionIdleTimeout"": ""00:30:00"",
        ""PostLogoutRedirectEndpoint"": ""bye.aspx"",
        ""EndpointName"": ""auth"",
        ""ReverseProxy"": {
            ""Routes"": {
                ""apiroute"": {
                    ""ClusterId"": ""apicluster"",
                    ""Match"": {
                        ""Path"": ""/api/{*any}""
                    }
                }
            },
            ""Clusters"": {
                ""apicluster"": {
                    ""Destinations"": {
                        ""api/node1"": {
                            ""Address"": ""http://localhost:8080/""
                        }
                    }
                }
            }
        }
    }
    ";
    
    private readonly OidcProxyConfig? _deserializedObject;
    
    public OpenIdConnectBffConfigurationTests()
    {
        _deserializedObject = JsonConvert.DeserializeObject<OidcProxyConfig>(json);
    }
    
    [Fact]
    public void ItShouldReadOidcProperties()
    {
        _deserializedObject?.Oidc.ClientId.Should().NotBeNullOrEmpty();
        _deserializedObject?.Oidc.ClientSecret.Should().NotBeNullOrEmpty();
        _deserializedObject?.Oidc.Authority.Should().NotBeNullOrEmpty();
        _deserializedObject?.Oidc.DiscoveryEndpoint.Should().NotBeNullOrEmpty();
        _deserializedObject?.Oidc.Scopes.Should().NotBeNullOrEmpty();
        _deserializedObject?.Oidc.PostLogoutRedirectEndpoint.Should().NotBeNullOrEmpty();
        _deserializedObject?.Oidc.DisablePushedAuthorization.Should().BeTrue();
        _deserializedObject?.EndpointName.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public void ItShouldReadBffProperties()
    {
        _deserializedObject?.ErrorPage.Should().NotBeNullOrEmpty();
        _deserializedObject?.LandingPage.Should().NotBeNullOrEmpty();
        _deserializedObject?.NameClaim.Should().NotBeNullOrEmpty();
        _deserializedObject?.RoleClaim.Should().NotBeNullOrEmpty();
        _deserializedObject?.HostName.Should().NotBeNull();
        _deserializedObject?.CookieName.Should().NotBeNullOrEmpty();
        _deserializedObject?.SessionIdleTimeout.Should().NotBe(TimeSpan.Zero);
    }
    
    [Fact]
    public void ItShouldReadClusters()
    {
        var clusters = _deserializedObject!.ReverseProxy!.Clusters.ToClusterConfig();
        clusters.Should().NotBeEmpty();

        var cluster = clusters.First();
        cluster.ClusterId.Should().Be("apicluster");
        cluster.Destinations.Should().NotBeEmpty();
    }

    [Fact]
    public void ItShouldReadRoutes()
    {
        var routes = _deserializedObject!.ReverseProxy!.Routes.ToRouteConfig();
        routes.Should().NotBeEmpty();

        var route = routes.First();
        route.RouteId.Should().Be("apiroute");
        route.Match.Path.Should().Be("/api/{*any}");
        route.ClusterId.Should().Be("apicluster");
    }
}