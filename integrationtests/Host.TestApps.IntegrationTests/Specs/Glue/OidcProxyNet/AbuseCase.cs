namespace Host.TestApps.IntegrationTests.Specs.Glue.OidcProxyNet;

[Flags]
public enum AbuseCase
{
    TamperedPayload = 1,
    
    ChangedAlgorithm = 2,
    
    RemovedHeader = 4,
    
    TrailingDots = 8
}