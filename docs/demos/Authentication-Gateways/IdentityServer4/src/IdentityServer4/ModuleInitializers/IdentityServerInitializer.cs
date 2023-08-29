using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication.Certificate;

namespace TestIdentityServer.ModuleInitializers;

public static class IdentityServerInitializer
{
    
    public static IServiceCollection AddIdentityServer4(this IServiceCollection services)
    {
        services.AddSameSiteCookiePolicy();
        
        services.AddIdentityServer(options =>
            {
                options.Events.RaiseSuccessEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;

                options.EmitScopesAsSpaceDelimitedStringInJwt = true;

                options.MutualTls.Enabled = true;
                options.MutualTls.DomainName = "mtls";
                //options.MutualTls.AlwaysEmitConfirmationClaim = true;
            })
            .AddInMemoryClients(new[] { Configuration.Client })
            .AddInMemoryIdentityResources(Configuration.IdentityResources)
            .AddInMemoryApiScopes(Configuration.ApiScopes)
            .AddSigningCredential()
            .AddJwtBearerClientAuthentication()
            .AddAppAuthRedirectUriValidator()
            .AddTestUsers(TestUsers.Users)
            .AddMutualTlsSecretValidators();
            
        services.AddAuthentication()
            .AddCertificate(options =>
            {
                options.AllowedCertificateTypes = CertificateTypes.All;
                options.RevocationMode = X509RevocationMode.NoCheck;
            });
            
        services.AddCertificateForwardingForNginx();
        
        return services;
    }
}