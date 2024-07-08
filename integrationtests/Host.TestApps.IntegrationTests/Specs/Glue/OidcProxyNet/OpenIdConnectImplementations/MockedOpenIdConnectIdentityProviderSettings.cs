namespace Host.TestApps.IntegrationTests.Specs.Glue.OidcProxyNet.OpenIdConnectImplementations;

public class MockedOpenIdConnectIdentityProviderSettings
{
    public bool WithExpiredToken { get; set; }

    public bool TamperedPayload { get; set; }
    
    public bool AlgorithmChanged { get; set; }
    
    public bool WithNoHeader { get; set; }
    
    public bool WithTrailingDots { get; set; }
}