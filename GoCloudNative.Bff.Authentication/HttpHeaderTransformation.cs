using System.Net.Http.Headers;
using GoCloudNative.Bff.Authentication.IdentityProviders;
using GoCloudNative.Bff.Authentication.Logging;
using GoCloudNative.Bff.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace GoCloudNative.Bff.Authentication;

internal class HttpHeaderTransformation<TIdp> : ITransformProvider
    where TIdp : IIdentityProvider
{
    private readonly IIdentityProvider _identityProvider;
    private readonly ILogger<HttpHeaderTransformation<TIdp>> _logger;

    public HttpHeaderTransformation(TIdp identityProvider, ILogger<HttpHeaderTransformation<TIdp>> logger)
    {
        _identityProvider = identityProvider;
        _logger = logger;
    }
    
    public void ValidateRoute(TransformRouteValidationContext context)
    {
        // I.L.E.
    }

    public void ValidateCluster(TransformClusterValidationContext context)
    {
        // This applies for all clusters
    }

    public void Apply(TransformBuilderContext context)
    {
        context.AddRequestTransform(async x =>
        {   
            if (!x.HttpContext.Session.HasAccessToken<TIdp>())
            {
                return;
            }

            var token = x.HttpContext.Session.GetAccessToken<TIdp>();
            
            var factory = new TokenFactory(_identityProvider, x.HttpContext.Session);
            if (await factory.RenewAccessTokenIfExpiredAsync<TIdp>())
            {   
                _logger.LogLine(x.HttpContext, new LogLine("Renewed access_token and refresh_token"));
                
                token = x.HttpContext.Session.GetAccessToken<TIdp>();
            }

            x.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        });
    }
}