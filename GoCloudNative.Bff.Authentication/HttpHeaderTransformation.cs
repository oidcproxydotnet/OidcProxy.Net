using System.Net.Http.Headers;
using GoCloudNative.Bff.Authentication.IdentityProviders;
using GoCloudNative.Bff.Authentication.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace GoCloudNative.Bff.Authentication;

public class HttpHeaderTransformation<T> : ITransformProvider
    where T : IIdentityProvider
{
    private readonly IIdentityProvider _identityProvider;
    private readonly ILogger<HttpHeaderTransformation<T>> _logger;

    public HttpHeaderTransformation(T identityProvider, ILogger<HttpHeaderTransformation<T>> logger)
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
            if (!x.HttpContext.Session.HasAccessToken())
            {
                return;
            }

            var token = x.HttpContext.Session.GetAccessToken();
            
            var factory = new TokenFactory(_identityProvider, x.HttpContext.Session);
            if (await factory.RenewAccessTokenIfExpired())
            {   
                _logger.LogLine(x.HttpContext, new LogLine("Renewed access_token and refresh_token"));
                
                token = x.HttpContext.Session.GetAccessToken();
            }

            x.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        });
    }
}