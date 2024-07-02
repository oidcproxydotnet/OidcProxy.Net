namespace Host.TestApps.IntegrationTests.Specs.Glue.OidcProxyNet.OpenIdConnectImplementations;

public class MockedOpenIdConnectIdentityProviderSettings
{
    public bool WithExpiredToken { get; set; }

    public bool WithTamperedToken { get; set; }
}