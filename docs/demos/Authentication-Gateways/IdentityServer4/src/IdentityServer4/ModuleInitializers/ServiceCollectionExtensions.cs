using System.Security.Cryptography.X509Certificates;
using System.Text;
using IdentityModel;
using IdentityServer4;
using Microsoft.IdentityModel.Tokens;

namespace TestIdentityServer.ModuleInitializers;

public static class ServiceCollectionExtensions
{
    public static IIdentityServerBuilder AddSigningCredential(this IIdentityServerBuilder builder)
    {
        // create random RS256 key
        //builder.AddDeveloperSigningCredential();

        // use an RSA-based certificate with RS256
        var rsaCert = new X509Certificate2("./Keys/identityserver.test.rsa.p12", "changeit");
        builder.AddSigningCredential(rsaCert, "RS256");

        // ...and PS256
        builder.AddSigningCredential(rsaCert, "PS256");

        // or manually extract ECDSA key from certificate (directly using the certificate is not support by Microsoft right now)
        var ecCert = new X509Certificate2("./Keys/identityserver.test.ecdsa.p12", "changeit");
        var key = new ECDsaSecurityKey(ecCert.GetECDsaPrivateKey())
        {
            KeyId = CryptoRandom.CreateUniqueId(16, CryptoRandom.OutputFormat.Hex)
        };

        return builder.AddSigningCredential(key, IdentityServerConstants.ECDsaSigningAlgorithm.ES256);
    }
    
    public static void AddCertificateForwardingForNginx(this IServiceCollection services)
    {
        services.AddCertificateForwarding(options =>
        {
            options.CertificateHeader = "X-SSL-CERT";

            options.HeaderConverter = (headerValue) =>
            {
                X509Certificate2 clientCertificate = null;

                if(!string.IsNullOrWhiteSpace(headerValue))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(Uri.UnescapeDataString(headerValue));
                    clientCertificate = new X509Certificate2(bytes);
                }

                return clientCertificate;
            };
        });
    }
    
    public static IServiceCollection AddSameSiteCookiePolicy(this IServiceCollection services)
    {
        services.Configure<CookiePolicyOptions>(options =>
        {
            options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
            options.OnAppendCookie = cookieContext => 
                CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
            options.OnDeleteCookie = cookieContext => 
                CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
        });

        return services;
    }
    
    private static void CheckSameSite(HttpContext httpContext, CookieOptions options)
    {
        if (options.SameSite == SameSiteMode.None)
        {
            var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
            if (!httpContext.Request.IsHttps || DisallowsSameSiteNone(userAgent))
            {
                // For .NET Core < 3.1 set SameSite = (SameSiteMode)(-1)
                options.SameSite = SameSiteMode.Unspecified;
            }
        }
    }
    
    private static bool DisallowsSameSiteNone(string userAgent)
    {
        // Cover all iOS based browsers here. This includes:
        // - Safari on iOS 12 for iPhone, iPod Touch, iPad
        // - WkWebview on iOS 12 for iPhone, iPod Touch, iPad
        // - Chrome on iOS 12 for iPhone, iPod Touch, iPad
        // All of which are broken by SameSite=None, because they use the iOS networking stack
        if (userAgent.Contains("CPU iPhone OS 12") || userAgent.Contains("iPad; CPU OS 12"))
        {
            return true;
        }

        // Cover Mac OS X based browsers that use the Mac OS networking stack. This includes:
        // - Safari on Mac OS X.
        // This does not include:
        // - Chrome on Mac OS X
        // Because they do not use the Mac OS networking stack.
        if (userAgent.Contains("Macintosh; Intel Mac OS X 10_14") && 
            userAgent.Contains("Version/") && userAgent.Contains("Safari"))
        {
            return true;
        }

        // Cover Chrome 50-69, because some versions are broken by SameSite=None, 
        // and none in this range require it.
        // Note: this covers some pre-Chromium Edge versions, 
        // but pre-Chromium Edge does not require SameSite=None.
        if (userAgent.Contains("Chrome/5") || userAgent.Contains("Chrome/6"))
        {
            return true;
        }

        return false;
    }
}